using UnityEngine;
using UnityEngine.InputSystem;

public class ChefMovement : MonoBehaviour
{
    public float speed;
    public float rotationSpeed = 200f;

    Animator animator;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        float x = Input.GetAxisRaw("Horizontal");   // A/D → turning
        float y = Input.GetAxisRaw("Vertical");     // W/S → forward/back

        // --- Rotation (A and D) ---
        if (x != 0)
        {
            transform.Rotate(0, x * rotationSpeed * Time.deltaTime, 0);
        }

        // --- Movement (W and S) ---
        Vector3 move = transform.forward * y * speed * Time.deltaTime;
        transform.Translate(move, Space.World);

        // --- Animation Control ---
        if (y != 0)   // walking or backward walking
        {
            animator.SetTrigger("Walking");
        }
        else
        {
            animator.SetTrigger("Idle");
        }
    }
}
