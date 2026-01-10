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

        private bool movementEnabled = true;

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

        [SerializeField]
        private TrailRenderer dashTrail;

        [Header("Climb Settings")]
        [SerializeField]
        private float climbSpeed = 5f;

        [SerializeField]
        private float maxClimbStamina = 100f;

        [SerializeField]
        private float climbStaminaDrainRate = 25f; // Per second when moving

        [SerializeField]
        private float climbStaminaHangRate = 5f; // Per second when hanging still

        // Removed climbStaminaRegenRate - instant refill used instead

        [SerializeField]
        private float climbStaminaRegenDelay = 0.5f; // Delay before instant refill

        [SerializeField]
        private float ledgeClimbBoost = 8f; // Upward boost when reaching ledge top

        // Removed ledgeCheckDistance - using fixed distance in code

        [SerializeField]
        private float wallJumpForce = 15f; // Wall jump vertical force

        [SerializeField]
        private float wallJumpPush = 8f; // Wall jump horizontal push away from wall

        [SerializeField]
        private float wallSlideSpeed = 2f; // Speed when sliding down wall without climbing

        [SerializeField]
        private float climbJumpStaminaCost = 20f; // Stamina cost for vertical climb jump

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

        [Header("Visual Settings")]
        [SerializeField]
        private float flipSpeed = 20f; // How fast the player flips (higher = faster)

        [Header("Camera Reference")]
        [SerializeField]
        private CameraFollow cameraFollow;

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
        private bool isCrabing; // For crab animation state
        private bool isTouchingWall;
        private bool isWallSliding;
        private int wallDirection; // -1 for left, 1 for right
        private float currentClimbStamina;
        private bool canRegenClimbStamina;
        private float climbStaminaRegenTimer;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            animator = GetComponent<Animator>();
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            rb.gravityScale = 3;
            currentClimbStamina = maxClimbStamina;

            // Find camera if not assigned
            if (cameraFollow == null)
            {
                cameraFollow = FindFirstObjectByType<CameraFollow>();
            }
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            Debug.Log($"PlayerMovement spawned. ClientId: {OwnerClientId}, IsOwner: {IsOwner}");
        }

        public void FreezeMovementForDuration(float duration)
        {
            if (gameObject.activeInHierarchy)
            {
                StartCoroutine(FreezeMovementCoroutine(duration));
            }
        }

        private System.Collections.IEnumerator FreezeMovementCoroutine(float duration)
        {
            movementEnabled = false;
            rb.linearVelocity = Vector2.zero;
            yield return new WaitForSeconds(duration);
            movementEnabled = true;
        }

        private void OnEnable()
        {
            if (!IsOwner)
                return;

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
            if (!IsOwner)
                return;

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

            if (!movementEnabled)
                return;

            ReadInput();
            UpdateTimers();
        }

        private void ReadInput()
        {
            // Read movement from Input Action
            if (moveAction != null && moveAction.action != null)
            {
                Vector2 move = moveAction.action.ReadValue<Vector2>();
                moveInput = new Vector2(move.x, 0);
            }
            else
            {
                if (moveAction == null)
                {
                    Debug.LogWarning($"MoveAction is null for player {OwnerClientId}");
                }
                moveInput = Vector2.zero;
            }

            // Jump buffer
            if (jumpAction != null && jumpAction.action != null)
            {
                if (jumpAction.action.WasPressedThisFrame())
                {
                    Debug.Log($"[Player {OwnerClientId}] JUMP PRESSED! Buffer set to {jumpBufferTime}");
                    jumpBufferCounter = jumpBufferTime;
                }

                if (jumpAction.action.WasReleasedThisFrame())
                {
                    Debug.Log($"[Player {OwnerClientId}] Jump released");
                    jumpReleased = true;
                }
            }
            else
            {
                Debug.LogWarning($"[Player {OwnerClientId}] jumpAction is null!");
            }

            // Dash input
            if (dashAction != null && dashAction.action.WasPressedThisFrame())
            {
                Debug.Log($"[Player {OwnerClientId}] Dash pressed! canDash={canDash}");
                if (canDash)
                {
                    StartDash();
                }
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

            if (!movementEnabled)
            {
                rb.linearVelocity = Vector2.zero;
                return;
            }

            wasGrounded = isGrounded;
            CheckGroundState();

            // Check for wall jump FIRST (before any other state)
            if (jumpBufferCounter > 0f && (isClimbing || isWallSliding) && isTouchingWall)
            {
                WallJump(isClimbing); // Pass whether we're climbing
                jumpBufferCounter = 0f;
            }
            else if (isDashing)
            {
                HandleDash();
            }
            else if (isClimbing)
            {
                HandleClimb();
            }
            else if (isWallSliding)
            {
                HandleWallSlide();
            }
            else
            {
                HandleJump();
                ApplyMovement();
                ApplyGravityModifiers();
            }

            // Check for wall slide (touching wall, moving towards it, not grounded, not climbing)
            CheckWallSlide();

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
                isWallSliding = false;

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

            // Set animation states based on priority
            animator.SetBool("isDashing", isDashing);
            animator.SetBool("isClimbing", isClimbing);
            animator.SetBool("isCrabing", isCrabing);

            // Walking only when on ground and not climbing/dashing
            bool isWalking =
                isGrounded
                && !isClimbing
                && !isDashing
                && Mathf.Abs(rb.linearVelocity.x) > WalkingThreshold;
            animator.SetBool("isWalking", isWalking);

            // Jump animation: true when in air and not climbing/dashing
            bool shouldShowJump = !isGrounded && !isClimbing && !isDashing;
            animator.SetBool("isJumping", shouldShowJump);

            // Optional: send Y velocity for rise/fall animations
            // TODO: Add 'yVelocity' parameter to Animator Controller
            // animator.SetFloat("yVelocity", rb.linearVelocity.y);

            // Smooth flip player sprite based on movement direction
            float targetScaleX = transform.localScale.x;

            if (moveInput.x > 0.01f)
            {
                targetScaleX = 1; // Face right
            }
            else if (moveInput.x < -0.01f)
            {
                targetScaleX = -1; // Face left
            }

            // Lerp to target scale for smooth flip
            float currentScaleX = transform.localScale.x;
            float newScaleX = Mathf.Lerp(currentScaleX, targetScaleX, flipSpeed * Time.deltaTime);
            transform.localScale = new Vector3(newScaleX, 1, 1);
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

            // Aktiviere Trail während Dash
            if (dashTrail != null)
            {
                dashTrail.emitting = true;
            // Apply camera shake and gamepad vibration (only for local player)
            if (IsOwner)
            {
                // Trigger camera shake
                if (cameraFollow != null)
                {
                    cameraFollow.TriggerShake(dashShakeIntensity, dashShakeDuration);
                }

                // Trigger gamepad vibration
                TriggerGamepadVibration(dashVibrationIntensity, dashVibrationDuration);
            }
            }
        }

        private void HandleDash()
        {
            dashTimeLeft -= Time.fixedDeltaTime;

            if (dashTimeLeft <= 0)
            {
                isDashing = false;
                // Small velocity preservation at end of dash
                rb.linearVelocity = dashDirection * dashSpeed * 0.3f;

                // Deaktiviere Trail wenn Dash endet
                if (dashTrail != null)
                {
                    dashTrail.emitting = false;
                }
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
            else
            {
                isCrabing = false; // Not crabing if not climbing
            }
        }

        private void HandleClimb()
        {
            // Get vertical input from move action
            float verticalInput = 0f;
            if (moveAction != null)
            {
                Vector2 move = moveAction.action.ReadValue<Vector2>();
                verticalInput = move.y;
            }

            // Dynamic stamina drain: more when moving, less when hanging still
            float staminaDrain =
                Mathf.Abs(verticalInput) > 0.1f ? climbStaminaDrainRate : climbStaminaHangRate;
            currentClimbStamina -= staminaDrain * Time.fixedDeltaTime;

            // Stop climbing if out of stamina or no longer touching wall
            if (currentClimbStamina <= 0 || !isTouchingWall)
            {
                isClimbing = false;
                isCrabing = false;
                rb.gravityScale = 3; // Restore gravity
                currentClimbStamina = Mathf.Max(currentClimbStamina, 0);
                return;
            }

            // Check if climb button is released
            bool climbHeld = climbAction != null && climbAction.action.IsPressed();

            if (!climbHeld)
            {
                isClimbing = false;
                isCrabing = false;
                rb.gravityScale = 3; // Restore gravity
                return;
            }

            // Check for ledge climb (climbing up and wall ends but ground above exists)
            if (verticalInput > 0.1f && wallCheckFront != null)
            {
                // Check if wall continues above (check slightly above player)
                Vector2 wallCheckAbove = (Vector2)wallCheckFront.position + Vector2.up * 0.5f;
                RaycastHit2D wallAboveHit = Physics2D.Raycast(
                    wallCheckAbove,
                    Vector2.right * wallDirection,
                    wallCheckDistance,
                    groundLayer
                );
                bool wallAbove = wallAboveHit.collider != null;

                // If no wall above = we're at the ledge top, boost up AND forward!
                if (!wallAbove)
                {
                    // Push player up and away from wall to get over ledge
                    float forwardPush = wallDirection * 3f;
                    rb.linearVelocity = new Vector2(forwardPush, ledgeClimbBoost);
                    isClimbing = false; // Stop climbing
                    rb.gravityScale = 3; // Restore gravity
                    jumpReleased = true; // Allow player control
                    return;
                }
            }

            // Apply climb velocity (stick to wall when still, move when input given)
            if (Mathf.Abs(verticalInput) > 0.01f)
            {
                rb.linearVelocity = new Vector2(0, verticalInput * climbSpeed);
                rb.gravityScale = 0; // No gravity while climbing
                isCrabing = false; // Not crabing when moving vertically
            }
            else
            {
                // Hang still on wall (completely frozen)
                rb.linearVelocity = Vector2.zero;
                rb.gravityScale = 0; // No gravity while hanging
                isCrabing = true; // Crabing when hanging still on wall
            }

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

        private void WallJump(bool fromClimb)
        {
            if (fromClimb)
            {
                // CLIMB JUMP: Check horizontal input to decide jump type
                float horizontalInput = moveInput.x;

                // If pushing away from wall: diagonal jump (no stamina cost)
                bool pushingAway =
                    (wallDirection > 0 && horizontalInput < -0.1f)
                    || (wallDirection < 0 && horizontalInput > 0.1f);

                if (pushingAway)
                {
                    // Diagonal jump away from wall
                    float pushDirection = -wallDirection;
                    rb.linearVelocity = new Vector2(pushDirection * wallJumpPush, wallJumpForce);
                }
                else
                {
                    // Vertical climb jump (costs stamina)
                    if (currentClimbStamina >= climbJumpStaminaCost)
                    {
                        currentClimbStamina -= climbJumpStaminaCost;
                        rb.linearVelocity = new Vector2(0, jumpForce); // Straight up
                    }
                    else
                    {
                        return; // Not enough stamina, cancel jump
                    }
                }
            }
            else
            {
                // WALL SLIDE JUMP: Always diagonal away from wall
                float pushDirection = -wallDirection;
                rb.linearVelocity = new Vector2(pushDirection * wallJumpPush, wallJumpForce);
            }

            // Stop climbing/sliding FIRST
            isClimbing = false;
            isWallSliding = false;
            rb.gravityScale = 3; // Restore gravity

            // Set jump state
            isJumping = true;
            jumpReleased = false;

            // Reset air dash
            hasAirDash = true;
        }

        private void CheckWallSlide()
        {
            // Only wall slide if: touching wall, not grounded, not climbing, not dashing, falling
            if (
                isTouchingWall
                && !isGrounded
                && !isClimbing
                && !isDashing
                && rb.linearVelocity.y < 0
            )
            {
                // Check if player is pushing towards wall
                bool pushingTowardsWall =
                    (wallDirection > 0 && moveInput.x > 0.1f)
                    || (wallDirection < 0 && moveInput.x < -0.1f);

                isWallSliding = pushingTowardsWall;
            }
            else
            {
                isWallSliding = false;
            }
        }

        private void HandleWallSlide()
        {
            // Slow down fall speed
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, -wallSlideSpeed);

            // Reset air dash while wall sliding
            hasAirDash = true;
        }

        // Public getter for UI
        public float GetCurrentClimbStamina()
        {
            return currentClimbStamina;
        }

        private void TriggerGamepadVibration(float intensity, float duration)
        {
            if (Gamepad.current != null)
            {
                Gamepad.current.SetMotorSpeeds(intensity, intensity);
                StartCoroutine(StopVibrationAfterDelay(duration));
            }
        }

        private System.Collections.IEnumerator StopVibrationAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            if (Gamepad.current != null)
            {
                Gamepad.current.SetMotorSpeeds(0, 0);
            }
        }
    }
}
