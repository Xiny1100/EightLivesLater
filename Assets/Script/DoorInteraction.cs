using UnityEngine;
using DoorScript;
public class DoorInteraction : MonoBehaviour
{
    private Door doorScript;
    private bool playerInRange = false;

    void Start()
    {
        // Automatically find Door.cs in children
        doorScript = GetComponentInChildren<Door>();
        if (doorScript == null)
            Debug.LogError("No Door.cs found in children!");
    }

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            doorScript.OpenDoor();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            playerInRange = true;
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            playerInRange = false;
    }
}
