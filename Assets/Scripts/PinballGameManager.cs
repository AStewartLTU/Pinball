using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public sealed class PinballGameManager : MonoBehaviour
{
    [SerializeField] private PinballBall ballPrefab;
    [SerializeField] private Transform launchPoint;
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private GameObject hudPanel;
    [SerializeField] private GameObject defeatPanel;
    [SerializeField] private TMP_Text scoreLabel;
    [SerializeField] private TMP_Text ballsLabel;
    [SerializeField] private TMP_Text finalScoreLabel;
    [SerializeField, Min(1)] private int startingBalls = 3;

    private int score;
    private int ballsLeft;
    private PinballBall currentBall;
    private bool respawnPending;
    public bool IsPlaying { get; private set; }

    private void Awake()
    {
        Time.timeScale = 1f;
        if (menuPanel == null) menuPanel = CreateWelcomeMenu();
        IsPlaying = false;
        SetScreens(true, false, false);
    }

    private void Update()
    {
        if (!IsPlaying && menuPanel != null && menuPanel.activeSelf &&
            Keyboard.current != null && Keyboard.current.enterKey.wasPressedThisFrame)
            StartGame();
    }

    public void StartGame()
    {
        Time.timeScale = 1f;
        if (currentBall != null) Destroy(currentBall.gameObject);
        currentBall = null;
        respawnPending = false;
        score = 0;
        ballsLeft = startingBalls;
        IsPlaying = true;
        SetScreens(false, true, false);
        UpdateHud();
        SpawnBall();
    }

    public void AddScore(int points)
    {
        if (!IsPlaying) return;
        score += Mathf.Max(0, points);
        UpdateHud();
    }

    public void DrainBall(PinballBall ball)
    {
        if (!IsPlaying || respawnPending) return;
        if (ball == null) ball = currentBall;
        if (ball != null && currentBall != null && ball != currentBall) return;

        respawnPending = true;
        if (ball != null) Destroy(ball.gameObject);
        if (currentBall == ball) currentBall = null;
        ballsLeft = Mathf.Max(0, ballsLeft - 1);
        UpdateHud();
        if (ballsLeft == 0)
        {
            IsPlaying = false;
            respawnPending = false;
            if (finalScoreLabel != null) finalScoreLabel.text = "FINAL SCORE  " + score;
            SetScreens(false, false, true);
        }
        else StartCoroutine(RespawnAfterDrain());
    }

    public void GiveExtraBall()
    {
        if (!IsPlaying) return;
        ballsLeft++;
        UpdateHud();
    }

    private void SpawnBall()
    {
        if (currentBall != null) return;
        if (ballPrefab == null || launchPoint == null)
        {
            Debug.LogError("PinballGameManager needs a ball prefab and launch point.", this);
            return;
        }
        currentBall = Instantiate(ballPrefab, launchPoint.position, Quaternion.identity);
        currentBall.Initialize(this);
    }

    private IEnumerator RespawnAfterDrain()
    {
        // Let Unity finish destroying the drained ball before creating its replacement.
        yield return new WaitForSecondsRealtime(0.25f);
        respawnPending = false;
        if (IsPlaying && currentBall == null) SpawnBall();
    }

    private GameObject CreateWelcomeMenu()
    {
        GameObject canvasObject = new GameObject("Pinball Welcome UI", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 50;
        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080f, 1920f);
        scaler.matchWidthOrHeight = 0.5f;

        EventSystem eventSystem = EventSystem.current;
        if (eventSystem == null)
            eventSystem = new GameObject("Pinball EventSystem", typeof(EventSystem)).GetComponent<EventSystem>();
        StandaloneInputModule legacyInput = eventSystem.GetComponent<StandaloneInputModule>();
        if (legacyInput != null) legacyInput.enabled = false;
        if (eventSystem.GetComponent<InputSystemUIInputModule>() == null)
            eventSystem.gameObject.AddComponent<InputSystemUIInputModule>();

        GameObject panel = new GameObject("Welcome and Controls", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        panel.transform.SetParent(canvas.transform, false);
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;
        panel.GetComponent<Image>().color = new Color(0.018f, 0.035f, 0.09f, 0.96f);

        MakeMenuText(panel.transform, "WELCOME TO PINBALL", 76, new Vector2(0.5f, 0.68f), new Vector2(900f, 150f), new Color(0.1f, 0.85f, 1f));
        MakeMenuText(panel.transform, "A / LEFT ARROW   Left flipper\nD / RIGHT ARROW   Right flipper\nSPACE   Launch the ball\nHit bumpers to score. Keep the ball above the drain!\nPress ENTER or click below to start.", 34, new Vector2(0.5f, 0.48f), new Vector2(920f, 400f), Color.white);

        GameObject buttonObject = new GameObject("START GAME Button", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(panel.transform, false);
        RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
        buttonRect.anchorMin = buttonRect.anchorMax = new Vector2(0.5f, 0.29f);
        buttonRect.sizeDelta = new Vector2(440f, 120f);
        buttonRect.anchoredPosition = Vector2.zero;
        Image buttonImage = buttonObject.GetComponent<Image>();
        buttonImage.color = new Color(0.95f, 0.24f, 0.55f);
        Button button = buttonObject.GetComponent<Button>();
        button.targetGraphic = buttonImage;
        button.onClick.AddListener(StartGame);
        MakeMenuText(buttonObject.transform, "START GAME", 38, new Vector2(0.5f, 0.5f), new Vector2(420f, 100f), Color.white);
        return panel;
    }

    private static TMP_Text MakeMenuText(Transform parent, string content, int size, Vector2 anchor, Vector2 dimensions, Color color)
    {
        GameObject textObject = new GameObject("Text - " + content, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(parent, false);
        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = anchor;
        rect.sizeDelta = dimensions;
        rect.anchoredPosition = Vector2.zero;
        TMP_Text text = textObject.GetComponent<TMP_Text>();
        text.font = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
        if (text.font == null) text.font = TMP_Settings.defaultFontAsset;
        text.text = content;
        text.fontSize = size;
        text.color = color;
        text.alignment = TextAlignmentOptions.Center;
        text.textWrappingMode = TextWrappingModes.Normal;
        text.overflowMode = TextOverflowModes.Overflow;
        return text;
    }

    private void SetScreens(bool menu, bool hud, bool defeat)
    {
        if (menuPanel != null) menuPanel.SetActive(menu);
        if (hudPanel != null) hudPanel.SetActive(hud);
        if (defeatPanel != null) defeatPanel.SetActive(defeat);
    }

    private void UpdateHud()
    {
        if (scoreLabel != null) scoreLabel.text = "SCORE  " + score;
        if (ballsLabel != null) ballsLabel.text = "BALLS  " + ballsLeft;
    }
}

