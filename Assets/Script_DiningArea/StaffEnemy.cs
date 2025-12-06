using UnityEngine;

public class StaffEnemy : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public Animator animator;

    [Header("Movement")]
    public float rotationSpeed = 5f;
    public float moveSpeed = 2f;

    [Header("Detection")]
    public float stompDistance = 2f;

    private bool isChasing = false;
    private bool isBusy = false; // prevents overlapping actions
    private string lastPlayerSurface = "Floor";

    [Header("Audio")]
    public AudioClip surpriseClip;   // assign in inspector
    private AudioSource audioSource;

    private bool hasPlayedSurpriseAudio = false;
    private bool queuedStomp = false;
    private Rigidbody rb;


    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            Debug.LogError("AudioSource missing on Staff!");

        rb = GetComponent<Rigidbody>();
        if (rb == null)
            Debug.LogError("Rigidbody missing on Staff!");
        rb.isKinematic = true; // keep it kinematic, we move it manually

    }
    void Update()
    {
        UpdatePlayerSurface();

        if (isBusy) return;

        // --- Manual Chasing ---
        if (isChasing)
        {
            // Move staff toward player
            Vector3 targetPosition = new Vector3(player.position.x, transform.position.y, player.position.z);
            float step = 3f * Time.deltaTime; // 3 = movement speed
            rb.MovePosition(Vector3.MoveTowards(rb.position, targetPosition, step));


            // Rotate toward player
            Vector3 dir = (player.position - transform.position).normalized;
            dir.y = 0;
            if (dir.sqrMagnitude > 0.001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(dir);
                rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, rotationSpeed * Time.deltaTime));
            }

        }


    }

    private void UpdatePlayerSurface()
    {
        RaycastHit hit;
        Vector3 rayStart = player.position + Vector3.up * 0.1f;
        float rayLength = 3f;

        if (Physics.Raycast(rayStart, Vector3.down, out hit, rayLength))
        {
            if (hit.collider.tag != "Player" && hit.collider.tag != "Enemy")
            {
                lastPlayerSurface = hit.collider.tag;
                Debug.Log("Updated lastPlayerSurface: " + lastPlayerSurface);
            }
        }
    }

   

    public void AlertToPlayer()
    {
        if (isBusy) return;

        isBusy = true;
        isChasing = false;

        UpdatePlayerSurface();
        FreezePlayer();
        FacePlayer();

        if (!hasPlayedSurpriseAudio)
        {
            audioSource.PlayOneShot(surpriseClip);
            hasPlayedSurpriseAudio = true;
        }

        animator.SetTrigger("Surprise");
    }


    public void StartWalkingAfterSurprise()
    {
        isBusy = false;
        isChasing = true;

        if (queuedStomp)
        {
            queuedStomp = false;
            TriggerStomp();
        }
    }

    private void FacePlayer()
    {
        if (player == null) return;

        Vector3 dir = player.position - transform.position;
        dir.y = 0; // ignore vertical

        if (dir.sqrMagnitude < 0.001f) return; // too close, skip rotation

        transform.forward = Vector3.Slerp(transform.forward, dir.normalized, rotationSpeed * Time.deltaTime * 50f);

        Debug.Log("Rotating: " + transform.name);
    }


    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        UpdatePlayerSurface(); // make sure we have the latest surface

        if (lastPlayerSurface == "CounterSurface")
        {
            if (!isBusy)
            {
                Debug.Log("start pick");
                StartPickUp();
            }
            return; // pickup has priority
        }

        if (lastPlayerSurface == "Floor")
        {
            if (isBusy)
            {
                queuedStomp = true; // wait for Surprise
            }
            else
            {
                Debug.Log("start stomp");
                StartSurprise();
                TriggerStomp();
            }
        }
    }



    private void StartPickUp()
    {
        isBusy = true;
        isChasing = false;

        FreezePlayer();

        animator.SetTrigger("PickUp");
        Debug.Log("Staff starts PickUp");
    }

    // Called via Animation Event exactly when hand should grab player
    public void AttachPlayerToHand()
    {
        Transform hand = animator.GetBoneTransform(HumanBodyBones.RightHand);
        if (hand != null)
        {
            player.SetParent(hand);
            player.localPosition = Vector3.zero;
            player.localRotation = Quaternion.identity;
        }
        Debug.Log("Player attached to hand!");
    }

    // Called via Animation Event at the end of PickUp animation
    public void FinishPickUp()
    {
        isBusy = false;
        Debug.Log("Staff finished PickUp action");
    }

    private void StartSurprise()
    {
        isBusy = true;
        isChasing = false;

        FreezePlayer();
        FacePlayer();

        if (!hasPlayedSurpriseAudio)
        {
            audioSource.PlayOneShot(surpriseClip);
            hasPlayedSurpriseAudio = true;
        }

        animator.SetTrigger("Surprise");
        Debug.Log("Staff triggered Surprise due to player proximity");
    }

    private void TriggerStomp()
    {
        if (isBusy) return;

        isBusy = true;
        isChasing = false;

        animator.SetTrigger("Stomp");
        Debug.Log("Staff starts Stomp");

        // Reset busy after animation
        float stompDuration = 1.5f;
        Invoke(nameof(ResetBusy), stompDuration);
    }

    private void ResetBusy()
    {
        isBusy = false;
        Debug.Log("Staff finished action");
    }

    public void StopChasing()
    {
        isChasing = false;
    }
    public void FreezePlayer()
    {

        var playerMovement = player.GetComponent<PlayerController>();
        if (playerMovement != null) playerMovement.enabled = false;

        Rigidbody rb = player.GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true;
    }
}
