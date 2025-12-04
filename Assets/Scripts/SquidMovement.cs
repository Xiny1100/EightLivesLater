using UnityEngine;
using UnityEngine.InputSystem;

public class SquidMovement : MonoBehaviour
{
    //public Animation anim;
    Animator animator;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            //anim.Play("kick");
            animator.SetTrigger("roll");

        }
        else
        {
            animator.SetTrigger("idle");
        }
    }
}
