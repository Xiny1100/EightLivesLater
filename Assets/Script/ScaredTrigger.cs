using UnityEngine;

public class ScaredTrigger : MonoBehaviour
{
    [Header("Settings")]
    public string targetTag = "Customer";       // Tag for objects to trigger scared animation and rotate
    public string scaredTriggerName = "Scared";
    public bool freezePlayer = true;            // Freeze player on trigger

    [Header("Audio")]
    public AudioSource audioSource;             // Assign the AudioSource in Inspector
    public AudioClip scaredClip;                // Sound to play

    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered) return; // already triggered
        if (!other.CompareTag("Player")) return;
        hasTriggered = true; // lock it

        // Freeze player
        if (freezePlayer)
        {
            var playerMovement = other.GetComponent<PlayerMovement>(); // replace with your movement script
            if (playerMovement != null)
            {
                playerMovement.enabled = false;
                Debug.Log("Player frozen!");
            }

            Rigidbody rb = other.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;   // stop all current motion
                rb.angularVelocity = Vector3.zero;
                rb.isKinematic = true;
            }

        }

        // Play sound
        if (audioSource != null && scaredClip != null)
        {
            audioSource.PlayOneShot(scaredClip);
        }

        // Find all colliders in the zone
        Collider[] colliders = Physics.OverlapBox(transform.position, transform.localScale / 2, transform.rotation);

        foreach (Collider col in colliders)
        {
            if (col.CompareTag(targetTag))
            {
                // Trigger animation
                Animator anim = col.GetComponent<Animator>();
                if (anim != null)
                {
                    anim.SetTrigger(scaredTriggerName);
                    Debug.Log("Triggered scared animation for: " + col.name);
                }

                // Rotate to face player
                Vector3 lookDirection = other.transform.position - col.transform.position;
                lookDirection.y = 0; // keep upright
                if (lookDirection != Vector3.zero)
                {
                    col.transform.rotation = Quaternion.LookRotation(lookDirection);
                    Debug.Log("Rotating: " + col.name);
                }
            }
        }
    }


}
