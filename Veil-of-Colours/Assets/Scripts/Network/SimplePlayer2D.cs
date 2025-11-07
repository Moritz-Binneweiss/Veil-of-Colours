using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace VeilOfColours.Network
{
    /// <summary>
    /// Simple 2D player controller with network support
    /// Each player controls their own character locally
    /// No need to sync position since players are in separate levels
    /// </summary>
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

        private Rigidbody2D rb;
        private bool isGrounded;
        private float horizontalInput;
        private bool jumpPressed;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
        }

        private void Update()
        {
            // Only allow input for the local player
            if (!IsOwner)
                return;

            // Get horizontal input (A/D or Arrow keys)
            horizontalInput = 0f;
            if (Keyboard.current != null)
            {
                if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
                    horizontalInput = -1f;
                else if (
                    Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed
                )
                    horizontalInput = 1f;
            }

            // Jump input (Space)
            if (
                Keyboard.current != null
                && Keyboard.current.spaceKey.wasPressedThisFrame
                && isGrounded
            )
            {
                Jump();
            }
        }

        private void FixedUpdate()
        {
            // Only move the local player
            if (!IsOwner)
                return;

            // Check if grounded
            isGrounded = Physics2D.OverlapCircle(
                groundCheck.position,
                groundCheckRadius,
                groundLayer
            );

            // Apply movement
            rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y);
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

            // Assign player to their respective level based on network ID
            if (IsOwner)
            {
                AssignPlayerToLevel();
            }
        }

        private void AssignPlayerToLevel()
        {
            // Player 1 (Host) goes to Level A spawn
            // Player 2 (Client) goes to Level B spawn

            GameObject spawnPoint = IsServer
                ? GameObject.FindGameObjectWithTag("SpawnA")
                : GameObject.FindGameObjectWithTag("SpawnB");

            if (spawnPoint != null)
            {
                transform.position = spawnPoint.transform.position;
                Debug.Log(
                    $"[SimplePlayer2D] Player spawned at {(IsServer ? "Level A" : "Level B")}"
                );
            }
            else
            {
                Debug.LogWarning(
                    "[SimplePlayer2D] No spawn point found! Tag your spawn points as 'SpawnA' or 'SpawnB'"
                );
            }
        }
    }
}
