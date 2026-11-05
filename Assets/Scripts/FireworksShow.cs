using System.Collections;
using UnityEngine;

public sealed class FireworksShow : MonoBehaviour
{
    [SerializeField, Min(0.5f)] private float minimumInterval = 1.8f;
    [SerializeField, Min(0.5f)] private float maximumInterval = 3.2f;
    [SerializeField] private float horizontalSpread = 4.1f;
    [SerializeField] private float launchHeight = -4.2f;
    [SerializeField] private float burstHeight = 1.5f;
    [SerializeField] private int burstParticles = 95;
    [SerializeField] private AudioClip whistleSound;
    [SerializeField] private AudioClip bangSound;

    private Material particleMaterial;
    private AudioClip whistleClip;
    private AudioClip bangClip;
    private static readonly Color[] Colors =
    {
        new Color(1f, 0.22f, 0.12f), new Color(0.2f, 0.8f, 1f),
        new Color(1f, 0.72f, 0.12f), new Color(0.72f, 0.3f, 1f),
        new Color(0.2f, 1f, 0.55f), new Color(1f, 0.35f, 0.66f)
    };

    private void Start()
    {
        Camera camera = Camera.main;
        if (camera != null)
        {
            camera.orthographic = true;
            camera.orthographicSize = 5f;
            camera.transform.position = new Vector3(0f, 0f, -10f);
            camera.backgroundColor = new Color(0.018f, 0.025f, 0.09f);
            camera.clearFlags = CameraClearFlags.SolidColor;
        }

        particleMaterial = CreateParticleMaterial();
        whistleClip = whistleSound != null ? whistleSound : CreateWhistle();
        bangClip = bangSound != null ? bangSound : CreateBang();
        StartCoroutine(FireworkLoop());
    }

