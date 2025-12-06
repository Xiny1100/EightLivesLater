using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class EnemyDialogue : MonoBehaviour
{
    [Header("Whistling Settings")]
    public AudioSource audioSource;       // For whistling sound
    public AudioClip whistlingClip;       // Single whistling sound file
    public AudioSource bgmAudioSource;    // For spotted BGM

    [Header("Whistling Timing")]
    public float minWhistleDuration = 10f;    // Minimum whistle time
    public float maxWhistleDuration = 30f;    // Maximum whistle time
    public float minPauseDuration = 5f;       // Minimum pause between whistles
    public float maxPauseDuration = 15f;      // Maximum pause between whistles
    public float fadeOutTime = 2f;            // Fade out duration
    public float fadeInTime = 1f;             // Fade in duration

    [Header("BGM Management")]
    public AudioSource mainBGM;           // Your regular background music to disable

    [Header("Spotted Settings")]
    public AudioClip spottedDialogue;     // Special "spotted" dialogue clip
    public AudioClip spottedBGM;          // Spotted background music
    public float spottedBGMDelay = 2f;    // Delay after spotted dialogue before BGM starts

    [Header("UI Settings")]
    public GameObject hideTextObject;     // Reference to your "HIDE!!" text UI object
    public float displayDuration = 3f;    // How long to show the text

    [Header("Noise Detection")]
    public NoiseMeter noiseMeter;         // Reference to your existing noise meter
    public float spottedNoiseThreshold = 100f; // Noise level that triggers spotted state

    // State variables
    private bool isSpotted = false;
    private bool hasPlayedSpottedDialogue = false;
    private bool spottedStateTriggered = false;
    private bool wasMainBGMPlaying = false;
    private bool isWhistling = false;
    private Coroutine currentWhistleCoroutine;
    private float nextWhistleTime;

    void Start()
    {
        // Setup audio sources
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        if (bgmAudioSource == null)
        {
            bgmAudioSource = GetComponent<AudioSource>();
            if (bgmAudioSource == audioSource)
            {
                bgmAudioSource = gameObject.AddComponent<AudioSource>();
            }
        }

        // Try to find main BGM automatically
        if (mainBGM == null)
        {
            GameObject bgmObject = GameObject.Find("BGM") ?? GameObject.Find("BackgroundMusic") ?? GameObject.Find("Music");
            if (bgmObject != null)
            {
                mainBGM = bgmObject.GetComponent<AudioSource>();
            }
        }

        // Try to find noise meter
        if (noiseMeter == null)
        {
            noiseMeter = FindObjectOfType<NoiseMeter>();
        }

        // Try to find hide text
        if (hideTextObject == null)
        {
            hideTextObject = GameObject.Find("HideText") ?? GameObject.Find("HIDEText") ?? GameObject.Find("HideTextObject");
        }

        // Hide the text at start
        if (hideTextObject != null)
        {
            hideTextObject.SetActive(false);
        }

        // Force audio to play in both ears
        if (audioSource != null)
        {
            audioSource.spatialBlend = 0f;
            audioSource.loop = true; // Whistling should loop
        }

        if (bgmAudioSource != null)
        {
            bgmAudioSource.spatialBlend = 0f;
        }

        // Start random whistling
        ScheduleNextWhistle();
    }

    void Update()
    {
        // Only check for spotted state if not already spotted
        if (!spottedStateTriggered && noiseMeter != null && noiseMeter.currentNoiseLevel >= spottedNoiseThreshold)
        {
            TriggerSpottedState();
        }

        // Handle random whistling schedule (only when not spotted)
        if (!isSpotted && !isWhistling && Time.time >= nextWhistleTime)
        {
            StartRandomWhistling();
        }
    }

    void ScheduleNextWhistle()
    {
        if (!isSpotted)
        {
            float delay = Random.Range(minPauseDuration, maxPauseDuration);
            nextWhistleTime = Time.time + delay;
            Debug.Log($"Next whistle scheduled in {delay:F1} seconds");
        }
    }

    void StartRandomWhistling()
    {
        if (whistlingClip != null && audioSource != null && !isSpotted)
        {
            if (currentWhistleCoroutine != null)
            {
                StopCoroutine(currentWhistleCoroutine);
            }

            currentWhistleCoroutine = StartCoroutine(PlayWhistlingSession());
        }
    }

    IEnumerator PlayWhistlingSession()
    {
        isWhistling = true;

        // Calculate random duration for this whistling session
        float whistleDuration = Random.Range(minWhistleDuration, maxWhistleDuration);
        Debug.Log($"Starting whistling session for {whistleDuration:F1} seconds");

        // Fade in
        audioSource.clip = whistlingClip;
        audioSource.volume = 0f;
        audioSource.Play();

        float fadeTimer = 0f;
        while (fadeTimer < fadeInTime)
        {
            fadeTimer += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(0f, 1f, fadeTimer / fadeInTime);
            yield return null;
        }
        audioSource.volume = 1f;

        // Whistle for the random duration (minus fade times)
        float remainingTime = whistleDuration - fadeInTime - fadeOutTime;
        yield return new WaitForSeconds(Mathf.Max(0f, remainingTime));

        // Fade out
        fadeTimer = 0f;
        while (fadeTimer < fadeOutTime)
        {
            fadeTimer += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(1f, 0f, fadeTimer / fadeOutTime);
            yield return null;
        }

        // Stop and reset
        audioSource.Stop();
        audioSource.volume = 1f;
        isWhistling = false;

        // Schedule next whistle
        ScheduleNextWhistle();

        Debug.Log("Whistling session ended");
    }

    void TriggerSpottedState()
    {
        isSpotted = true;
        spottedStateTriggered = true;
        hasPlayedSpottedDialogue = false;

        // Stop any current whistling
        if (currentWhistleCoroutine != null)
        {
            StopCoroutine(currentWhistleCoroutine);
            currentWhistleCoroutine = null;
        }

        if (audioSource != null && audioSource.isPlaying)
        {
            StartCoroutine(FadeOutAudio(audioSource, 0.5f)); // Quick fade out
        }

        // Store main BGM state and stop it
        if (mainBGM != null)
        {
            wasMainBGMPlaying = mainBGM.isPlaying;
            mainBGM.Stop();
        }

        // Play spotted dialogue
        PlaySpottedDialogue();

        Debug.Log("ENEMY SPOTTED PLAYER! Spotted state activated.");
    }

    IEnumerator FadeOutAudio(AudioSource source, float duration)
    {
        float startVolume = source.volume;
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            source.volume = Mathf.Lerp(startVolume, 0f, timer / duration);
            yield return null;
        }

        source.Stop();
        source.volume = startVolume;
    }

    void PlaySpottedDialogue()
    {
        if (spottedDialogue != null && audioSource != null && !hasPlayedSpottedDialogue)
        {
            audioSource.spatialBlend = 0f;
            audioSource.PlayOneShot(spottedDialogue, 1.8f);
            hasPlayedSpottedDialogue = true;

            // Start coroutine to play BGM after dialogue finishes
            StartCoroutine(PlaySpottedBGMAfterDelay(spottedDialogue.length));
        }
        else if (spottedDialogue == null)
        {
            PlaySpottedBGM();
            ShowHideText();
        }
    }

    IEnumerator PlaySpottedBGMAfterDelay(float dialogueLength)
    {
        yield return new WaitForSeconds(dialogueLength + spottedBGMDelay);
        PlaySpottedBGM();
        ShowHideText();
    }

    void PlaySpottedBGM()
    {
        if (spottedBGM != null && bgmAudioSource != null)
        {
            bgmAudioSource.spatialBlend = 0f;
            bgmAudioSource.clip = spottedBGM;
            bgmAudioSource.loop = true;
            bgmAudioSource.Play();
        }
    }

    void ShowHideText()
    {
        if (hideTextObject != null)
        {
            hideTextObject.SetActive(true);
            StartCoroutine(HideTextAfterDelay());
        }
    }

    IEnumerator HideTextAfterDelay()
    {
        yield return new WaitForSeconds(displayDuration);

        if (hideTextObject != null)
        {
            hideTextObject.SetActive(false);
        }
    }

    // Public methods for external control
    public void ForceSpottedState()
    {
        TriggerSpottedState();
    }

    public void ResetSpottedState()
    {
        isSpotted = false;
        spottedStateTriggered = false;
        hasPlayedSpottedDialogue = false;

        // Stop spotted BGM
        if (bgmAudioSource != null && bgmAudioSource.isPlaying)
        {
            bgmAudioSource.Stop();
        }

        // Hide text
        if (hideTextObject != null)
        {
            hideTextObject.SetActive(false);
        }

        // Restore main BGM
        if (mainBGM != null && wasMainBGMPlaying)
        {
            mainBGM.Play();
        }

        // Restart whistling
        ScheduleNextWhistle();
    }

    public bool IsSpotted()
    {
        return isSpotted;
    }

    // Optional: Visual feedback
    void OnDrawGizmos()
    {
        if (isSpotted)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, 2f);
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(transform.position, 2.5f);
        }
    }
}