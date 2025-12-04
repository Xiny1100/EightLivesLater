using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class EnemyAI : MonoBehaviour
{
    [Header("Rear Detection")]
    public float rearDetectionDistance = 3f; // how close the player must be behind
    public float rearTurnSpeed = 5f;        // how fast the waiter turns toward the player

    [Header("References")]
    public NavMeshAgent agent;
    public Animator animator;
    public Transform player;
    public Light spotlight; // Your spotlight component

    [Header("Chase Settings")]
    public float patrolSpeed = 2f;
    public float chaseSpeed = 5f;
    public Transform[] patrolPoints;
    private int currentPoint = 0;

    [Header("Detection")]
    public LayerMask obstructionMask;
    public bool canSeePlayer;
    public float chaseMemory = 1.5f; // seconds to remember player after losing sight
    private float lastTimeSawPlayer = 0f;

    [Header("Catch Settings")]
    public float catchDistance = 1f; // how close the enemy needs to be to catch the player
    private bool hasCaughtPlayer = false;

    [Header("Attach Settings")]
    public Transform handAttachPoint; // empty under hand to attach player

    [Header("Audio")]
    public AudioSource audioSource;       // Assign in Inspector
    public AudioClip foundSound;          // Assign the "found you" audio clip
    private bool hasPlayedFoundSound = false; // to prevent spamming

    [Header("Chase Music")]
    public AudioSource chaseMusicSource; // Assign in Inspector
    public float fadeDuration = 4.0f;   // seconds to fade in/out
    private bool isChaseMusicPlaying = false;


    private HidingSpot currentHidingSpot;
    
    void Start()
    {
        agent.speed = patrolSpeed;
        GoToNextPoint();

    }

    void Update()
    {
        CheckPlayerInSpotlight();

        // Update last seen time
        if (canSeePlayer)
            lastTimeSawPlayer = Time.time;

        // Check if close enough to catch player (independent of vision)
        if (!hasCaughtPlayer && Vector3.Distance(transform.position, player.position) <= catchDistance)
        {
            Debug.Log("Player within catch distance");
            CatchPlayer();
            return; // stop further logic once caught
        }

        // --- Rear detection check ---
        if (!hasCaughtPlayer && !canSeePlayer && IsPlayerBehind())
        {
            TurnTowardsPlayer();
            // Optional: start chasing if you want the rear detection to trigger chase
            StartChase();
            agent.SetDestination(player.position);
        }

        // Chase logic: continue chasing if can see or within memory time
        else if (!hasCaughtPlayer && (canSeePlayer || (Time.time - lastTimeSawPlayer <= chaseMemory)))
        {
            StartChase();
            agent.SetDestination(player.position);
        }
        else
        {
            StopChase();
        }

        animator.SetBool("IsChasing", isChasing);

        // Patrol if not chasing
        if (!isChasing && !agent.pathPending && agent.remainingDistance < 0.3f)
            GoToNextPoint();
    }


    void CheckPlayerInSpotlight()
    {
        if (player == null || spotlight == null) return;

        // Check if player is hiding
        HidingSpot hidingSpot = player.GetComponentInParent<HidingSpot>();
        if (hidingSpot != null && hidingSpot.playerInside)
        {
            canSeePlayer = false;
            Debug.Log("Player is hiding, enemy ignores them");
            return;
        }

        // Normal spotlight detection
        Vector3 directionToPlayer = (player.position - spotlight.transform.position).normalized;
        float distanceToPlayer = Vector3.Distance(spotlight.transform.position, player.position);
        float angle = Vector3.Angle(spotlight.transform.forward, directionToPlayer);

        if (angle < spotlight.spotAngle / 2 && distanceToPlayer <= spotlight.range)
        {
            if (!Physics.Raycast(spotlight.transform.position, directionToPlayer, distanceToPlayer, obstructionMask))
            {
                canSeePlayer = true;
                Debug.Log("Enemy sees player!");

                if (canSeePlayer)
                {
                    // Play found sound once
                    if (!hasPlayedFoundSound && audioSource != null && foundSound != null)
                    {
                        audioSource.PlayOneShot(foundSound);
                        hasPlayedFoundSound = true;
                    }

                    // Start chase music if not playing
                    if (!isChaseMusicPlaying && chaseMusicSource != null)
                    {
                        isChaseMusicPlaying = true;
                        StopAllCoroutines(); // stop any fading coroutines
                        StartCoroutine(FadeInMusic(chaseMusicSource, fadeDuration));
                    }

                    return;
                }

                return;
            }
        }

        if (!canSeePlayer && isChaseMusicPlaying)
        {
            isChaseMusicPlaying = false;
            StopAllCoroutines(); // stop any fading coroutines
            StartCoroutine(FadeOutMusic(chaseMusicSource, fadeDuration));
            hasPlayedFoundSound = false;
        }


        Debug.Log("Player not in spotlight.");
    }

    void GoToNextPoint()
    {
        if (patrolPoints.Length == 0) return;
        agent.SetDestination(patrolPoints[currentPoint].position);
        currentPoint = (currentPoint + 1) % patrolPoints.Length;
    }

    private bool isChasing = false;
    void StartChase()
    {
        if (isChasing) return;
        isChasing = true;
        agent.speed = chaseSpeed;
        Debug.Log("Enemy started chasing!");
    }

    void StopChase()
    {
        if (!isChasing) return;
        isChasing = false;
        agent.speed = patrolSpeed;
        GoToNextPoint();
        Debug.Log("Enemy stopped chasing.");
    }

    void CatchPlayer()
    {
        if (hasCaughtPlayer) return;

        hasCaughtPlayer = true;

        agent.isStopped = true;
        isChasing = false;

        if (animator != null)
        {
            animator.SetTrigger("PickUp");
            Debug.Log("PickingUp animation triggered");
        }

        Debug.Log("Game Over! Player is in catch state");

        // Freeze player immediately
        FreezePlayer();
        
        // Fade out chase music
        if (chaseMusicSource != null)
        {
            StopAllCoroutines(); // stop any other fading coroutines
            StartCoroutine(FadeOutMusic(chaseMusicSource, fadeDuration));
        }
    }


    public void GrabPlayer()
    {
        Debug.Log("GrabPlayer() called from animation event");

        // Freeze player immediately
        FreezePlayer();

        // Attach to hand
        if (handAttachPoint != null)
        {
            player.SetParent(handAttachPoint);
            player.localPosition = Vector3.zero;
            player.localRotation = Quaternion.identity;
            Debug.Log("Player attached to enemy hand");
        }
    }

    void FreezePlayer()
    {
        var playerMovement = player.GetComponent<PlayerController>();
        if (playerMovement != null)
            playerMovement.enabled = false;

        Rigidbody rb = player.GetComponent<Rigidbody>();
        if (rb != null)
            rb.isKinematic = true;
    }

    // Visualize spotlight line
    private void OnDrawGizmos()
    {
        if (spotlight == null || player == null) return;

        Vector3 direction = (player.position - spotlight.transform.position).normalized;
        Gizmos.color = canSeePlayer ? Color.green : Color.red;
        Gizmos.DrawLine(spotlight.transform.position, player.position);
    }

    bool IsPlayerBehind()
    {
        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        directionToPlayer.y = 0; // ignore vertical differences

        float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        return angleToPlayer > 90f && distanceToPlayer <= rearDetectionDistance;
    }

    void TurnTowardsPlayer()
    {
        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        directionToPlayer.y = 0; // keep rotation horizontal

        Quaternion lookRotation = Quaternion.LookRotation(directionToPlayer);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rearTurnSpeed);
    }

    private IEnumerator FadeInMusic(AudioSource source, float duration)
    {
        if (source == null) yield break;
        source.volume = 0;
        source.Play();
        float timer = 0f;
        while (timer < duration)
        {
            source.volume = Mathf.Lerp(0, 1, timer / duration);
            timer += Time.deltaTime;
            yield return null;
        }
        source.volume = 1;
    }

    private IEnumerator FadeOutMusic(AudioSource source, float duration)
    {
        if (source == null) yield break;
        float startVolume = source.volume;
        float timer = 0f;
        while (timer < duration)
        {
            source.volume = Mathf.Lerp(startVolume, 0, timer / duration);
            timer += Time.deltaTime;
            yield return null;
        }
        source.volume = 0;
        source.Stop();
    }

}
