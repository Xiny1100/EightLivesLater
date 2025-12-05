using DoorScript;
using UnityEngine;
using UnityEngine.InputSystem;

public class RotateItem : MonoBehaviour
{
    public float rotationAmount = 5.0f;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(0.0f,rotationAmount,0.0f);
    }
}
