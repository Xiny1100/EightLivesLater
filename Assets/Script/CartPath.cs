using UnityEngine;

public class CartPath : MonoBehaviour
{
    public Transform[] pathPoints;
    public float speed = 5f;
    private int currentIndex = 0;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        if (pathPoints.Length > 0)
            transform.position = pathPoints[0].position;
    }

    void FixedUpdate()
    {
        if (pathPoints.Length == 0) return;

        Vector3 targetPos = pathPoints[currentIndex].position;
        Vector3 move = Vector3.MoveTowards(transform.position, targetPos, speed * Time.fixedDeltaTime);
        rb.MovePosition(move);

        if (Vector3.Distance(transform.position, targetPos) < 0.1f)
        {
            currentIndex = (currentIndex + 1) % pathPoints.Length; // loops path
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("Player hit by cart!");
            // Trigger fail, damage, or knockback here
        }
    }
}
