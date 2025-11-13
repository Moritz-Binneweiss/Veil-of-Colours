using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace VeilOfColours.Players
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class SimplePlayer2D : NetworkBehaviour
    {
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
        private bool isGrounded;
        private Vector2 moveInput;
        private Animator animator;

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

            moveInput = Vector2.zero;

            var keyboard = Keyboard.current;
            if (keyboard != null)
            {
                if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed)
                    moveInput.x = -1f;
                else if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed)
                    moveInput.x = 1f;

                if (keyboard.spaceKey.wasPressedThisFrame && isGrounded)
                    Jump();
            }

            var gamepad = Gamepad.current;
            if (gamepad != null)
            {
                var stickInput = gamepad.leftStick.ReadValue();
                if (Mathf.Abs(stickInput.x) > 0.1f)
                    moveInput.x = stickInput.x;

                if (gamepad.buttonSouth.wasPressedThisFrame && isGrounded)
                    Jump();
            }
        }

        private void FixedUpdate()
        {
            if (!IsOwner)
                return;

            isGrounded = Physics2D.OverlapCircle(
                groundCheck.position,
                groundCheckRadius,
                groundLayer
            );

            rb.linearVelocity = new Vector2(moveInput.x * moveSpeed, rb.linearVelocity.y);

            if (animator != null)
            {
                bool isWalking = Mathf.Abs(moveInput.x) > 0.01f;
                animator.SetBool("isWalking", isWalking);
            }
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
