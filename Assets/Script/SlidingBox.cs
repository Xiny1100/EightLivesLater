using UnityEngine;

public class SlidingBox : MonoBehaviour
{
    public Transform openPosition;  // where the box slides to when opening
    public float moveSpeed = 2f;
    public AudioClip slideSound;

    private bool isMoving = false;
    private bool playerInside = false;
    private AudioSource audioSource;

    private Vector3 closedPos;   // original position
    private Vector3 targetPos;   // current target

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        closedPos = transform.position;
        targetPos = closedPos;  // start closed
    }

    void Update()
    {
        // Move the box if it's sliding
        if (isMoving)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, targetPos) < 0.01f)
            {
                isMoving = false;
                Debug.Log("Sliding finished!");
            }
        }

        // Check for player input
        if (playerInside && Input.GetKeyDown(KeyCode.E) && !isMoving)
        {
            ToggleSlide();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            playerInside = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            playerInside = false;
    }

    private void ToggleSlide()
    {
        isMoving = true;

        // Switch target between open and closed
        targetPos = (targetPos == closedPos) ? openPosition.position : closedPos;

        // Play sound
        if (slideSound != null && audioSource != null)
            audioSource.PlayOneShot(slideSound);

        Debug.Log("Sliding started!");
    }
}
