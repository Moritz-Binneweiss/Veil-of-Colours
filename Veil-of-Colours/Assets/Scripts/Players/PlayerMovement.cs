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
        private float moveSpeed = 5f;

        [SerializeField]
        private float jumpForce = 10f;

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
        private Vector2 moveInput;

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
        }

        private void ReadInput()
        {
            moveInput = Vector2.zero;

            if (Keyboard.current != null)
            {
                moveInput.x = GetKeyboardInput();

                if (Keyboard.current.spaceKey.wasPressedThisFrame && isGrounded)
                    Jump();
            }

            if (Gamepad.current != null)
            {
                Vector2 stickInput = Gamepad.current.leftStick.ReadValue();
                if (Mathf.Abs(stickInput.x) > GamepadDeadZone)
                    moveInput.x = stickInput.x;

                if (Gamepad.current.buttonSouth.wasPressedThisFrame && isGrounded)
                    Jump();
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

        private void FixedUpdate()
        {
            if (!IsOwner)
                return;

            CheckGroundState();
            ApplyMovement();
            UpdateAnimation();
        }

        private void CheckGroundState()
        {
            isGrounded = Physics2D.OverlapCircle(
                groundCheck.position,
                groundCheckRadius,
                groundLayer
            );
        }

        private void ApplyMovement()
        {
            rb.linearVelocity = new Vector2(moveInput.x * moveSpeed, rb.linearVelocity.y);
        }

        private void UpdateAnimation()
        {
            if (animator == null)
                return;

            bool isWalking = Mathf.Abs(moveInput.x) > WalkingThreshold;
            animator.SetBool("isWalking", isWalking);
        }

        private void Jump()
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
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
