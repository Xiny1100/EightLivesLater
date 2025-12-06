using UnityEngine;
using UnityEngine.UI;
using DoorScript;
using UnityEngine.InputSystem;

public class CheeseCollect : MonoBehaviour
{
    public GameObject secretDoor;

    public static int cheeseCount = 0;

    public Texture cheese1tex;
    public Texture cheese2tex;
    public Texture cheese3tex;
    public Texture cheese4tex;
    public Texture cheese0tex;
    private RawImage img;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
       img = GetComponent<RawImage>();
       img.enabled = false;
       cheeseCount = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if(cheeseCount == 1)
        {
            img.texture = cheese1tex;
            img.enabled = true;
        }
        else if (cheeseCount == 2)
        {
            img.texture = cheese2tex;
        }
        else if (cheeseCount == 3)
        {
            img.texture = cheese3tex;
        }
        else if (cheeseCount == 4)
        {
            img.texture = cheese4tex;
        }
        else
        {
            img.texture = cheese0tex;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            cheeseCount++;
            Debug.Log("Cheese collected! Count = " + cheeseCount);
            Destroy(gameObject);
        }
    }
}
