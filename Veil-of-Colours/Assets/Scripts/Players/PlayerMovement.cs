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

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            animator = GetComponent<Animator>();
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
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
            moveInput = Vector2.zero;

            if (Keyboard.current != null)
            {
                moveInput.x = GetKeyboardInput();

                // Jump buffer
                if (Keyboard.current.spaceKey.wasPressedThisFrame)
                {
                    jumpBufferCounter = jumpBufferTime;
                }

                if (Keyboard.current.spaceKey.wasReleasedThisFrame)
                {
                    jumpReleased = true;
                }

                // Dash input (Left Shift or X key)
                if (
                    (
                        Keyboard.current.leftShiftKey.wasPressedThisFrame
                        || Keyboard.current.xKey.wasPressedThisFrame
                    ) && canDash
                )
                {
                    StartDash();
                }
            }

            if (Gamepad.current != null)
            {
                Vector2 stickInput = Gamepad.current.leftStick.ReadValue();
                if (Mathf.Abs(stickInput.x) > GamepadDeadZone)
                    moveInput.x = stickInput.x;

                if (Gamepad.current.buttonSouth.wasPressedThisFrame)
                {
                    jumpBufferCounter = jumpBufferTime;
                }

                if (Gamepad.current.buttonSouth.wasReleasedThisFrame)
                {
                    jumpReleased = true;
                }

                // Dash input (Right Bumper or X button)
                if (
                    (
                        Gamepad.current.rightShoulder.wasPressedThisFrame
                        || Gamepad.current.buttonWest.wasPressedThisFrame
                    ) && canDash
                )
                {
                    StartDash();
                }
            }
        }

        private float GetKeyboardInput()
        {
            bool leftPressed =
                Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed;
            bool rightPressed =
                Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed;

            if (leftPressed && !rightPressed)
                return -1f;
            if (rightPressed && !leftPressed)
                return 1f;
            return 0f;
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

            // Get dash direction based on input
            Vector2 inputDirection = Vector2.zero;

            if (Keyboard.current != null)
            {
                if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed)
                    inputDirection.y = 1;
                if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed)
                    inputDirection.y = -1;
                if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
                    inputDirection.x = -1;
                if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
                    inputDirection.x = 1;
            }

            if (Gamepad.current != null)
            {
                Vector2 stick = Gamepad.current.leftStick.ReadValue();
                if (Mathf.Abs(stick.x) > GamepadDeadZone || Mathf.Abs(stick.y) > GamepadDeadZone)
                {
                    inputDirection = stick;
                }
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

        // Visualize ground check in editor
        private void OnDrawGizmosSelected()
        {
            if (groundCheck != null)
            {
                Gizmos.color = isGrounded ? Color.green : Color.red;
                Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
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
    }
}
