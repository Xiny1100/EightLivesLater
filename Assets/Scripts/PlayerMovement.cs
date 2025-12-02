using UnityEngine;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed;
    public float groundDrag;
    public float jumpForce;
    public float jumpCoolDown;
    public float airMultiplier;
    bool readyToJump = true;

    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode attackKey = KeyCode.Mouse0;

    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask whatIsGround;
    bool grounded;

    [Header("Animation")]
    public Animator animator;

    [Header("Idle Sit Settings")]
    public float idleTimeBeforeSit = 10f;
    public float minIdleTime = 5f;

    [Header("Raycasting Toilet Door")]
    public float checkDistance = 0.2f;
    public AudioClip lowerDownSound;
    public string playerAnimationName = "LowerDown";

    [Header("Lowering Settings")]
    public bool isLowering = false;
    public float lowerDuration = 3f;
    public float loweredHeightMultiplier = 0.2f;

    private CapsuleCollider playerCollider;
    private float originalHeight;
    private float originalCenterY;

    private AudioSource audioSource;
    private bool animationPlayed = false;
    private GameObject currentDoor;

    public Transform orientation;

    float horizontalInput;
    float verticalInput;

    Vector3 moveDirection;
    Rigidbody rb;

    private bool isAttacking = false;
    private bool isDead = false;
    private bool isSitting = false;
    private float attackCooldown = 0.5f;
    private bool canAttack = true;
    private bool isJumping = false;

    private float idleTimer = 0f;
    private float lastMoveTime = 0f;
    private Vector3 lastPosition;
    private bool isMoving = false;

    void Start()
    {
        playerCollider = GetComponent<CapsuleCollider>();
        originalHeight = playerCollider.height;
        originalCenterY = playerCollider.center.y;
        audioSource = GetComponent<AudioSource>();
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        readyToJump = true;

        if (animator == null)
            animator = GetComponent<Animator>();

        lastPosition = transform.position;
    }



    private GameObject GetDoorInFront()
    {
        RaycastHit hit;
        Vector3 origin = orientation.position;
        Vector3 dir = orientation.forward;

        if (Physics.Raycast(origin, dir, out hit, checkDistance))
        {
            if (hit.collider.CompareTag("Door"))
            {
                return hit.collider.gameObject;
            }
        }
        return null;
    }

    public void MyInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        // Check for key DOWN, not holding
        if (Input.GetKeyDown(jumpKey) && readyToJump && grounded && !isAttacking && !isSitting)
        {
            readyToJump = false;
            isJumping = true;
            Jump();
            Invoke(nameof(ResetJump), jumpCoolDown);
        }

        if (Input.GetKeyDown(attackKey) && grounded && canAttack && !isDead && !isJumping && !isSitting)
        {
            Attack();
        }

        if (isSitting && (horizontalInput != 0 || verticalInput != 0 || Input.GetKeyDown(jumpKey) || Input.GetKeyDown(attackKey)))
        {
            BreakSit();
        }
    }

    void Update()
    {
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround);

        MyInput();
        SpeedControl();
        CheckMovement();
        UpdateIdleTimer();
        UpdateAnimations();

        if (grounded && isJumping && rb.linearVelocity.y <= 0)
        {
            isJumping = false;
        }

        if (grounded)
        {
            rb.linearDamping = groundDrag;
        }
        else
        {
            rb.linearDamping = 0;
        }

        //Raycasting to check for toilet door
        RaycastHit hit;
        Vector3 origin = orientation.position;
        Vector3 dir = orientation.forward;

        Debug.DrawRay(origin, dir * checkDistance, Color.yellow);


    }

    void FixedUpdate()
    {
        if (!isAttacking && !isDead && !isSitting && !isLowering)
            MovePlayer();
    }

    private void MovePlayer()
    {
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        if (grounded)
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
        else if (!grounded)
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);
    }

    private void SpeedControl()
    {
        Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        if (flatVel.magnitude > moveSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * moveSpeed;
            rb.linearVelocity = new Vector3(limitedVel.x, rb.linearVelocity.y, limitedVel.z);
        }
    }

    private void CheckMovement()
    {
        bool wasMoving = isMoving;
        isMoving = (transform.position != lastPosition) ||
                   horizontalInput != 0 ||
                   verticalInput != 0 ||
                   isAttacking ||
                   isJumping;

        if (isMoving && !wasMoving)
        {
            OnStartMoving();
        }
        else if (!isMoving && wasMoving)
        {
            OnStopMoving();
        }

        lastPosition = transform.position;
    }

    private void OnStartMoving()
    {
        if (isSitting)
        {
            BreakSit();
        }
        idleTimer = 0f;
    }

    private void OnStopMoving()
    {
        lastMoveTime = Time.time;
    }

    private void UpdateIdleTimer()
    {
        if (!isMoving && grounded && !isDead && !isSitting && !isAttacking)
        {
            idleTimer += Time.deltaTime;

            if (idleTimer >= idleTimeBeforeSit)
            {
                StartSitting();
            }
        }
        else
        {
            idleTimer = 0f;
        }
    }

    private void StartSitting()
    {
        if (!isSitting && grounded && !isDead)
        {
            isSitting = true;
            idleTimer = 0f;

            if (animator != null)
            {
                animator.SetTrigger("Sit");
            }
        }
    }

    private void BreakSit()
    {
        if (isSitting)
        {
            isSitting = false;
            idleTimer = 0f;

            if (animator != null)
            {
                animator.SetTrigger("Stand");
            }

            CancelInvoke(nameof(BreakSit));
        }
    }

    private void UpdateAnimations()
    {
        if (animator == null) return;

        float currentSpeed = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z).magnitude;

        animator.SetFloat("Speed", currentSpeed);
        animator.SetBool("IsGrounded", grounded);
        animator.SetBool("IsJumping", isJumping);
        animator.SetBool("IsAttacking", isAttacking);
        animator.SetBool("IsDead", isDead);
        animator.SetBool("IsSitting", isSitting);
    }

    private void Jump()
    {
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    private void ResetJump()
    {
        readyToJump = true;
    }

    private void Attack()
    {
        isAttacking = true;
        canAttack = false;

        Invoke(nameof(ResetAttack), attackCooldown);
    }

    private void ResetAttack()
    {
        isAttacking = false;
        canAttack = true;
    }

    public void OnAttackAnimationEnd()
    {
        isAttacking = false;
    }

    public void Die()
    {
        isDead = true;
        BreakSit(); 
    }

    public void Respawn()
    {
        isDead = false;
        isAttacking = false;
        canAttack = true;
        isJumping = false;
        isSitting = false;
        idleTimer = 0f;
    }

    void OnGUI()
    {
#if UNITY_EDITOR
        GUI.Label(new Rect(10, 100, 300, 20), $"Idle Timer: {idleTimer:F1}/{idleTimeBeforeSit}");
        GUI.Label(new Rect(10, 120, 300, 20), $"Is Moving: {isMoving}");
        GUI.Label(new Rect(10, 140, 300, 20), $"Is Sitting: {isSitting}");
#endif
    }
}