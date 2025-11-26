using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace VeilOfColours.Players
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerMovement : NetworkBehaviour
    {
        private const float GamepadDeadZone = 0.1f;
        private const float WalkingThreshold = 0.01f;

        [Header("Input Actions")]
        [SerializeField]
        private InputActionReference moveAction;

        [SerializeField]
        private InputActionReference jumpAction;

        [SerializeField]
        private InputActionReference dashAction;

        [SerializeField]
        private InputActionReference climbAction;

        [Header("Movement Settings")]
        [SerializeField]
        private float maxSpeed = 7f;

        [SerializeField]
        private float acceleration = 50f;

        [SerializeField]
        private float deceleration = 60f;

        [SerializeField]
        private float airAcceleration = 30f;

        [SerializeField]
        private float airDeceleration = 30f;

        [Header("Jump Settings")]
        [SerializeField]
        private float jumpForce = 15f;

        [SerializeField]
        private float jumpCutMultiplier = 0.5f;

        [SerializeField]
        private float coyoteTime = 0.15f;

        [SerializeField]
        private float jumpBufferTime = 0.15f;

        [Header("Dash Settings")]
        [SerializeField]
        private float dashSpeed = 20f;

        [SerializeField]
        private float dashDuration = 0.15f;

        [SerializeField]
        private float dashCooldown = 0.8f;

        [SerializeField]
        private bool dashResetOnGround = true;

        [Header("Climb Settings")]
        [SerializeField]
        private float climbSpeed = 5f;

        [SerializeField]
        private float maxClimbStamina = 100f;

        [SerializeField]
        private float climbStaminaDrainRate = 25f; // Per second

        [SerializeField]
        private float climbStaminaRegenRate = 50f; // Per second

        [SerializeField]
        private float climbStaminaRegenDelay = 0.5f; // Delay before instant refill

        [Header("Gravity Settings")]
        [SerializeField]
        private float fallMultiplier = 2.5f;

        [SerializeField]
        private float lowJumpMultiplier = 2f;

        [SerializeField]
        private float maxFallSpeed = 20f;

        [Header("Ground Check")]
        [SerializeField]
        private Transform groundCheck;

        [SerializeField]
        private float groundCheckRadius = 0.2f;

        [SerializeField]
        private LayerMask groundLayer;

        [Header("Wall Check")]
        [SerializeField]
        private Transform wallCheckFront;

        [SerializeField]
        private float wallCheckDistance = 0.5f;

        [Header("Camera Reference")]
        [SerializeField]
        private Camera playerCamera;

        private Rigidbody2D rb;
        private Animator animator;
        private bool isGrounded;
        private bool wasGrounded;
        private Vector2 moveInput;

        private float coyoteTimeCounter;

        private float jumpBufferCounter;

        private bool isJumping;
        private bool jumpReleased = true;

        // Dash state
        private bool isDashing;
        private bool canDash = true;
        private bool hasAirDash = true; // Track if air dash is available
        private float dashTimeLeft;
        private float dashCooldownTimer;
        private Vector2 dashDirection;

        // Climb state
        private bool isClimbing;
        private bool isTouchingWall;
        private int wallDirection; // -1 for left, 1 for right
        private float currentClimbStamina;
        private bool canRegenClimbStamina;
        private float climbStaminaRegenTimer;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            animator = GetComponent<Animator>();
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            currentClimbStamina = maxClimbStamina;
        }

        private void OnEnable()
        {
            if (moveAction != null)
                moveAction.action.Enable();
            if (jumpAction != null)
                jumpAction.action.Enable();
            if (dashAction != null)
                dashAction.action.Enable();
            if (climbAction != null)
                climbAction.action.Enable();
        }

        private void OnDisable()
        {
            if (moveAction != null)
                moveAction.action.Disable();
            if (jumpAction != null)
                jumpAction.action.Disable();
            if (dashAction != null)
                dashAction.action.Disable();
            if (climbAction != null)
                climbAction.action.Disable();
        }

        private void Update()
        {
            if (!IsOwner)
                return;

            ReadInput();
            UpdateTimers();
        }

        private void ReadInput()
        {
            // Read movement from Input Action
            if (moveAction != null)
            {
                Vector2 move = moveAction.action.ReadValue<Vector2>();
                moveInput = new Vector2(move.x, 0);
            }
            else
            {
                moveInput = Vector2.zero;
            }

            // Jump buffer
            if (jumpAction != null && jumpAction.action.WasPressedThisFrame())
            {
                jumpBufferCounter = jumpBufferTime;
            }

            if (jumpAction != null && jumpAction.action.WasReleasedThisFrame())
            {
                jumpReleased = true;
            }

            // Dash input
            if (dashAction != null && dashAction.action.WasPressedThisFrame() && canDash)
            {
                StartDash();
            }

            // Climb input
            if (climbAction != null && climbAction.action.IsPressed())
            {
                TryStartClimb();
            }
        }

        private void UpdateTimers()
        {
            // Coyote time
            if (isGrounded)
            {
                coyoteTimeCounter = coyoteTime;
            }
            else
            {
                coyoteTimeCounter -= Time.deltaTime;
            }

            // Jump buffer
            jumpBufferCounter -= Time.deltaTime;

            // Dash cooldown
            if (dashCooldownTimer > 0)
            {
                dashCooldownTimer -= Time.deltaTime;
                if (dashCooldownTimer <= 0)
                {
                    canDash = true;
                }
            }

            // Climb stamina regeneration (instant refill after delay when grounded)
            if (canRegenClimbStamina && isGrounded)
            {
                climbStaminaRegenTimer -= Time.deltaTime;
                if (climbStaminaRegenTimer <= 0)
                {
                    currentClimbStamina = maxClimbStamina; // Instant refill
                    canRegenClimbStamina = false; // Stop checking
                }
            }
        }

        private void FixedUpdate()
        {
            if (!IsOwner)
                return;

            wasGrounded = isGrounded;
            CheckGroundState();

            if (isDashing)
            {
                HandleDash();
            }
            else if (isClimbing)
            {
                HandleClimb();
            }
            else
            {
                HandleJump();
                ApplyMovement();
                ApplyGravityModifiers();
            }

            UpdateAnimation();
        }

        private void CheckGroundState()
        {
            isGrounded = Physics2D.OverlapCircle(
                groundCheck.position,
                groundCheckRadius,
                groundLayer
            );

            // Check for walls (uses same layer as ground since tilemap is both)
            if (wallCheckFront != null)
            {
                RaycastHit2D hit = Physics2D.Raycast(
                    wallCheckFront.position,
                    Vector2.right * Mathf.Sign(transform.localScale.x),
                    wallCheckDistance,
                    groundLayer
                );
                isTouchingWall = hit.collider != null;
                wallDirection = (int)Mathf.Sign(transform.localScale.x);
            }

            // Reset jump state when landing
            if (!wasGrounded && isGrounded)
            {
                isJumping = false;
                jumpReleased = true;

                // Reset air dash when landing
                if (dashResetOnGround)
                {
                    hasAirDash = true;
                }

                // Stop climbing when landing
                isClimbing = false;

                // Start climb stamina regeneration timer when landing
                canRegenClimbStamina = true;
                climbStaminaRegenTimer = climbStaminaRegenDelay;
            }
        }

        private void HandleJump()
        {
            // Jump if: has jump buffered AND (grounded OR within coyote time)
            if (jumpBufferCounter > 0f && coyoteTimeCounter > 0f && !isJumping)
            {
                Jump();
                jumpBufferCounter = 0f;
                coyoteTimeCounter = 0f;
            }

            // Variable jump height - cut jump short when button released
            if (jumpReleased && rb.linearVelocity.y > 0f && isJumping)
            {
                rb.linearVelocity = new Vector2(
                    rb.linearVelocity.x,
                    rb.linearVelocity.y * jumpCutMultiplier
                );
            }
        }

        private void ApplyMovement()
        {
            float targetSpeed = moveInput.x * maxSpeed;
            float currentSpeed = rb.linearVelocity.x;

            // Choose acceleration/deceleration based on ground state
            float accelRate;
            if (Mathf.Abs(moveInput.x) > 0.01f)
            {
                accelRate = isGrounded ? acceleration : airAcceleration;
            }
            else
            {
                accelRate = isGrounded ? deceleration : airDeceleration;
            }

            // Smooth acceleration instead of instant speed
            float speedDiff = targetSpeed - currentSpeed;
            float movement = speedDiff * accelRate * Time.fixedDeltaTime;

            rb.linearVelocity = new Vector2(currentSpeed + movement, rb.linearVelocity.y);
        }

        private void ApplyGravityModifiers()
        {
            // Apply different gravity when falling vs rising for better feel
            if (rb.linearVelocity.y < 0)
            {
                // Falling - stronger gravity for snappier feel
                rb.linearVelocity +=
                    Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime;
            }
            else if (rb.linearVelocity.y > 0 && jumpReleased)
            {
                // Rising but jump released - moderate gravity for low jump
                rb.linearVelocity +=
                    Vector2.up
                    * Physics2D.gravity.y
                    * (lowJumpMultiplier - 1)
                    * Time.fixedDeltaTime;
            }

            // Clamp fall speed
            if (rb.linearVelocity.y < -maxFallSpeed)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, -maxFallSpeed);
            }
        }

        private void UpdateAnimation()
        {
            if (animator == null)
                return;

            bool isWalking = Mathf.Abs(rb.linearVelocity.x) > WalkingThreshold;
            animator.SetBool("isWalking", isWalking);
        }

        private void Jump()
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            isJumping = true;
            jumpReleased = false;
        }

        private void StartDash()
        {
            // Can only dash if: cooldown is done AND (grounded OR has air dash)
            bool canDashNow = canDash && (isGrounded || hasAirDash);
            if (!canDashNow || isDashing)
                return;

            // Get dash direction from move input
            Vector2 inputDirection = Vector2.zero;
            if (moveAction != null)
            {
                inputDirection = moveAction.action.ReadValue<Vector2>();
            }

            // Default to horizontal dash if no input
            if (inputDirection.magnitude < 0.1f)
            {
                // Dash in the direction player is facing (use last movement or default right)
                inputDirection.x = moveInput.x != 0 ? Mathf.Sign(moveInput.x) : 1f;
            }

            dashDirection = inputDirection.normalized;
            isDashing = true;
            dashTimeLeft = dashDuration;
            canDash = false;
            dashCooldownTimer = dashCooldown;

            // Use up air dash if in air
            if (!isGrounded)
            {
                hasAirDash = false;
            }

            // Cancel current velocity for clean dash
            rb.linearVelocity = Vector2.zero;
        }

        private void HandleDash()
        {
            dashTimeLeft -= Time.fixedDeltaTime;

            if (dashTimeLeft <= 0)
            {
                isDashing = false;
                // Small velocity preservation at end of dash
                rb.linearVelocity = dashDirection * dashSpeed * 0.3f;
                return;
            }

            // Apply dash velocity (ignore gravity during dash)
            rb.linearVelocity = dashDirection * dashSpeed;
        }

        private void TryStartClimb()
        {
            // Can only climb if touching wall and has stamina
            if (isTouchingWall && !isGrounded && currentClimbStamina > 0)
            {
                isClimbing = true;
                isDashing = false; // Cancel dash if climbing
                canRegenClimbStamina = false; // Stop regen while climbing
            }
        }

        private void HandleClimb()
        {
            // Drain stamina
            currentClimbStamina -= climbStaminaDrainRate * Time.fixedDeltaTime;

            // Stop climbing if out of stamina or no longer touching wall
            if (currentClimbStamina <= 0 || !isTouchingWall)
            {
                isClimbing = false;
                currentClimbStamina = Mathf.Max(currentClimbStamina, 0);
                return;
            }

            // Check if climb button is released
            bool climbHeld = climbAction != null && climbAction.action.IsPressed();

            if (!climbHeld)
            {
                isClimbing = false;
                return;
            }

            // Get vertical input from move action
            float verticalInput = 0f;
            if (moveAction != null)
            {
                Vector2 move = moveAction.action.ReadValue<Vector2>();
                verticalInput = move.y;
            }

            // Apply climb velocity (very little gravity, stick to wall)
            rb.linearVelocity = new Vector2(0, verticalInput * climbSpeed);

            // Reset air dash while climbing
            hasAirDash = true;
        }

        // Visualize ground check in editor
        private void OnDrawGizmosSelected()
        {
            if (groundCheck != null)
            {
                Gizmos.color = isGrounded ? Color.green : Color.red;
                Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
            }

            if (wallCheckFront != null)
            {
                Gizmos.color = isTouchingWall ? Color.blue : Color.yellow;
                Vector3 direction = Vector3.right * Mathf.Sign(transform.localScale.x);
                Gizmos.DrawRay(wallCheckFront.position, direction * wallCheckDistance);
            }
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            if (IsOwner)
            {
                AssignCamera();
            }
            else
            {
                DisableNonOwnerCamera();
            }
        }

        private void AssignCamera()
        {
            if (playerCamera != null)
                playerCamera.enabled = true;
        }

        private void DisableNonOwnerCamera()
        {
            if (playerCamera != null)
                playerCamera.enabled = false;
        }

        // Public getter for UI
        public float GetCurrentClimbStamina()
        {
            return currentClimbStamina;
        }
    }
}
