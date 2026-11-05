using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

[InitializeOnLoad]
internal static class PinballTextUpgradeOnReload
{
    private const string SessionKey = "PinballTMPUpgradeAppliedV2";

    static PinballTextUpgradeOnReload()
    {
        EditorApplication.delayCall += UpgradeLoadedPinballScene;
    }

    private static void UpgradeLoadedPinballScene()
    {
        if (Application.isPlaying || SessionState.GetBool(SessionKey, false)) return;
        const string scenePath = "Assets/Scenes/PinballComplete.unity";
        Scene scene = SceneManager.GetSceneByPath(scenePath);
        bool openedForUpgrade = !scene.IsValid() || !scene.isLoaded;
        Scene previousActive = SceneManager.GetActiveScene();
        if (openedForUpgrade)
        {
            try { scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive); }
            catch (System.Exception exception)
            {
                Debug.LogError("Could not open the pinball scene for text upgrade: " + exception.Message);
                return;
            }
        }
        if (!scene.IsValid() || !scene.isLoaded) return;
        SceneManager.SetActiveScene(scene);
        SessionState.SetBool(SessionKey, true);
        PinballSceneBuilder.UpgradeSceneTextToTmp();
        if (previousActive.IsValid() && previousActive.isLoaded && previousActive != scene)
            SceneManager.SetActiveScene(previousActive);
        if (openedForUpgrade) EditorSceneManager.CloseScene(scene, true);
    }
}

public static class PinballSceneBuilder
{
    private const string Generated = "Assets/PinballGenerated";
    private const string ScenePath = "Assets/Scenes/PinballComplete.unity";
    private static Material spriteMaterial;

