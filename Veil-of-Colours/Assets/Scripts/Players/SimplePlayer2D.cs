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

        private Rigidbody2D rb;
        private bool isGrounded;
        private float horizontalInput;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
        }

        private void Update()
        {
            if (!IsOwner)
                return;

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
            if (!IsOwner)
                return;

            isGrounded = Physics2D.OverlapCircle(
                groundCheck.position,
                groundCheckRadius,
                groundLayer
            );
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

            if (IsOwner)
            {
                AssignPlayerToLevel();
                AssignCamera();
            }
            else
            {
                DisableNonOwnerCamera();
            }
        }

        private void AssignPlayerToLevel()
        {
            GameObject spawnPoint = IsServer
                ? GameObject.FindGameObjectWithTag("SpawnA")
                : GameObject.FindGameObjectWithTag("SpawnB");

            if (spawnPoint != null)
            {
                transform.position = spawnPoint.transform.position;
            }
        }

        private void AssignCamera()
        {
            string cameraTag = IsServer ? "CameraA" : "CameraB";
            GameObject cameraObject = GameObject.FindGameObjectWithTag(cameraTag);

            if (cameraObject != null)
            {
                Camera cam = cameraObject.GetComponent<Camera>();
                if (cam != null)
                {
                    cam.enabled = true;
                    cam.rect = new Rect(0f, 0f, 1f, 1f);
                }
            }
        }

        private void DisableNonOwnerCamera()
        {
            string otherCameraTag = IsServer ? "CameraB" : "CameraA";
            GameObject otherCamera = GameObject.FindGameObjectWithTag(otherCameraTag);

            if (otherCamera != null)
            {
                Camera cam = otherCamera.GetComponent<Camera>();
                if (cam != null)
                {
                    cam.enabled = false;
                }
            }
        }
    }
}
