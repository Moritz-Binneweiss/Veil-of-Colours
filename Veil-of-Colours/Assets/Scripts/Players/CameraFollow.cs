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
        private float smoothTime = 0.2f;

        [SerializeField]
        private Vector3 offset = new Vector3(0, 2f, -10f);

        [Header("Deadzone")]
        [SerializeField]
        private bool useDeadzone = true;

        [SerializeField]
        private float deadzoneWidth = 3f;

        [SerializeField]
        private float deadzoneHeight = 2f;

        [Header("Look Ahead")]
        [SerializeField]
        private bool useLookAhead = true;

        [SerializeField]
        private float lookAheadDistance = 2f;

        [SerializeField]
        private float lookAheadSmooth = 5f;

        private Vector3 velocity = Vector3.zero;
        private Vector3 currentLookAhead = Vector3.zero;
        private Rigidbody2D targetRigidbody;

        private void Start()
        {
            if (transform.parent != null)
            {
                Transform playerTransform = transform.parent;
                transform.SetParent(null);
                if (target == null)
                    target = playerTransform;
            }

            if (target != null)
            {
                targetRigidbody = target.GetComponent<Rigidbody2D>();
                Vector3 targetPos = target.position;
                targetPos.z = offset.z;
                transform.position = targetPos;
            }
        }

        private void LateUpdate()
        {
            if (target == null)
                return;

            Vector3 targetPosition = target.position;

            // Look ahead in movement direction
            if (useLookAhead && targetRigidbody != null)
            {
                Vector3 targetLookAhead = new Vector3(
                    Mathf.Sign(targetRigidbody.linearVelocity.x) * lookAheadDistance,
                    0f,
                    0f
                );
                currentLookAhead = Vector3.Lerp(
                    currentLookAhead,
                    targetLookAhead,
                    Time.deltaTime * lookAheadSmooth
                );
                targetPosition += currentLookAhead;
            }

            // Apply deadzone
            if (useDeadzone)
            {
                Vector3 currentPos = transform.position;
                Vector3 diff = targetPosition - currentPos;

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

            Vector3 desiredPosition = targetPosition + offset;
            desiredPosition.z = offset.z;

            // Smooth follow
            transform.position = Vector3.SmoothDamp(
                transform.position,
                desiredPosition,
                ref velocity,
                smoothTime
            );
        }

        public void SetTarget(Transform newTarget)
        {
            target = newTarget;

            if (target != null)
            {
                targetRigidbody = target.GetComponent<Rigidbody2D>();
                Vector3 targetPos = target.position;
                targetPos.z = offset.z;
                transform.position = targetPos;
            }
        }

        private void OnDrawGizmos()
        {
            if (!useDeadzone || !Application.isPlaying)
                return;

            // Draw deadzone
            Gizmos.color = Color.yellow;
            Vector3 center = transform.position;
            center.z = 0;
            Gizmos.DrawWireCube(center, new Vector3(deadzoneWidth * 2, deadzoneHeight * 2, 0));
        }
    }
}
