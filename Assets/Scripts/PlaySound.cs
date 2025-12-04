using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

[RequireComponent (typeof(AudioSource))]
public class PlaySound : MonoBehaviour
{
    public AudioClip myClip;
    AudioSource audioSource;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        //audioSource.clip = myClip;
        //audioSource.Play();
        //AudioSource.PlayClipAtPoint(myClip, new Vector3(100, 0, 100));
    }

    // Update is called once per frame
    void Update()
    {
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame) {
            // AudioSource.PlayClipAtPoint(myClip,transform.position);
            audioSource.PlayOneShot(myClip,0.5F);

        }
    }
}