    private void OnDrawGizmos()
    {
        Color previousColor = Gizmos.color;
        const int previewCount = 5;
        for (int i = 0; i < previewCount; i++)
        {
            float x = Mathf.Lerp(-horizontalSpread, horizontalSpread, i / (float)(previewCount - 1));
            float y = burstHeight + (i % 2) * 0.85f;
            Vector3 launch = new Vector3(x, launchHeight, 0f);
            Vector3 burst = new Vector3(x, y, 0f);
            Color color = Colors[i % Colors.Length];

            Gizmos.color = new Color(color.r, color.g, color.b, 0.8f);
            Gizmos.DrawLine(launch, burst);
            Gizmos.DrawSphere(launch, 0.07f);
            for (int ray = 0; ray < 12; ray++)
            {
                float angle = ray * Mathf.PI * 2f / 12f;
                Vector3 direction = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f);
                Gizmos.DrawLine(burst + direction * 0.16f, burst + direction * 0.78f);
            }
            Gizmos.DrawWireSphere(burst, 0.82f);
        }
        Gizmos.color = previousColor;
    }
    private IEnumerator FireworkLoop()
    {
        yield return new WaitForSeconds(0.7f);
        while (true)
        {
            LaunchFirework();
            yield return new WaitForSeconds(Random.Range(minimumInterval, maximumInterval));
        }
    }

    private void LaunchFirework()
    {
        float x = Random.Range(-horizontalSpread, horizontalSpread);
        float targetY = Random.Range(burstHeight - 0.45f, burstHeight + 1.25f);
        Color color = Colors[Random.Range(0, Colors.Length)];
        Vector3 start = new Vector3(x, launchHeight, 0f);
        Vector3 end = new Vector3(x + Random.Range(-0.6f, 0.6f), targetY, 0f);
        float flightTime = Random.Range(1.15f, 1.65f);

        GameObject rocket = new GameObject("Firework Rocket");
        rocket.transform.position = start;
        ParticleSystem trail = rocket.AddComponent<ParticleSystem>();
        ConfigureTrail(trail, color, end, flightTime);
        AudioSource.PlayClipAtPoint(whistleClip, start, 0.45f);
        StartCoroutine(ExplodeAfterFlight(rocket, end, color, flightTime));
    }

    private void ConfigureTrail(ParticleSystem ps, Color color, Vector3 end, float flightTime)
    {
        var main = ps.main;
        main.loop = false;
        main.duration = flightTime;
        main.startLifetime = 0.38f;
        main.startSpeed = 0f;
        main.startSize = 0.09f;
        main.startColor = color;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles = 500;
        var emission = ps.emission;
        emission.rateOverTime = 75f;
        var shape = ps.shape;
        shape.enabled = false;
        var velocity = ps.velocityOverLifetime;
        velocity.enabled = true;
        velocity.space = ParticleSystemSimulationSpace.World;
        velocity.x = (end.x - ps.transform.position.x) / flightTime;
        velocity.y = (end.y - ps.transform.position.y) / flightTime;
        var renderer = ps.GetComponent<ParticleSystemRenderer>();
        renderer.material = particleMaterial;
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        ps.Play();
    }

    private IEnumerator ExplodeAfterFlight(GameObject rocket, Vector3 position, Color color, float flightTime)
    {
        yield return new WaitForSeconds(flightTime);
        if (rocket != null) Destroy(rocket);
        AudioSource.PlayClipAtPoint(bangClip, position, 0.8f);
        CreateBurst(position, color);
    }

    private void CreateBurst(Vector3 position, Color color)
    {
        GameObject burstObject = new GameObject("Firework Burst");
        burstObject.transform.position = position;
        ParticleSystem burst = burstObject.AddComponent<ParticleSystem>();
        var main = burst.main;
        main.loop = false;
        main.duration = 1.8f;
        main.startLifetime = new ParticleSystem.MinMaxCurve(1.1f, 2.1f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(3.2f, 7f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.045f, 0.12f);
        main.startColor = color;
        main.gravityModifier = 0.62f;
        main.maxParticles = burstParticles + 30;
        var emission = burst.emission;
        emission.rateOverTime = 0f;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)burstParticles) });
        var shape = burst.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.12f;
        var colorOverLifetime = burst.colorOverLifetime;
        colorOverLifetime.enabled = true;
        var gradient = new Gradient();
        gradient.SetKeys(
            new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(color, 0.2f), new GradientColorKey(color * 0.35f, 1f) },
            new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 0.65f), new GradientAlphaKey(0f, 1f) });
        colorOverLifetime.color = gradient;
        var renderer = burst.GetComponent<ParticleSystemRenderer>();
        renderer.material = particleMaterial;
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        burst.Play();
        Destroy(burstObject, 3.5f);

        // A second, finer sparkle layer makes the burst read clearly against the night sky.
        GameObject sparkleObject = new GameObject("Firework Sparkles");
        sparkleObject.transform.position = position;
        ParticleSystem sparkles = sparkleObject.AddComponent<ParticleSystem>();
        var sparkleMain = sparkles.main;
        sparkleMain.loop = false;
        sparkleMain.duration = 0.5f;
        sparkleMain.startLifetime = new ParticleSystem.MinMaxCurve(0.45f, 0.9f);
        sparkleMain.startSpeed = new ParticleSystem.MinMaxCurve(1.3f, 4.5f);
        sparkleMain.startSize = new ParticleSystem.MinMaxCurve(0.025f, 0.055f);
        sparkleMain.startColor = Color.white;
        sparkleMain.gravityModifier = 0.45f;
        sparkleMain.maxParticles = 120;
        var sparkleEmission = sparkles.emission;
        sparkleEmission.rateOverTime = 0f;
        sparkleEmission.SetBursts(new[] { new ParticleSystem.Burst(0f, 35) });
        var sparkleShape = sparkles.shape;
        sparkleShape.shapeType = ParticleSystemShapeType.Sphere;
        sparkleShape.radius = 0.08f;
        sparkles.GetComponent<ParticleSystemRenderer>().material = particleMaterial;
        sparkles.Play();
        Destroy(sparkleObject, 2f);
    }

    private Material CreateParticleMaterial()
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
        if (shader == null) shader = Shader.Find("Particles/Standard Unlit");
        if (shader == null) shader = Shader.Find("Sprites/Default");
        return new Material(shader) { name = "Runtime Firework Glow" };
    }

    private AudioClip CreateWhistle()
    {
        const int rate = 22050;
        int count = rate;
        float[] samples = new float[count];
        float phase = 0f;
        for (int i = 0; i < count; i++)
        {
            float t = (float)i / (count - 1);
            float frequency = Mathf.Lerp(520f, 1450f, t);
            phase += 2f * Mathf.PI * frequency / rate;
            float envelope = Mathf.Sin(Mathf.PI * t);
            samples[i] = (Mathf.Sin(phase) * 0.55f + Mathf.Sin(phase * 2f) * 0.16f) * envelope * 0.42f;
        }
        AudioClip clip = AudioClip.Create("Firework Whistle", count, 1, rate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    private AudioClip CreateBang()
    {
        const int rate = 22050;
        int count = Mathf.RoundToInt(rate * 1.1f);
        float[] samples = new float[count];
        float previousNoise = 0f;
        for (int i = 0; i < count; i++)
        {
            float t = (float)i / rate;
            float decay = Mathf.Exp(-t * 4.2f);
            float noise = Random.Range(-1f, 1f);
            previousNoise = Mathf.Lerp(previousNoise, noise, 0.42f);
            float boom = Mathf.Sin(2f * Mathf.PI * (62f - 25f * t) * t) * Mathf.Exp(-t * 8f);
            samples[i] = Mathf.Clamp((previousNoise * 0.62f + boom * 0.75f) * decay * 0.8f, -1f, 1f);
        }
        AudioClip clip = AudioClip.Create("Firework Bang", count, 1, rate, false);
        clip.SetData(samples, 0);
        return clip;
    }
}


