using System.Collections;
using UnityEngine;

public class FieldOfView : MonoBehaviour
{
    [Header("FOV Settings")]
    public float radius = 10f;
    [Range(0, 360)]
    public float angle = 90f;

    [Header("References")]
    public GameObject playerRef;

    public LayerMask targetMask;
    public LayerMask obstructionMask;

    [Header("Detection")]
    public bool canSeePlayer;

    private void Start()
    {
        playerRef = GameObject.FindGameObjectWithTag("Player");
        StartCoroutine(FOVRoutine());
    }

    private IEnumerator FOVRoutine()
    {
        WaitForSeconds wait = new WaitForSeconds(0.2f);

        while (true)
        {
            yield return wait;
            FieldOfViewCheck();
        }
    }

    private void FieldOfViewCheck()
    {
        canSeePlayer = false;

        Collider[] rangeChecks = Physics.OverlapSphere(transform.position, radius, targetMask);

        if (rangeChecks.Length != 0)
        {
            Transform target = rangeChecks[0].transform;

            // Check multiple points on player: head, center, feet
            Vector3[] pointsToCheck = new Vector3[]
            {
                target.position + Vector3.up * 1f,    // head
                target.position + Vector3.up * 0.5f,  // center
                target.position + Vector3.up * 0.1f   // feet
            };

            foreach (Vector3 point in pointsToCheck)
            {
                Vector3 directionToTarget = (point - transform.position).normalized;
                float distanceToTarget = Vector3.Distance(transform.position, point);

                if (Vector3.Angle(transform.forward, directionToTarget) < angle / 2)
                {
                    if (!Physics.Raycast(transform.position, directionToTarget, distanceToTarget, obstructionMask))
                    {
                        canSeePlayer = true;
                        Debug.Log($"Enemy sees player at point: {point}");
                        break; // Player seen, no need to check other points
                    }
                    else
                    {
                        Debug.Log($"Player blocked at point: {point}");
                    }
                }
            }
        }
        else
        {
            Debug.Log("Player not in detection radius");
        }
    }

    // Debug visualization in Scene view
    private void OnDrawGizmos()
    {
        if (playerRef == null) return;

        Vector3 origin = transform.position;

        // Draw vision radius
        Gizmos.color = new Color(1, 1, 0, 0.1f);
        Gizmos.DrawWireSphere(origin, radius);

        // Draw cone boundaries
        Vector3 leftBoundary = Quaternion.Euler(0, -angle / 2, 0) * transform.forward * radius;
        Vector3 rightBoundary = Quaternion.Euler(0, angle / 2, 0) * transform.forward * radius;

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(origin, origin + leftBoundary);
        Gizmos.DrawLine(origin, origin + rightBoundary);

        // Draw line to player
        Vector3 dirToPlayer = (playerRef.transform.position - origin).normalized;
        float dist = Vector3.Distance(origin, playerRef.transform.position);

        if (canSeePlayer)
            Gizmos.color = Color.green;
        else
            Gizmos.color = Color.red;

        Gizmos.DrawLine(origin, playerRef.transform.position);
    }
}
