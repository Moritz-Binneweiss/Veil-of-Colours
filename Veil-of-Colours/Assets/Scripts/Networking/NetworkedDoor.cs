using Unity.Netcode;
using UnityEngine;

namespace VeilOfColours.Puzzle
{
    /// <summary>
    /// Door that opens/closes based on networked puzzle state.
    /// </summary>
    public class NetworkedDoor : MonoBehaviour
    {
        private const float PuzzleManagerCheckInterval = 0.1f;
        private const float PositionTolerance = 0.01f;

        [Header("Door Settings")]
        [SerializeField]
        private string doorId = "A";

        [Header("Animation Settings")]
        [SerializeField]
        private float moveDistance = 3f;

        [SerializeField]
        private float moveSpeed = 2f;

        [SerializeField]
        private bool moveVertically = true;

        [Header("Components")]
        [SerializeField]
        private Collider2D doorCollider;

        [SerializeField]
        private SpriteRenderer spriteRenderer;

        private Vector3 closedPosition;
        private Vector3 openPosition;
        private bool isOpen;
        private bool isMoving;

        private void Start()
        {
            CacheComponents();
            CalculatePositions();
        }

        private void CacheComponents()
        {
            if (doorCollider == null)
                doorCollider = GetComponent<Collider2D>();
            if (spriteRenderer == null)
                spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void CalculatePositions()
        {
            closedPosition = transform.position;
            Vector3 moveDirection = moveVertically ? Vector3.up : Vector3.right;
            openPosition = closedPosition + moveDirection * moveDistance;
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
            var wait = new WaitForSeconds(PuzzleManagerCheckInterval);

            while (PuzzleManager.Instance == null)
                yield return wait;

            SubscribeToDoorEvents();
            InitializeDoorState();
        }

        private void InitializeDoorState()
        {
            bool shouldBeOpen = doorId switch
            {
                "A" => PuzzleManager.Instance.DoorAOpen.Value,
                "B" => PuzzleManager.Instance.DoorBOpen.Value,
                _ => false,
            };

            if (shouldBeOpen)
                OnDoorStateChanged(true);
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
                case "B":
                    PuzzleManager.Instance.OnDoorBChanged += OnDoorStateChanged;
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
                case "B":
                    PuzzleManager.Instance.OnDoorBChanged -= OnDoorStateChanged;
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

            if (Vector3.Distance(transform.position, targetPosition) < PositionTolerance)
            {
                OnReachedTarget(targetPosition);
            }
        }

        private void OnReachedTarget(Vector3 targetPosition)
        {
            transform.position = targetPosition;
            isMoving = false;

            if (doorCollider != null)
                doorCollider.enabled = !isOpen;
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
