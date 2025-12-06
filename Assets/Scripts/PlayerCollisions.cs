using UnityEngine;
using DoorScript;
using UnityEngine.InputSystem;

public class PlayerCollisions : MonoBehaviour
{
    public AudioClip cheeseCollect;
    public GameObject secretDoor;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter(Collider collisionInfo)
    {
        if (collisionInfo.gameObject.tag == "Cheese")
        {
            AudioSource audio = GetComponent<AudioSource>();
            audio.clip = cheeseCollect;
            audio.Play();
            CheeseCollect.cheeseCount++;
            audio.PlayOneShot(cheeseCollect, 0.7F);
            Destroy(collisionInfo.gameObject);
        }

        if (collisionInfo.CompareTag("Player"))
        {
            // Show the hidden door (enable GameObject)
            secretDoor.SetActive(true);

            // Remove the cheese
            Destroy(gameObject);
        }

    }
}
