using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    public float interactDistance = 3f;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("Key E was pressed");
            TryInteract();
        }
    }

    void TryInteract()
    {
        RaycastHit hit;


        if (Physics.Raycast(transform.position, transform.forward, out hit, interactDistance))
        {
            Debug.Log("Raycast hit: " + hit.collider.name);

            IInteractable interactable = hit.collider.GetComponent<IInteractable>();

            if (interactable != null)
            {
                Debug.Log("Interact() is called");
                interactable.Interact();

            }
        }
        else
        {
            Debug.Log("Raycast hit nothing");
        }
    }
}
