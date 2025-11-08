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
            if (doorCollider == null)
                doorCollider = GetComponent<Collider2D>();
            if (spriteRenderer == null)
                spriteRenderer = GetComponent<SpriteRenderer>();

            closedPosition = transform.position;
            Vector3 moveDirection = moveVertically ? Vector3.up : Vector3.right;
            openPosition = closedPosition + (moveDirection * moveDistance);
        }

        private void OnEnable()
        {
            if (PuzzleManager.Instance != null)
            {
                SubscribeToDoorEvents();
            }
            else
            {
                StartCoroutine(WaitForPuzzleManager());
            }
        }

        private System.Collections.IEnumerator WaitForPuzzleManager()
        {
            while (PuzzleManager.Instance == null)
            {
                yield return new WaitForSeconds(0.1f);
            }

            SubscribeToDoorEvents();

            if (doorId == "A" && PuzzleManager.Instance.DoorAOpen.Value)
            {
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
            switch (doorId)
            {
                case "A":
                    PuzzleManager.Instance.OnDoorAChanged += OnDoorStateChanged;
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
            if (shouldOpen != isOpen)
            {
                isOpen = shouldOpen;
                isMoving = true;
            }
        }

        private void Update()
        {
            if (!isMoving)
                return;

            Vector3 targetPosition = isOpen ? openPosition : closedPosition;
            transform.position = Vector3.MoveTowards(
                transform.position,
                targetPosition,
                moveSpeed * Time.deltaTime
            );

            if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
            {
                transform.position = targetPosition;
                isMoving = false;

                if (doorCollider != null)
                    doorCollider.enabled = !isOpen;
            }
        }

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
