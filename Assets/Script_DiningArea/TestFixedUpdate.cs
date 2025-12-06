using UnityEngine;

public class TestFixedUpdate : MonoBehaviour
{
    public Rigidbody rb;
    public float radius = 5.0f;
    public float power = 100.0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    
    // Update is called once per frame
    void FixedUpdate()
    {
        Vector3 explosionPos = transform.position;
        //rb.AddForce(10.0f *Vector3.up);
        rb.AddExplosionForce(power, explosionPos, radius, 3.0F);
    }
}
