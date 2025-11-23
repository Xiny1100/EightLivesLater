using UnityEngine;
using System.Collections;

public class ToiletSitter : MonoBehaviour
{
    public AudioSource audioSource;
    public float fadeOutTime = 1.5f;
    public Animator animator;
    public string boolName;

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.CompareTag("Player"))
        {
            StopAllCoroutines();
            animator.SetBool(boolName, true);
            if (!audioSource.isPlaying)
                audioSource.Play();

        }
    }

    private void OnTriggerExit(Collider collision)
    {
        if (collision.CompareTag("Player"))
        {
            StartCoroutine(FadeOut(audioSource, fadeOutTime));
            animator.SetBool(boolName, false);
        }
    }

    IEnumerator FadeOut(AudioSource audioSource, float duration)
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
