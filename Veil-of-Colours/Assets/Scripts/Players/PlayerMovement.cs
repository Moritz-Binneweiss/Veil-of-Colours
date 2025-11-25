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

                // Jump buffer - register jump input even if not grounded yet
                if (Keyboard.current.spaceKey.wasPressedThisFrame)
                {
                    jumpBufferCounter = jumpBufferTime;
                }

                // Track jump release for variable jump height
                if (Keyboard.current.spaceKey.wasReleasedThisFrame)
                {
                    jumpReleased = true;
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
            // Coyote time - grace period after leaving ground
            if (isGrounded)
            {
                coyoteTimeCounter = coyoteTime;
            }
            else
            {
                coyoteTimeCounter -= Time.deltaTime;
            }

            // Jump buffer countdown
            jumpBufferCounter -= Time.deltaTime;
        }

        private void FixedUpdate()
        {
            if (!IsOwner)
                return;

            wasGrounded = isGrounded;
            CheckGroundState();
            HandleJump();
            ApplyMovement();
            ApplyGravityModifiers();
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
