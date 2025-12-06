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
    public KeyCode interactKey = KeyCode.E;

    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask whatIsGround;
    bool grounded;

    [Header("Animation")]
    public Animator animator;

    [Header("Idle Sit Settings")]
    public float idleTimeBeforeSit = 10f;
    public float minIdleTime = 5f;

    [Header("Shadow Detection")]
    public AudioClip lowerDownSound;
    public string playerAnimationName = "LowerDown";
    public float raycastDistance = 0.2f;
    public LayerMask shadowLayerMask = 1;

    [Header("Lowering Settings")]
    public bool isLowering = false;
    public float lowerDuration = 3f;
    public float loweredHeightMultiplier = 0.2f;

    private CapsuleCollider playerCollider;
    private float originalHeight;
    private float originalCenterY;

    private AudioSource audioSource;
    private bool animationPlayed = false;
    private GameObject detectedShadow;
    private Collider shadowCollider;

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

    // Shadow detection variables
    private bool isLookingAtShadow = false;
    private bool canInteractWithShadow = true;

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

    IEnumerator DoLowerAnimation()
    {
        isLowering = true;
        animationPlayed = true;
        canInteractWithShadow = false; // Prevent multiple interactions

        if (detectedShadow != null && shadowCollider != null)
        {
            // Disable shadow collision temporarily
            shadowCollider.enabled = false;

            // Calculate target position (move forward through the shadow)
            Vector3 moveDirection = orientation.forward;
            Vector3 targetPosition = transform.position + moveDirection * 1.5f;

            animator.SetBool("isLowering", true);
            if (audioSource != null && lowerDownSound != null)
            {
                audioSource.clip = lowerDownSound;
                audioSource.Play();
            }

            // Shrink player collider
            playerCollider.height = originalHeight * loweredHeightMultiplier;
            playerCollider.center = new Vector3(
                playerCollider.center.x,
                originalCenterY * loweredHeightMultiplier,
                playerCollider.center.z
            );

            // Move player through the shadow area over time
            float elapsedTime = 0f;
            Vector3 startPosition = transform.position;

            while (elapsedTime < lowerDuration)
            {
                elapsedTime += Time.deltaTime;
                float progress = elapsedTime / lowerDuration;

                // Smooth movement through the shadow
                transform.position = Vector3.Lerp(startPosition, targetPosition, progress);

                yield return null;
            }

            // Ensure final position
            transform.position = targetPosition;

            // Reset animation and player size
            animator.SetBool("isLowering", false);

            playerCollider.height = originalHeight;
            playerCollider.center = new Vector3(
                playerCollider.center.x,
                originalCenterY,
                playerCollider.center.z
            );

            // Wait a bit before re-enabling shadow interaction
            yield return new WaitForSeconds(1f);

            // Re-enable shadow collision
            shadowCollider.enabled = true;

            // Reset all shadow-related variables
            detectedShadow = null;
            shadowCollider = null;
            isLookingAtShadow = false;

            yield return new WaitForSeconds(0.5f); // Additional cooldown

            // Re-enable interactions
            canInteractWithShadow = true;
        }
        else
        {
            // Fallback if no shadow found
            animator.SetBool("isLowering", true);
            if (audioSource != null && lowerDownSound != null)
            {
                audioSource.clip = lowerDownSound;
                audioSource.Play();
            }

            playerCollider.height = originalHeight * loweredHeightMultiplier;
            playerCollider.center = new Vector3(
                playerCollider.center.x,
                originalCenterY * loweredHeightMultiplier,
                playerCollider.center.z
            );

            yield return new WaitForSeconds(lowerDuration);

            animator.SetBool("isLowering", false);

            playerCollider.height = originalHeight;
            playerCollider.center = new Vector3(
                playerCollider.center.x,
                originalCenterY,
                playerCollider.center.z
            );

            // Reset variables for fallback case
            yield return new WaitForSeconds(1f);
            canInteractWithShadow = true;
        }

        isLowering = false;
        animationPlayed = false;
    }

    public void MyInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        // Check for key DOWN, not holding
        if (Input.GetKeyDown(jumpKey) && readyToJump && grounded && !isAttacking && !isSitting && !isLowering)
        {
            readyToJump = false;
            isJumping = true;
            Jump();
            Invoke(nameof(ResetJump), jumpCoolDown);
        }

        if (Input.GetKeyDown(attackKey) && grounded && canAttack && !isDead && !isJumping && !isSitting && !isLowering)
        {
            Attack();
        }

        // Check for interact key to lower down through shadow
        if (Input.GetKeyDown(interactKey) && isLookingAtShadow && !isLowering && !isAttacking && !isSitting && canInteractWithShadow)
        {
            StartCoroutine(DoLowerAnimation());
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

        // Only check for shadow if we can interact and not currently lowering
        if (!isLowering && canInteractWithShadow)
        {
            CheckForShadow();
        }

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
    }

    private void CheckForShadow()
    {
        RaycastHit hit;
        Vector3 origin = orientation.position;
        Vector3 dir = orientation.forward;

        // Debug visualization
        Debug.DrawRay(origin, dir * raycastDistance, isLookingAtShadow ? Color.green : Color.yellow);

        // Store previous state
        bool wasLookingAtShadow = isLookingAtShadow;

        // Reset detection first
        isLookingAtShadow = false;

        if (Physics.Raycast(origin, dir, out hit, raycastDistance, shadowLayerMask))
        {
            if (hit.collider.CompareTag("ToiletDoor") && hit.collider.enabled)
            {
                isLookingAtShadow = true;
                detectedShadow = hit.collider.gameObject;
                shadowCollider = hit.collider;
            }
        }

        // If we're no longer looking at a valid shadow, clear references
        if (!isLookingAtShadow && (detectedShadow != null || shadowCollider != null))
        {
            detectedShadow = null;
            shadowCollider = null;
        }
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
        if (!isMoving && grounded && !isDead && !isSitting && !isAttacking && !isLowering)
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
        animator.SetBool("IsLowering", isLowering);
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
        // Reset shadow detection when player dies
        detectedShadow = null;
        shadowCollider = null;
        isLookingAtShadow = false;
        canInteractWithShadow = true;
    }

    public void Respawn()
    {
        isDead = false;
        isAttacking = false;
        canAttack = true;
        isJumping = false;
        isSitting = false;
        isLowering = false;
        idleTimer = 0f;
        isLookingAtShadow = false;
        detectedShadow = null;
        shadowCollider = null;
        canInteractWithShadow = true;
    }

    void OnGUI()
    {
#if UNITY_EDITOR
        GUI.Label(new Rect(10, 100, 300, 20), $"Idle Timer: {idleTimer:F1}/{idleTimeBeforeSit}");
        GUI.Label(new Rect(10, 120, 300, 20), $"Is Moving: {isMoving}");
        GUI.Label(new Rect(10, 140, 300, 20), $"Is Sitting: {isSitting}");
        GUI.Label(new Rect(10, 160, 300, 20), $"Looking at Shadow: {isLookingAtShadow}");
        GUI.Label(new Rect(10, 180, 300, 20), $"Is Lowering: {isLowering}");
        GUI.Label(new Rect(10, 200, 300, 20), $"Can Interact: {canInteractWithShadow}");
#endif

        // Interaction prompt for player
        if (isLookingAtShadow && !isLowering && canInteractWithShadow)
        {
            GUIStyle style = new GUIStyle(GUI.skin.box);
            style.alignment = TextAnchor.MiddleCenter;
            style.fontSize = 20;
            style.normal.textColor = Color.white;
            style.normal.background = MakeTexture(2, 2, new Color(0, 0, 0, 0.8f));

            GUI.Box(new Rect(Screen.width / 2 - 150, Screen.height / 2 + 50, 300, 40), "Press E to Lower Down", style);
        }
    }

    // Helper method to create texture for GUI background
    private Texture2D MakeTexture(int width, int height, Color color)
    {
        Color[] pix = new Color[width * height];
        for (int i = 0; i < pix.Length; i++)
            pix[i] = color;
        Texture2D result = new Texture2D(width, height);
        result.SetPixels(pix);
        result.Apply();
        return result;
    }
}