using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class GlassBottle : MonoBehaviour
{
    public AudioClip fallSound;          // Sound to play when bottle falls
    public StaffEnemy staffToAlert;      // Assign the staff that should react
    public float fallVelocityThreshold = 0.5f; // Minimum impact velocity to trigger
    public float destroyAfterSeconds = 5f;     // Delay destroy to let sound play

    private AudioSource audioSource;
    private Rigidbody rb;
    private bool hasFallen = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (hasFallen) return;

        // Check collision with floor or counter
        if (collision.gameObject.CompareTag("Floor") || collision.gameObject.CompareTag("CounterSurface"))
        {
            // Only trigger if fall is strong enough
            if (rb.linearVelocity.magnitude >= fallVelocityThreshold)
            {
                hasFallen = true;

                // Play sound
                if (audioSource != null && fallSound != null)
                    audioSource.PlayOneShot(fallSound);

                // Alert the staff
                if (staffToAlert != null)
                    staffToAlert.AlertToPlayer();

                // Destroy after delay so sound can play
                Destroy(gameObject, destroyAfterSeconds);
            }
        }
    }
}
