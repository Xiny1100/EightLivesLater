using DoorScript;
using UnityEngine;
using UnityEngine.InputSystem;

public class PropPickUp : MonoBehaviour
{
    public GameObject secretDoor;
    // Reference to the door

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Show the hidden door (enable GameObject)
            secretDoor.SetActive(true);

            // Remove the cheese
            Destroy(gameObject);
        }
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
