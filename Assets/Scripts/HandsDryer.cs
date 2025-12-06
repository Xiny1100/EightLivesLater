using UnityEngine;
using System.Collections;


public class HandsDryer : MonoBehaviour
{
    [Header("Light Components")]
    public Light flickeringLight;
    public Color normalLightColor = Color.blue;
    public Color activeLightColor = Color.red;
    public float colorChangeSpeed = 2f;
    private float baseIntensity;
    private Color originalLightColor;

    [Header("Light Flicker Settings")]
    public bool enableFlicker = true;
    public float flickerIntensityMin = 0.8f;
    public float flickerIntensityMax = 1.2f;
    public float flickerSpeed = 3f;

    [Header("Audio Settings")]
    public AudioSource audioSource;
    public float fadeOutTime = 1.5f;

    [Header("Animation Settings")]
    public Animator animator;
    public string boolName;

    [Header("Noise Settings")]
    public float noiseContribution = 25f;

    private bool isPlayerInTrigger = false;
    private bool isLightActive = false;
    private NoiseMeter noiseMeter;

    void Start()
    {
        if (flickeringLight == null)
            flickeringLight = GetComponent<Light>();

        if (flickeringLight != null)
        {
            baseIntensity = flickeringLight.intensity;
            originalLightColor = flickeringLight.color;
        }

        noiseMeter = FindObjectOfType<NoiseMeter>();

    }

    void Update()
    {
        if (isPlayerInTrigger && flickeringLight != null)
        {
            // Smoothly change light color to active color
            flickeringLight.color = Color.Lerp(flickeringLight.color, activeLightColor, colorChangeSpeed * Time.deltaTime);

            // Add flickering effect when active
            if (enableFlicker)
            {
                float flicker = Mathf.PingPong(Time.time * flickerSpeed, 1f);
                flickeringLight.intensity = Mathf.Lerp(flickerIntensityMin, flickerIntensityMax, flicker) * baseIntensity;
            }
        }
        else if (flickeringLight != null && isLightActive)
        {
            // Smoothly change light color back to normal
            flickeringLight.color = Color.Lerp(flickeringLight.color, normalLightColor, colorChangeSpeed * Time.deltaTime);

            // Reset intensity when not active
            if (!enableFlicker)
            {
                flickeringLight.intensity = Mathf.Lerp(flickeringLight.intensity, baseIntensity, colorChangeSpeed * Time.deltaTime);
            }
        }
    }
    private void OnTriggerEnter(Collider collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerInTrigger = true;
            isLightActive = true;

            StopAllCoroutines();
            animator.SetBool(boolName, true);

            if (audioSource != null && !audioSource.isPlaying)
                audioSource.Play();

            // Start light color change coroutine
            StartCoroutine(ChangeLightColor(activeLightColor));


            if (noiseMeter != null)
            {
                noiseMeter.AddNoise(noiseContribution);
            }
        }
    }

    private void OnTriggerExit(Collider collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerInTrigger = false;

            StartCoroutine(FadeOut(audioSource, fadeOutTime));
            animator.SetBool(boolName, false);

            // Start fading light back to normal
            StartCoroutine(ChangeLightColor(normalLightColor));
        }
    }

    IEnumerator FadeOut(AudioSource audioSource, float duration)
    {
        if (audioSource != null)
        {
            float startVolume = audioSource.volume;

            while (audioSource.volume > 0f)
            {
                audioSource.volume -= startVolume * Time.deltaTime / duration;
                yield return null;
            }

            audioSource.Stop();
            audioSource.volume = startVolume;
        }
    }

    IEnumerator ChangeLightColor(Color targetColor)
    {
        if (flickeringLight == null) yield break;

        Color startColor = flickeringLight.color;
        float elapsedTime = 0f;

        while (elapsedTime < 1f)
        {
            elapsedTime += Time.deltaTime * colorChangeSpeed;
            flickeringLight.color = Color.Lerp(startColor, targetColor, elapsedTime);
            yield return null;
        }

        flickeringLight.color = targetColor;

        // If we're changing to normal color, we're no longer active
        if (targetColor == normalLightColor)
        {
            isLightActive = false;
            flickeringLight.intensity = baseIntensity;
        }
    }

    // Optional: Method to manually set light color
    public void SetLightColor(Color newColor, float duration = 1f)
    {
        if (flickeringLight != null)
        {
            StartCoroutine(ChangeLightColorOverTime(newColor, duration));
        }
    }

    private IEnumerator ChangeLightColorOverTime(Color newColor, float duration)
    {
        Color startColor = flickeringLight.color;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            flickeringLight.color = Color.Lerp(startColor, newColor, elapsedTime / duration);
            yield return null;
        }

        flickeringLight.color = newColor;
    }
}