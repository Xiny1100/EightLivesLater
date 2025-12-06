using UnityEngine;

public class Chest : MonoBehaviour, IInteractable
{
    public AudioClip openSound;
    private bool isOpened = false;
    private bool playerInRange = false;

    private AudioSource audioSource;
    private Animation anim;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        anim = GetComponent<Animation>();
    }

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            Interact();
        }
    }

    public void Interact()
    {
        if (isOpened) return;

        audioSource.clip = openSound;
        audioSource.Play();

        anim.Play("ChestOpen");

        isOpened = true;
    }

    // Detect when player enters trigger
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            Debug.Log("Player in range of chest");
        }
    }

    // Detect when player leaves trigger
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            Debug.Log("Player left chest range");
        }
    }
}