    [MenuItem("Tools/Pinball/Upgrade Scene Text To Sharp TMP")]
    public static void UpgradeSceneTextToTmp()
    {
        const string targetScenePath = "Assets/Scenes/PinballComplete.unity";
        Scene previousActive = SceneManager.GetActiveScene();
        Scene scene = SceneManager.GetSceneByPath(targetScenePath);
        bool openedForUpgrade = !scene.IsValid() || !scene.isLoaded;
        if (openedForUpgrade)
        {
            try { scene = EditorSceneManager.OpenScene(targetScenePath, OpenSceneMode.Additive); }
            catch (System.Exception exception)
            {
                Debug.LogError("Could not open the pinball scene for text upgrade: " + exception.Message);
                return;
            }
        }
        if (!scene.IsValid() || !scene.isLoaded)
        {
            Debug.LogError("Open the pinball scene before upgrading its text.");
            return;
        }
        SceneManager.SetActiveScene(scene);

        TMP_FontAsset font = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
        if (font == null) font = TMP_Settings.defaultFontAsset;
        if (font == null)
        {
            Debug.LogError("TextMeshPro's Liberation Sans SDF font is unavailable.");
            return;
        }

        System.Collections.Generic.List<Text> oldText = new System.Collections.Generic.List<Text>();
        foreach (GameObject root in scene.GetRootGameObjects())
            oldText.AddRange(root.GetComponentsInChildren<Text>(true));
        int converted = 0;
        foreach (Text legacy in oldText)
        {
            if (legacy == null || !legacy.gameObject.scene.IsValid()) continue;
            GameObject oldObject = legacy.gameObject;
            RectTransform oldRect = legacy.rectTransform;
            Transform oldParent = oldObject.transform.parent;
            GameObject replacement = new GameObject(oldObject.name + " (Sharp Text)", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            Undo.RegisterCreatedObjectUndo(replacement, "Upgrade Pinball Text");
            replacement.transform.SetParent(oldParent, false);
            RectTransform newRect = replacement.GetComponent<RectTransform>();
            newRect.anchorMin = oldRect.anchorMin;
            newRect.anchorMax = oldRect.anchorMax;
            newRect.pivot = oldRect.pivot;
            newRect.anchoredPosition = oldRect.anchoredPosition;
            newRect.sizeDelta = oldRect.sizeDelta;
            newRect.localScale = oldRect.localScale;
            newRect.localRotation = oldRect.localRotation;
            TMP_Text upgraded = replacement.GetComponent<TextMeshProUGUI>();
            upgraded.font = font;
            upgraded.text = legacy.text;
            upgraded.fontSize = legacy.fontSize;
            upgraded.color = legacy.color;
            upgraded.alignment = ConvertAlignment(legacy.alignment);
            upgraded.textWrappingMode = TextWrappingModes.Normal;
            upgraded.overflowMode = TextOverflowModes.Overflow;
            upgraded.raycastTarget = legacy.raycastTarget;

            if (legacy.gameObject.name.Contains("SCORE") || legacy.gameObject.name.Contains("BALLS"))
                upgraded.fontSize = Mathf.Max(upgraded.fontSize, 42f);
            else if (legacy.gameObject.name.Contains("A / LEFT"))
                upgraded.fontSize = Mathf.Max(upgraded.fontSize, 26f);

            bool isTextOnlyObject = oldObject.GetComponents<Component>().Length == 3;
            if (isTextOnlyObject) Undo.DestroyObjectImmediate(oldObject);
            else Undo.DestroyObjectImmediate(legacy);
            converted++;
        }

        PinballGameManager manager = null;
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            manager = root.GetComponentInChildren<PinballGameManager>(true);
            if (manager != null) break;
        }
        if (manager != null)
        {
            TMP_Text score = FindTmpText(scene, "Text - SCORE");
            TMP_Text balls = FindTmpText(scene, "Text - BALLS");
            TMP_Text finalScore = FindTmpText(scene, "Text - FINAL SCORE");
            SerializedObject serialized = new SerializedObject(manager);
            serialized.FindProperty("scoreLabel").objectReferenceValue = score;
            serialized.FindProperty("ballsLabel").objectReferenceValue = balls;
            serialized.FindProperty("finalScoreLabel").objectReferenceValue = finalScore;
            serialized.ApplyModifiedProperties();
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("Upgraded " + converted + " UI labels to crisp TextMeshPro SDF text and saved " + scene.path + ".");
        if (previousActive.IsValid() && previousActive.isLoaded && previousActive != scene)
            SceneManager.SetActiveScene(previousActive);
        if (openedForUpgrade) EditorSceneManager.CloseScene(scene, true);
    }

    private static TMP_Text FindTmpText(Scene scene, string namePrefix)
    {
        foreach (GameObject root in scene.GetRootGameObjects())
            foreach (TMP_Text label in root.GetComponentsInChildren<TMP_Text>(true))
                if (label.gameObject.name.StartsWith(namePrefix)) return label;
        return null;
    }

    private static TextAlignmentOptions ConvertAlignment(TextAnchor anchor)
    {
        switch (anchor)
        {
            case TextAnchor.UpperLeft: return TextAlignmentOptions.TopLeft;
            case TextAnchor.UpperCenter: return TextAlignmentOptions.Top;
            case TextAnchor.UpperRight: return TextAlignmentOptions.TopRight;
            case TextAnchor.MiddleLeft: return TextAlignmentOptions.Left;
            case TextAnchor.MiddleRight: return TextAlignmentOptions.Right;
            case TextAnchor.LowerLeft: return TextAlignmentOptions.BottomLeft;
            case TextAnchor.LowerCenter: return TextAlignmentOptions.Bottom;
            case TextAnchor.LowerRight: return TextAlignmentOptions.BottomRight;
            default: return TextAlignmentOptions.Center;
        }
    }

    [MenuItem("Tools/Pinball/Build Complete Game Scene")]
    public static void Build()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        Directory.CreateDirectory(Generated);
        AssetDatabase.Refresh();
        Sprite square = LoadOrCreateSprite("TableTile", false);
        Sprite circle = LoadOrCreateSprite("RoundSprite", true);
        ConfigureSpriteMaterial();

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        BuildCamera();
        BuildBoard(square, circle);
        PinballBall ballPrefab = BuildBallPrefab(circle);
        ExtraBallPickup pickupPrefab = BuildPickupPrefab(circle);
        BuildInterface(ballPrefab, pickupPrefab);

        Directory.CreateDirectory("Assets/Scenes");
        EditorSceneManager.SaveScene(scene, ScenePath);
        EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
        var updated = new System.Collections.Generic.List<EditorBuildSettingsScene> { new EditorBuildSettingsScene(ScenePath, true) };
        foreach (EditorBuildSettingsScene entry in scenes)
            if (entry.path != ScenePath && entry.enabled) updated.Add(entry);
        EditorBuildSettings.scenes = updated.ToArray();
        AssetDatabase.SaveAssets();
        SceneView.lastActiveSceneView?.Frame(new Bounds(Vector3.zero, new Vector3(9f, 14f, 0f)), false);
        Debug.Log("Pinball scene created at " + ScenePath + ". Press Play; Space launches, A/Left and D/Right control flippers.");
    }

