using UnityEngine;

namespace VeilOfColours.Players
{
    public class CameraFollow : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField]
        private Transform target;

        [Header("Follow Settings")]
        [SerializeField]
        private float smoothSpeed = 10f; // How quickly camera follows player

        [SerializeField]
        private Vector3 offset = new Vector3(0, 2f, -10f); // Camera offset from player

        [Header("Deadzone")]
        [SerializeField]
        private bool useDeadzone = true;

        [SerializeField]
        private float deadzoneWidth = 2f; // Horizontal deadzone

        [SerializeField]
        private float deadzoneHeight = 1f; // Vertical deadzone

        [Header("Look Ahead")]
        [SerializeField]
        private bool useLookAhead = true;

        [SerializeField]
        private float lookAheadDistance = 3f; // How far ahead to look

        [SerializeField]
        private float lookAheadSpeed = 2f; // How quickly look-ahead adjusts

        [Header("Bounds (Optional)")]
        [SerializeField]
        private bool useBounds = false;

        [SerializeField]
        private Vector2 minBounds = new Vector2(-100, -100);

        [SerializeField]
        private Vector2 maxBounds = new Vector2(100, 100);

        private Vector3 currentVelocity;
        private Vector3 desiredPosition;
        private float currentLookAhead;
        private Rigidbody2D targetRigidbody;

        private void Start()
        {
            if (target != null)
            {
                targetRigidbody = target.GetComponent<Rigidbody2D>();
                // Snap camera to target initially
                transform.position = target.position + offset;
            }
        }

        private void LateUpdate()
        {
            if (target == null)
                return;

            UpdateCameraPosition();
        }

        private void UpdateCameraPosition()
        {
            Vector3 targetPosition = target.position;

            // Apply look-ahead based on player velocity
            if (useLookAhead && targetRigidbody != null)
            {
                float targetLookAhead = 0f;

                // Look ahead in direction of movement
                if (Mathf.Abs(targetRigidbody.linearVelocity.x) > 0.5f)
                {
                    targetLookAhead =
                        Mathf.Sign(targetRigidbody.linearVelocity.x) * lookAheadDistance;
                }

                // Smooth look-ahead transition
                currentLookAhead = Mathf.Lerp(
                    currentLookAhead,
                    targetLookAhead,
                    lookAheadSpeed * Time.deltaTime
                );

                targetPosition.x += currentLookAhead;
            }

            // Apply deadzone
            if (useDeadzone)
            {
                Vector3 currentPos = transform.position;
                Vector3 diff = targetPosition - currentPos;

                // Only move camera if target exits deadzone
                if (Mathf.Abs(diff.x) > deadzoneWidth)
                {
                    float move = Mathf.Abs(diff.x) - deadzoneWidth;
                    targetPosition.x = currentPos.x + Mathf.Sign(diff.x) * move;
                }
                else
                {
                    targetPosition.x = currentPos.x;
                }

                if (Mathf.Abs(diff.y) > deadzoneHeight)
                {
                    float move = Mathf.Abs(diff.y) - deadzoneHeight;
                    targetPosition.y = currentPos.y + Mathf.Sign(diff.y) * move;
                }
                else
                {
                    targetPosition.y = currentPos.y;
                }
            }

            // Add offset
            desiredPosition = targetPosition + offset;

            // Apply bounds
            if (useBounds)
            {
                desiredPosition.x = Mathf.Clamp(desiredPosition.x, minBounds.x, maxBounds.x);
                desiredPosition.y = Mathf.Clamp(desiredPosition.y, minBounds.y, maxBounds.y);
            }

            // Smooth follow
            transform.position = Vector3.Lerp(
                transform.position,
                desiredPosition,
                smoothSpeed * Time.deltaTime
            );
        }

        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
            if (target != null)
            {
                targetRigidbody = target.GetComponent<Rigidbody2D>();
                transform.position = target.position + offset;
            }
        }

        public void SnapToTarget()
        {
            if (target != null)
            {
                transform.position = target.position + offset;
            }
        }

        // Visualize deadzone in editor
        private void OnDrawGizmosSelected()
        {
            if (!useDeadzone)
                return;

            Gizmos.color = Color.yellow;
            Vector3 center = transform.position;
            center.z = 0;

            // Draw deadzone rectangle
            Vector3 size = new Vector3(deadzoneWidth * 2, deadzoneHeight * 2, 0);
            Gizmos.DrawWireCube(center, size);
        }
    }
}
