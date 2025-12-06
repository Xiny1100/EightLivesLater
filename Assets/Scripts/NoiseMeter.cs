using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NoiseMeter : MonoBehaviour
{
    [Header("Noise Meter Settings")]
    public float maxNoiseLevel = 100f;
    public float currentNoiseLevel = 0f;
    public float noiseIncreaseRate = 15f;
    public float noiseDecreaseRate = 8f;
    public float decayDelay = 2f;

    [Header("Noise Sources")]
    public List<HandsDryer> handDryers = new List<HandsDryer>();
    public PlayerMovement playerMovement;

    [Header("GUI Settings")]
    public bool showMeter = true;
    public Vector2 meterPosition = new Vector2(20, 20);
    public Vector2 meterSize = new Vector2(200, 25);
    public Color lowNoiseColor = Color.green;
    public Color mediumNoiseColor = Color.yellow;
    public Color highNoiseColor = Color.red;
    public float mediumThreshold = 40f;
    public float highThreshold = 70f;

    [Header("Audio Detection")]
    public float audioThreshold = 0.1f;

    private bool isMakingNoise = false;
    private float lastNoiseTime = 0f;
    private List<AudioSource> activeNoiseSources = new List<AudioSource>();

    void Start()
    {
        // Auto-find references if not assigned
        if (handDryers.Count == 0)
        {
            HandsDryer[] foundDryers = FindObjectsOfType<HandsDryer>();
            handDryers.AddRange(foundDryers);
            Debug.Log($"Auto-found {foundDryers.Length} hand dryers");
        }

        if (playerMovement == null)
        {
            playerMovement = FindObjectOfType<PlayerMovement>();
            if (playerMovement != null)
                Debug.Log("Auto-found player movement");
        }

        // Initialize noise level
        currentNoiseLevel = 0f;
    }

    void Update()
    {
        CheckForNoise();
        UpdateNoiseLevel();
        ClampNoiseLevel();

        // Debug info
        if (Input.GetKeyDown(KeyCode.N))
        {
            Debug.Log($"Noise Level: {currentNoiseLevel}, Making Noise: {isMakingNoise}, Active Sources: {activeNoiseSources.Count}");
        }
    }

    void CheckForNoise()
    {
        bool previousNoiseState = isMakingNoise;
        isMakingNoise = false;
        activeNoiseSources.Clear();

        // Check hand dryers
        foreach (HandsDryer dryer in handDryers)
        {
            if (dryer != null && dryer.audioSource != null && dryer.audioSource.isPlaying)
            {
                float volume = dryer.audioSource.volume;
                if (volume > audioThreshold)
                {
                    isMakingNoise = true;
                    activeNoiseSources.Add(dryer.audioSource);
                    lastNoiseTime = Time.time;
                }
            }
        }

        // Check player lowering noise
        if (playerMovement != null && playerMovement.isLowering)
        {
            AudioSource playerAudio = playerMovement.GetComponent<AudioSource>();
            if (playerAudio != null && playerAudio.isPlaying)
            {
                float volume = playerAudio.volume;
                if (volume > audioThreshold)
                {
                    isMakingNoise = true;
                    activeNoiseSources.Add(playerAudio);
                    lastNoiseTime = Time.time;
                }
            }
        }

        // Update last noise time if we're making noise
        if (isMakingNoise)
        {
            lastNoiseTime = Time.time;
        }
    }

    void UpdateNoiseLevel()
    {
        if (isMakingNoise)
        {
            // Increase based on number of active sources and their volumes
            float totalVolume = 0f;
            foreach (AudioSource source in activeNoiseSources)
            {
                totalVolume += source.volume;
            }

            float increaseMultiplier = Mathf.Clamp(totalVolume, 0.5f, 2f);
            currentNoiseLevel += noiseIncreaseRate * increaseMultiplier * Time.deltaTime;
        }
        else
        {
            // Check if decay delay has passed
            if (Time.time - lastNoiseTime >= decayDelay)
            {
                currentNoiseLevel -= noiseDecreaseRate * Time.deltaTime;
            }
        }
    }

    void ClampNoiseLevel()
    {
        currentNoiseLevel = Mathf.Clamp(currentNoiseLevel, 0f, maxNoiseLevel);
    }

    // Public methods to manually control noise
    public void AddNoise(float amount)
    {
        currentNoiseLevel = Mathf.Clamp(currentNoiseLevel + amount, 0f, maxNoiseLevel);
        lastNoiseTime = Time.time;
    }

    public void SetNoise(float amount)
    {
        currentNoiseLevel = Mathf.Clamp(amount, 0f, maxNoiseLevel);
        lastNoiseTime = Time.time;
    }

    public void ResetNoise()
    {
        currentNoiseLevel = 0f;
    }

    public float GetNoisePercentage()
    {
        return currentNoiseLevel / maxNoiseLevel;
    }

    public bool IsNoiseCritical()
    {
        return currentNoiseLevel >= highThreshold;
    }

    // GUI Display
    void OnGUI()
    {
        if (!showMeter) return;

        DrawNoiseMeter();
    }

    void DrawNoiseMeter()
    {
        // Background
        Rect bgRect = new Rect(meterPosition.x, meterPosition.y, meterSize.x, meterSize.y);
        GUI.Box(bgRect, "");

        // Fill
        float fillPercent = currentNoiseLevel / maxNoiseLevel;
        Rect fillRect = new Rect(meterPosition.x, meterPosition.y, meterSize.x * fillPercent, meterSize.y);

        // Choose color
        Color fillColor = lowNoiseColor;
        if (currentNoiseLevel >= highThreshold)
            fillColor = highNoiseColor;
        else if (currentNoiseLevel >= mediumThreshold)
            fillColor = mediumNoiseColor;

        // Draw fill
        Texture2D fillTexture = CreateColorTexture(fillColor);
        GUIStyle fillStyle = new GUIStyle();
        fillStyle.normal.background = fillTexture;
        GUI.Box(fillRect, "", fillStyle);

        // Label
        GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
        labelStyle.alignment = TextAnchor.MiddleCenter;
        labelStyle.normal.textColor = Color.white;
        labelStyle.fontStyle = FontStyle.Bold;

        string noiseText = $"NOISE: {currentNoiseLevel:F0}%";
        GUI.Label(bgRect, noiseText, labelStyle);

        // Threshold markers
        DrawThresholdMarkers(bgRect);
    }

    void DrawThresholdMarkers(Rect bgRect)
    {
        // Medium threshold
        float mediumX = bgRect.x + (mediumThreshold / maxNoiseLevel) * meterSize.x;
        Rect mediumMarker = new Rect(mediumX, bgRect.y, 2, bgRect.height);
        GUI.Box(mediumMarker, "");

        // High threshold
        float highX = bgRect.x + (highThreshold / maxNoiseLevel) * meterSize.x;
        Rect highMarker = new Rect(highX, bgRect.y, 2, bgRect.height);
        GUI.Box(highMarker, "");
    }

    Texture2D CreateColorTexture(Color color)
    {
        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, color);
        texture.Apply();
        return texture;
    }
}