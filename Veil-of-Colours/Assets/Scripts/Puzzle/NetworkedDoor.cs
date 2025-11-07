using Unity.Netcode;
using UnityEngine;

namespace VeilOfColours.Puzzle
{
    /// <summary>
    /// A door that opens/closes based on networked puzzle state
    /// Reacts to switches being activated by either player
    /// </summary>
    public class NetworkedDoor : MonoBehaviour
    {
        [Header("Door Settings")]
        [SerializeField]
        private string doorId = "A"; // Which door in PuzzleManager (A, B, etc.)

        [Header("Animation Settings")]
        [SerializeField]
        private float moveDistance = 3f; // How far the door moves when opening

        [SerializeField]
        private float moveSpeed = 2f;

        [SerializeField]
        private bool moveVertically = true; // true = up/down, false = left/right

        [Header("Components")]
        [SerializeField]
        private Collider2D doorCollider;

        [SerializeField]
        private SpriteRenderer spriteRenderer;

        private Vector3 closedPosition;
        private Vector3 openPosition;
        private bool isOpen = false;
        private bool isMoving = false;

        private void Start()
        {
            Debug.Log($"[NetworkedDoor {doorId}] Starting initialization...");

            // Setup components
            if (doorCollider == null)
                doorCollider = GetComponent<Collider2D>();
            if (spriteRenderer == null)
                spriteRenderer = GetComponent<SpriteRenderer>();

            // Calculate positions
            closedPosition = transform.position;
            Vector3 moveDirection = moveVertically ? Vector3.up : Vector3.right;
            openPosition = closedPosition + (moveDirection * moveDistance);

            Debug.Log(
                $"[NetworkedDoor {doorId}] Closed pos: {closedPosition}, Open pos: {openPosition}"
            );
        }

        private void OnEnable()
        {
            Debug.Log(
                $"[NetworkedDoor {doorId}] OnEnable called. PuzzleManager.Instance = {PuzzleManager.Instance != null}"
            );

            // Subscribe to door state changes from PuzzleManager
            if (PuzzleManager.Instance != null)
            {
                SubscribeToDoorEvents();
            }
            else
            {
                Debug.LogWarning(
                    $"[NetworkedDoor {doorId}] PuzzleManager.Instance is NULL in OnEnable! Will try again..."
                );
                // Try to subscribe later when PuzzleManager becomes available
                StartCoroutine(WaitForPuzzleManager());
            }
        }

        private System.Collections.IEnumerator WaitForPuzzleManager()
        {
            Debug.Log($"[NetworkedDoor {doorId}] Waiting for PuzzleManager...");
            while (PuzzleManager.Instance == null)
            {
                yield return new WaitForSeconds(0.1f);
            }
            Debug.Log($"[NetworkedDoor {doorId}] PuzzleManager found! Subscribing now...");
            SubscribeToDoorEvents();

            // Check initial door state
            if (doorId == "A" && PuzzleManager.Instance.DoorAOpen.Value)
            {
                Debug.Log($"[NetworkedDoor {doorId}] Door was already open! Setting state...");
                OnDoorStateChanged(true);
            }
        }

        private void OnDisable()
        {
            if (PuzzleManager.Instance != null)
            {
                UnsubscribeFromDoorEvents();
            }
        }

        private void SubscribeToDoorEvents()
        {
            Debug.Log($"[NetworkedDoor {doorId}] Subscribing to door events...");
            switch (doorId)
            {
                case "A":
                    PuzzleManager.Instance.OnDoorAChanged += OnDoorStateChanged;
                    Debug.Log($"[NetworkedDoor {doorId}] Subscribed to OnDoorAChanged event!");
                    break;
            }
        }

        private void UnsubscribeFromDoorEvents()
        {
            switch (doorId)
            {
                case "A":
                    PuzzleManager.Instance.OnDoorAChanged -= OnDoorStateChanged;
                    break;
            }
        }

        private void OnDoorStateChanged(bool shouldOpen)
        {
            Debug.Log(
                $"[NetworkedDoor {doorId}] OnDoorStateChanged called! shouldOpen={shouldOpen}, isOpen={isOpen}"
            );
            if (shouldOpen != isOpen)
            {
                isOpen = shouldOpen;
                isMoving = true;
                Debug.Log($"[NetworkedDoor {doorId}] {(isOpen ? "Opening" : "Closing")} door");
            }
            else
            {
                Debug.Log($"[NetworkedDoor {doorId}] Door already in correct state, ignoring");
            }
        }

        private void Update()
        {
            if (!isMoving)
                return;

            // Animate door movement
            Vector3 targetPosition = isOpen ? openPosition : closedPosition;
            transform.position = Vector3.MoveTowards(
                transform.position,
                targetPosition,
                moveSpeed * Time.deltaTime
            );

            // Check if we've reached the target
            if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
            {
                transform.position = targetPosition;
                isMoving = false;

                // Update collider (disable when open)
                if (doorCollider != null)
                    doorCollider.enabled = !isOpen;

                Debug.Log($"[NetworkedDoor {doorId}] Door {(isOpen ? "opened" : "closed")}");
            }
        }

        // Visualize door movement in editor
        private void OnDrawGizmos()
        {
            if (!Application.isPlaying)
            {
                Vector3 startPos = transform.position;
                Vector3 moveDir = moveVertically ? Vector3.up : Vector3.right;
                Vector3 endPos = startPos + (moveDir * moveDistance);

                Gizmos.color = Color.green;
                Gizmos.DrawLine(startPos, endPos);
                Gizmos.DrawWireCube(endPos, Vector3.one * 0.5f);
            }
        }
    }
}
