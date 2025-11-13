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

        [Header("Camera Follow")]
        [SerializeField]
        private Vector3 cameraOffset = new Vector3(0, 2, -10);

        [SerializeField]
        private float cameraSmoothing = 5f;

        private Rigidbody2D rb;
        private bool isGrounded;
        private Vector2 moveInput;
        private Camera assignedCamera;
        private Animator animator;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            animator = GetComponent<Animator>();
            
            // Verhindere, dass der Spieler umkippt
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }

        private void Update()
        {
            if (!IsOwner)
                return;

            // Lese Movement Input (neues Input System - direkter Zugriff)
            moveInput = Vector2.zero;

            var keyboard = Keyboard.current;
            if (keyboard != null)
            {
                if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed)
                    moveInput.x = -1f;
                else if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed)
                    moveInput.x = 1f;

                // Jump mit Space
                if (keyboard.spaceKey.wasPressedThisFrame && isGrounded)
                {
                    Jump();
                }
            }

            // Gamepad Support (optional)
            var gamepad = Gamepad.current;
            if (gamepad != null)
            {
                var stickInput = gamepad.leftStick.ReadValue();
                if (Mathf.Abs(stickInput.x) > 0.1f)
                    moveInput.x = stickInput.x;

                if (gamepad.buttonSouth.wasPressedThisFrame && isGrounded)
                {
                    Jump();
                }
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

            // Nutze moveInput.x für horizontale Bewegung
            rb.linearVelocity = new Vector2(moveInput.x * moveSpeed, rb.linearVelocity.y);

            // Update animator
            if (animator != null)
            {
                bool isWalking = Mathf.Abs(moveInput.x) > 0.01f;
                animator.SetBool("isWalking", isWalking);
            }
        }

        private void LateUpdate()
        {
            if (!IsOwner || assignedCamera == null)
                return;

            // Smooth camera follow
            Vector3 targetPosition = transform.position + cameraOffset;
            assignedCamera.transform.position = Vector3.Lerp(
                assignedCamera.transform.position,
                targetPosition,
                Time.deltaTime * cameraSmoothing
            );
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
                assignedCamera = cameraObject.GetComponent<Camera>();
                if (assignedCamera != null)
                {
                    assignedCamera.enabled = true;
                    Debug.Log($"Activated camera: {cameraObject.name}");
                }
                else
                {
                    Debug.LogError($"No Camera component found on {cameraObject.name}!");
                }
            }
            else
            {
                Debug.LogError($"Camera with tag '{cameraTag}' not found!");
            }
        }

        private void DisableNonOwnerCamera()
        {
            // Stelle sicher, dass die andere Kamera deaktiviert bleibt
            string otherCameraTag = IsServer ? "CameraB" : "CameraA";
            GameObject otherCamera = GameObject.FindGameObjectWithTag(otherCameraTag);

            if (otherCamera != null)
            {
                Camera cam = otherCamera.GetComponent<Camera>();
                if (cam != null)
                {
                    cam.enabled = false;
                    Debug.Log($"Disabled non-owner camera: {otherCamera.name}");
                }
            }
        }
    }
}
