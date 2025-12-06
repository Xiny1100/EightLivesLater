using UnityEngine;
using System.Collections;

public class FaultyLightFlicker : MonoBehaviour
{
    [Header("Light Components")]
    public Light flickeringLight;
    public Renderer lightBulbRenderer;
    public Material lightOnMaterial;
    public Material lightOffMaterial;

    [Header("Flicker Settings")]
    public FlickerPattern[] patterns;
    public float patternChangeInterval = 5f;
    public bool startAutomatically = true; // NEW: Control when to start

    [Header("Audio")]
    public AudioSource lightAudioSource;
    public AudioClip flickerSound;
    public AudioClip buzzSound;

    private float patternTimer = 0f;
    private int currentPatternIndex = 0;
    private bool isFlickering = false;
    private float baseIntensity;
    private bool scriptEnabled = true; // Track script state separately

    [System.Serializable]
    public class FlickerPattern
    {
        public string name;
        public AnimationCurve intensityCurve;
        public float duration = 1f;
        public float intensityMultiplier = 1f;
    }

    void Start()
    {
        // Try to find light if not assigned
        if (flickeringLight == null)
        {
            flickeringLight = GetComponent<Light>();
            if (flickeringLight == null)
            {
                Debug.LogError("No Light component found on " + gameObject.name);
                enabled = false; // Disable this script
                return;
            }
        }

        // Ensure light is enabled
        flickeringLight.enabled = true;

        // Store initial intensity
        baseIntensity = flickeringLight.intensity;

        // Create default patterns if none assigned
        if (patterns == null || patterns.Length == 0)
            CreateDefaultPatterns();

        // Start flickering if configured
        if (startAutomatically)
        {
            StartFlickering();
        }
    }

    void Update()
    {
        // Don't run if script is disabled
        if (!scriptEnabled) return;

        patternTimer += Time.deltaTime;

        if (patternTimer >= patternChangeInterval)
        {
            ChangePattern();
            patternTimer = 0f;
        }

        if (!isFlickering && patterns.Length > 0)
        {
            StartCoroutine(PlayFlickerPattern(patterns[currentPatternIndex]));
        }
    }

    private IEnumerator PlayFlickerPattern(FlickerPattern pattern)
    {
        isFlickering = true;

        float timer = 0f;
        while (timer < pattern.duration)
        {
            float evaluated = pattern.intensityCurve.Evaluate(timer / pattern.duration);
            flickeringLight.intensity = evaluated * baseIntensity * pattern.intensityMultiplier;

            // Update bulb material if assigned
            if (lightBulbRenderer != null)
            {
                lightBulbRenderer.material = flickeringLight.intensity > 0.1f ? lightOnMaterial : lightOffMaterial;
            }

            timer += Time.deltaTime;
            yield return null;
        }

        isFlickering = false;
    }

    private void ChangePattern()
    {
        if (patterns.Length > 1)
        {
            int newPattern;
            do
            {
                newPattern = Random.Range(0, patterns.Length);
            } while (newPattern == currentPatternIndex && patterns.Length > 1);

            currentPatternIndex = newPattern;

            // Occasionally play sound
            if (lightAudioSource != null && Random.Range(0f, 1f) > 0.7f)
            {
                lightAudioSource.PlayOneShot(buzzSound);
            }
        }
    }

    private void CreateDefaultPatterns()
    {
        patterns = new FlickerPattern[3];

        // Quick flicker pattern
        patterns[0] = new FlickerPattern();
        patterns[0].name = "Quick Flicker";
        patterns[0].duration = 0.5f;
        patterns[0].intensityCurve = new AnimationCurve(
            new Keyframe(0f, 1f), new Keyframe(0.1f, 0.3f), new Keyframe(0.2f, 1f), // Changed 0f to 0.3f
            new Keyframe(0.3f, 0.3f), new Keyframe(0.4f, 1f), new Keyframe(0.5f, 1f)
        );

        // Slow fade pattern
        patterns[1] = new FlickerPattern();
        patterns[1].name = "Slow Fade";
        patterns[1].duration = 2f;
        patterns[1].intensityCurve = new AnimationCurve(
            new Keyframe(0f, 1f), new Keyframe(0.3f, 0.3f), new Keyframe(0.6f, 0.8f),
            new Keyframe(0.8f, 0.2f), new Keyframe(1f, 1f)
        );

        // Random flicker pattern
        patterns[2] = new FlickerPattern();
        patterns[2].name = "Random Flicker";
        patterns[2].duration = 1.5f;
        patterns[2].intensityCurve = new AnimationCurve(
            new Keyframe(0f, 1f), new Keyframe(0.2f, 0.3f), new Keyframe(0.25f, 1f), // Changed 0.1f to 0.3f
            new Keyframe(0.4f, 0.8f), new Keyframe(0.6f, 0.3f), new Keyframe(0.65f, 0.5f), // Changed 0f to 0.3f
            new Keyframe(0.8f, 0.3f), new Keyframe(0.9f, 1f) // Changed 0f to 0.3f
        );
    }

    // Public methods to control flicker
    public void StartFlickering()
    {
        scriptEnabled = true;
        flickeringLight.enabled = true;
        this.enabled = true;
    }

    public void StopFlickering()
    {
        scriptEnabled = false;
        flickeringLight.intensity = baseIntensity;
        flickeringLight.enabled = true; // Keep light enabled

        if (lightBulbRenderer != null && lightOnMaterial != null)
            lightBulbRenderer.material = lightOnMaterial;
    }

    public void SetFlickerIntensity(float multiplier)
    {
        baseIntensity = multiplier;
    }

    // NEW: Simple on/off toggle
    public void ToggleLight()
    {
        if (flickeringLight != null)
        {
            flickeringLight.enabled = !flickeringLight.enabled;
        }
    }
}