    private static void ConfigureSpriteMaterial()
    {
        // The URP 2D sprite shader was rendering the playfield blank with this project's
        // renderer setup. Use Unity's plain sprite shader so the board is visible in play.
        Shader shader = Shader.Find("Sprites/Default");
        if (shader == null) shader = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default");
        if (shader == null) throw new System.InvalidOperationException("No supported sprite shader is available.");
        const string path = Generated + "/PinballUnlit.mat";
        spriteMaterial = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (spriteMaterial == null)
        {
            spriteMaterial = new Material(shader);
            AssetDatabase.CreateAsset(spriteMaterial, path);
        }
        else spriteMaterial.shader = shader;
        EditorUtility.SetDirty(spriteMaterial);
    }

    private static Sprite LoadOrCreateSprite(string name, bool circle)
    {
        string path = Generated + "/" + name + ".png";
        if (!File.Exists(path))
        {
            const int size = 128;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var pixels = new Color32[size * size];
            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                bool inside = !circle || (x - 63.5f) * (x - 63.5f) + (y - 63.5f) * (y - 63.5f) <= 62f * 62f;
                pixels[y * size + x] = inside ? new Color32(255, 255, 255, 255) : new Color32(255, 255, 255, 0);
            }
            texture.SetPixels32(pixels);
            texture.Apply();
            File.WriteAllBytes(path, texture.EncodeToPNG());
            Object.DestroyImmediate(texture);
            AssetDatabase.ImportAsset(path);
        }
        var importer = (TextureImporter)AssetImporter.GetAtPath(path);
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = 100;
        importer.alphaIsTransparency = true;
        importer.filterMode = FilterMode.Bilinear;
        importer.SaveAndReimport();
        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (sprite == null) throw new System.InvalidOperationException("Could not import the generated sprite at " + path);
        return sprite;
    }

    private static void BuildCamera()
    {
        var go = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
        go.tag = "MainCamera";
        go.transform.position = new Vector3(0f, 0f, -10f);
        Camera camera = go.GetComponent<Camera>();
        camera.orthographic = true;
        camera.orthographicSize = 7.25f;
        camera.rect = new Rect(0f, 0f, 1f, 1f);
        camera.backgroundColor = new Color(0.025f, 0.035f, 0.09f);
        camera.clearFlags = CameraClearFlags.SolidColor;
    }

    private static void BuildBoard(Sprite square, Sprite circle)
    {
        MakeSprite("Deep Blue Playfield", square, new Vector2(0f, 0f), new Vector2(7.8f, 12.5f), new Color(0.035f, 0.07f, 0.17f), -10);
        // Framing rails and table walls share a vivid cyan finish.
        Wall("Left Border", square, new Vector2(-4f, 0f), new Vector2(0.22f, 12.2f), new Color(0.1f, 0.85f, 1f));
        Wall("Right Border", square, new Vector2(4f, 0f), new Vector2(0.22f, 12.2f), new Color(0.1f, 0.85f, 1f));
        Wall("Top Border Left", square, new Vector2(-0.78f, 6f), new Vector2(6.64f, 0.22f), new Color(0.1f, 0.85f, 1f));
        Wall("Top Border Right", square, new Vector2(3.99f, 6f), new Vector2(0.22f, 0.22f), new Color(0.1f, 0.85f, 1f));
        Wall("Launch Lane Divider", square, new Vector2(2.55f, -0.1f), new Vector2(0.14f, 11.4f), new Color(0.12f, 0.52f, 0.88f));

        MakeSprite("Lane Glow", square, new Vector2(3.25f, 0.1f), new Vector2(1.25f, 11.5f), new Color(0.04f, 0.12f, 0.23f), -5);
        MakeSprite("Table Center Glow", square, new Vector2(-0.6f, 1.8f), new Vector2(0.055f, 8.1f), new Color(0.05f, 0.35f, 0.53f, 0.55f), -4);
        Wall("Left Inlane", square, new Vector2(-2.75f, -4.75f), new Vector2(2.25f, 0.16f), new Color(0.16f, 0.6f, 0.92f), -24f);
        Wall("Right Inlane", square, new Vector2(1.45f, -4.75f), new Vector2(2.25f, 0.16f), new Color(0.16f, 0.6f, 0.92f), 24f);
        Wall("Left Drain Rail", square, new Vector2(-2.7f, -5.45f), new Vector2(2.3f, 0.18f), new Color(0.08f, 0.72f, 0.9f), 14f);
        Wall("Right Drain Rail", square, new Vector2(2.15f, -5.45f), new Vector2(2.3f, 0.18f), new Color(0.08f, 0.72f, 0.9f), -14f);

        var drain = new GameObject("Drain", typeof(BoxCollider2D), typeof(PinballDrain));
        drain.transform.position = new Vector3(0.1f, -6.25f, 0f);
        drain.GetComponent<BoxCollider2D>().size = new Vector2(2.4f, 0.45f);
        drain.GetComponent<BoxCollider2D>().isTrigger = true;

        Bumper("Top Bumper", circle, new Vector2(-0.65f, 3.3f), 0);
        Bumper("Left Bumper", circle, new Vector2(-2.15f, 1.15f), 1);
        Bumper("Right Bumper", circle, new Vector2(1.05f, 1.15f), 2);
        Bumper("Center Bumper", circle, new Vector2(-0.55f, -1.15f), 1);

        MakeFlipper("Left Flipper", square, new Vector2(-1.35f, -4.65f), false, Key.A, Key.LeftArrow, -2f, 38f, new Vector2(0.22f, 1f));
        MakeFlipper("Right Flipper", square, new Vector2(1.35f, -4.65f), true, Key.D, Key.RightArrow, 182f, 142f, new Vector2(-0.22f, 1f));

        var launchPoint = new GameObject("Ball Launch Point");
        launchPoint.transform.position = new Vector3(3.25f, -5.45f, 0f);
        MakeSprite("Launch Guide", square, new Vector2(3.25f, -4.5f), new Vector2(0.045f, 1.1f), new Color(1f, 0.65f, 0.13f), 1);
    }

    private static void Wall(string name, Sprite sprite, Vector2 position, Vector2 size, Color color, float angle = 0f)
    {
        GameObject go = MakeSprite(name, sprite, position, size, color, 2);
        go.transform.rotation = Quaternion.Euler(0f, 0f, angle);
        go.AddComponent<BoxCollider2D>().size = Vector2.one;
    }

    private static GameObject MakeSprite(string name, Sprite sprite, Vector2 position, Vector2 size, Color color, int order)
    {
        var go = new GameObject(name, typeof(SpriteRenderer));
        go.transform.position = position;
        go.transform.localScale = new Vector3(size.x, size.y, 1f);
        SpriteRenderer renderer = go.GetComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.sharedMaterial = spriteMaterial;
        renderer.color = color;
        renderer.sortingOrder = order;
        return go;
    }

    private static void Bumper(string name, Sprite circle, Vector2 position, int variant)
    {
        Color[] colors = { new Color(1f, 0.28f, 0.32f), new Color(1f, 0.68f, 0.12f), new Color(0.25f, 0.95f, 0.78f) };
        GameObject go = MakeSprite(name, circle, position, new Vector2(0.9f, 0.9f), colors[variant], 5);
        CircleCollider2D collider = go.AddComponent<CircleCollider2D>();
        collider.isTrigger = true;
        go.AddComponent<PinballBumper>();
    }

    private static void MakeFlipper(string name, Sprite square, Vector2 hinge, bool right, Key key, Key alternateKey, float rest, float pressed, Vector2 kick)
    {
        var root = new GameObject(name, typeof(Rigidbody2D), typeof(PinballFlipper));
        root.transform.position = hinge;
        Rigidbody2D body = root.GetComponent<Rigidbody2D>();
        body.bodyType = RigidbodyType2D.Kinematic;
        body.gravityScale = 0f;
        PinballFlipper flipper = root.GetComponent<PinballFlipper>();
        Set(flipper, "key", key);
        Set(flipper, "alternateKey", alternateKey);
        Set(flipper, "restAngle", rest);
        Set(flipper, "pressedAngle", pressed);
        Set(flipper, "kickDirection", kick);
        Set(flipper, "kickImpulse", 12f);
        var blade = new GameObject(name + " Blade", typeof(SpriteRenderer), typeof(BoxCollider2D));
        blade.transform.SetParent(root.transform, false);
        blade.transform.localPosition = new Vector3(0.72f, 0f, 0f);
        blade.transform.localScale = new Vector3(1.5f, 0.24f, 1f);
        SpriteRenderer renderer = blade.GetComponent<SpriteRenderer>();
        renderer.sprite = square;
        renderer.sharedMaterial = spriteMaterial;
        renderer.color = right ? new Color(1f, 0.33f, 0.58f) : new Color(1f, 0.33f, 0.58f);
        renderer.sortingOrder = 8;
        blade.GetComponent<BoxCollider2D>().size = Vector2.one;
    }

    private static PinballBall BuildBallPrefab(Sprite circle)
    {
        var go = new GameObject("Pinball Ball", typeof(SpriteRenderer), typeof(CircleCollider2D), typeof(Rigidbody2D), typeof(PinballBall));
        go.GetComponent<SpriteRenderer>().sprite = circle;
        go.GetComponent<SpriteRenderer>().sharedMaterial = spriteMaterial;
        go.GetComponent<SpriteRenderer>().color = new Color(0.9f, 0.97f, 1f);
        go.GetComponent<SpriteRenderer>().sortingOrder = 10;
        Set(go.GetComponent<PinballBall>(), "launchImpulse", 14f);
        Set(go.GetComponent<PinballBall>(), "maxSpeed", 24f);
        go.transform.localScale = Vector3.one * 0.32f;
        CircleCollider2D collider = go.GetComponent<CircleCollider2D>();
        collider.radius = 0.5f;
        Rigidbody2D body = go.GetComponent<Rigidbody2D>();
        body.gravityScale = 1f;
        body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        string path = Generated + "/PinballBall.prefab";
        PinballBall prefab = PrefabUtility.SaveAsPrefabAsset(go, path).GetComponent<PinballBall>();
        Object.DestroyImmediate(go);
        return prefab;
    }

    private static ExtraBallPickup BuildPickupPrefab(Sprite circle)
    {
        var go = new GameObject("Extra Ball Pickup", typeof(SpriteRenderer), typeof(CircleCollider2D), typeof(ExtraBallPickup));
        go.GetComponent<SpriteRenderer>().sprite = circle;
        go.GetComponent<SpriteRenderer>().sharedMaterial = spriteMaterial;
        go.GetComponent<SpriteRenderer>().color = new Color(0.4f, 1f, 0.42f);
        go.GetComponent<SpriteRenderer>().sortingOrder = 9;
        CircleCollider2D collider = go.GetComponent<CircleCollider2D>();
        collider.isTrigger = true;
        string path = Generated + "/ExtraBallPickup.prefab";
        ExtraBallPickup prefab = PrefabUtility.SaveAsPrefabAsset(go, path).GetComponent<ExtraBallPickup>();
        Object.DestroyImmediate(go);
        return prefab;
    }

    private static void BuildInterface(PinballBall ballPrefab, ExtraBallPickup pickupPrefab)
    {
        var managerObject = new GameObject("Pinball Game Manager", typeof(PinballGameManager), typeof(ExtraBallSpawner));
        PinballGameManager manager = managerObject.GetComponent<PinballGameManager>();
        ExtraBallSpawner spawner = managerObject.GetComponent<ExtraBallSpawner>();
        Set(spawner, "pickupPrefab", pickupPrefab);
        Set(spawner, "lowerLeft", new Vector2(-3.1f, -2.8f));
        Set(spawner, "upperRight", new Vector2(2f, 4.8f));

        GameObject launchPointObject = GameObject.Find("Ball Launch Point");
        Canvas canvas = new GameObject("Game UI", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster)).GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080f, 1920f);
        scaler.matchWidthOrHeight = 0.5f;
        var eventObject = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));

        GameObject menu = Panel(canvas.transform, "Welcome and Controls", new Color(0.018f, 0.035f, 0.09f, 0.94f));
        CreateText(menu.transform, "WELCOME TO PINBALL", 76, new Vector2(0.5f, 0.68f), new Vector2(900, 150), new Color(0.1f, 0.85f, 1f), TextAnchor.MiddleCenter);
        CreateText(menu.transform, "A / LEFT ARROW   Left flipper\nD / RIGHT ARROW   Right flipper\nSPACE   Launch the ball\nHit bumpers to score. Keep the ball above the drain!", 36, new Vector2(0.5f, 0.51f), new Vector2(900, 300), Color.white, TextAnchor.MiddleCenter);
        Button start = CreateButton(menu.transform, "START GAME", new Vector2(0.5f, 0.31f), new Color(0.95f, 0.24f, 0.55f));
        start.onClick.AddListener(manager.StartGame);

        GameObject hud = Panel(canvas.transform, "HUD", new Color(0f, 0f, 0f, 0f));
        TMP_Text score = CreateText(hud.transform, "SCORE  0", 42, new Vector2(0.36f, 0.97f), new Vector2(420, 90), Color.white, TextAnchor.MiddleCenter);
        TMP_Text balls = CreateText(hud.transform, "BALLS  3", 42, new Vector2(0.64f, 0.97f), new Vector2(420, 90), new Color(1f, 0.78f, 0.2f), TextAnchor.MiddleCenter);
        CreateText(hud.transform, "A / LEFT ARROW     D / RIGHT ARROW     SPACE / LAUNCH", 26, new Vector2(0.5f, 0.045f), new Vector2(900, 90), Color.white, TextAnchor.MiddleCenter);

        GameObject defeat = Panel(canvas.transform, "Defeat Screen", new Color(0.018f, 0.035f, 0.09f, 0.97f));
        CreateText(defeat.transform, "GAME OVER", 86, new Vector2(0.5f, 0.67f), new Vector2(900, 150), new Color(1f, 0.35f, 0.55f), TextAnchor.MiddleCenter);
        TMP_Text finalScore = CreateText(defeat.transform, "FINAL SCORE  0", 48, new Vector2(0.5f, 0.53f), new Vector2(900, 100), Color.white, TextAnchor.MiddleCenter);
        Button restart = CreateButton(defeat.transform, "PLAY AGAIN", new Vector2(0.5f, 0.36f), new Color(0.05f, 0.72f, 0.84f));

        restart.onClick.AddListener(manager.StartGame);
        Set(manager, "ballPrefab", ballPrefab);
        Set(manager, "launchPoint", launchPointObject.transform);
        Set(manager, "menuPanel", menu);
        Set(manager, "hudPanel", hud);
        Set(manager, "defeatPanel", defeat);
        Set(manager, "scoreLabel", score);
        Set(manager, "ballsLabel", balls);
        Set(manager, "finalScoreLabel", finalScore);
        Set(manager, "startingBalls", 3);
        menu.SetActive(true);
        hud.SetActive(false);
        defeat.SetActive(false);
    }

    private static GameObject Panel(Transform parent, string name, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(parent, false);
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        go.GetComponent<Image>().color = color;
        return go;
    }

    private static TMP_Text CreateText(Transform parent, string content, int size, Vector2 anchor, Vector2 dimensions, Color color, TextAnchor alignment)
    {
        var go = new GameObject("Text - " + content, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = anchor;
        rect.sizeDelta = dimensions;
        rect.anchoredPosition = Vector2.zero;
        TMP_Text text = go.GetComponent<TMP_Text>();
        text.font = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
        if (text.font == null) text.font = TMP_Settings.defaultFontAsset;
        text.text = content;
        text.fontSize = size;
        text.color = color;
        text.alignment = alignment == TextAnchor.MiddleCenter ? TextAlignmentOptions.Center : TextAlignmentOptions.TopLeft;
        text.textWrappingMode = TextWrappingModes.Normal;
        text.overflowMode = TextOverflowModes.Overflow;
        return text;
    }

    private static Button CreateButton(Transform parent, string label, Vector2 anchor, Color color)
    {
        var go = new GameObject(label + " Button", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = anchor;
        rect.sizeDelta = new Vector2(440, 120);
        rect.anchoredPosition = Vector2.zero;
        Image image = go.GetComponent<Image>();
        image.color = color;
        Button button = go.GetComponent<Button>();
        button.targetGraphic = image;
        CreateText(go.transform, label, 38, new Vector2(0.5f, 0.5f), new Vector2(420, 100), Color.white, TextAnchor.MiddleCenter);
        return button;
    }

    private static void Set(Object target, string property, object value)
    {
        var serialized = new SerializedObject(target);
        SerializedProperty field = serialized.FindProperty(property);
        if (field == null) return;
        if (value is Object objectValue) field.objectReferenceValue = objectValue;
        else if (value is int intValue) field.intValue = intValue;
        else if (value is float floatValue) field.floatValue = floatValue;
        else if (value is Vector2 vectorValue) field.vector2Value = vectorValue;
        else if (value is Key keyValue) field.intValue = (int)keyValue;
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }
}





