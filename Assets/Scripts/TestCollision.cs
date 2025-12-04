using UnityEngine;

public class TestCollision : MonoBehaviour
{

    void OnCollisionEnter(Collision collision)
    {
        foreach (ContactPoint contact in collision.contacts)
        {
            Debug.DrawRay(contact.point, contact.normal, Color.red, 50.0f);
        }
        if (collision.relativeVelocity.magnitude > 2)
        {
            Debug.Log("Collide!");

        }
    }

    void OnCollisionExit(Collision collision)
    {


    }
}