using UnityEngine;

public class FridgeDoorController : MonoBehaviour
{
    public Animator animator;  // Drag the Animator here
    private bool isOpen = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            // Toggle the door state
            isOpen = !isOpen;

            // Send the state to the Animator
            animator.SetBool("isOpen", isOpen);

            // Debug messages
            Debug.Log("F key pressed! Door state: " + (isOpen ? "Open" : "Closed"));
            if (animator != null)
                Debug.Log("Animator found and isOpen parameter set.");
            else
                Debug.LogWarning("Animator not assigned!");
        }
    }
}
