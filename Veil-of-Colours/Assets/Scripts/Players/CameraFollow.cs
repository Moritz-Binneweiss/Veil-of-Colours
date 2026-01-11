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
        private float deadzoneWidth = 6f; // Smaller deadzone for more responsive camera

        [SerializeField]
        private float deadzoneHeight = 5f; // Vertical deadzone threshold

        [SerializeField]
        private float recenterSpeed = 0.125f; // Lower value = smoother/slower recentering

        [SerializeField]
        private float idleThreshold = 0.1f; // Velocity threshold to consider player as idle

        [Header("Camera Shake")]
        [SerializeField]
        private bool enableShake = true;

        private Vector3 velocity = Vector3.zero;
        private Rigidbody2D targetRigidbody;
        private float shakeIntensity = 0f;
        private float shakeTimeRemaining = 0f;
        private float shakeDuration = 0f;
        private Vector3 shakeOffset = Vector3.zero;

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
            bool isPlayerMoving = false;

            // Check if player is moving
            if (targetRigidbody != null)
            {
                isPlayerMoving = targetRigidbody.linearVelocity.magnitude > idleThreshold;
            }

            // Apply deadzone - only when moving, recenter when idle
            if (useDeadzone && isPlayerMoving)
            {
                // Compare camera position (minus offset) with target position
                Vector3 currentPosWithoutOffset = transform.position - offset;
                Vector3 diff = targetPosition - currentPosWithoutOffset;

                // Horizontal deadzone
                if (Mathf.Abs(diff.x) > deadzoneWidth)
                {
                    float move = Mathf.Abs(diff.x) - deadzoneWidth;
                    targetPosition.x = currentPosWithoutOffset.x + Mathf.Sign(diff.x) * move;
                }
                else
                {
                    targetPosition.x = currentPosWithoutOffset.x;
                }

                // Vertical deadzone
                if (Mathf.Abs(diff.y) > deadzoneHeight)
                {
                    float move = Mathf.Abs(diff.y) - deadzoneHeight;
                    targetPosition.y = currentPosWithoutOffset.y + Mathf.Sign(diff.y) * move;
                }
                else
                {
                    targetPosition.y = currentPosWithoutOffset.y;
                }
            }
            // When player is idle, gradually recenter the camera
            else if (!isPlayerMoving)
            {
                // No deadzone when idle - smoothly move towards centered position
                targetPosition = target.position;
            }

            Vector3 desiredPosition = targetPosition + offset;
            desiredPosition.z = offset.z;

            // Use different smooth time based on whether player is idle
            float currentSmoothTime = isPlayerMoving
                ? smoothTime
                : (smoothTime * (1f / recenterSpeed));

            // Smooth follow WITHOUT shake (shake applied after)
            Vector3 smoothPosition = Vector3.SmoothDamp(
                transform.position,
                desiredPosition,
                ref velocity,
                currentSmoothTime
            );

            // Update camera shake
            UpdateShake();

            // Apply shake AFTER smoothing for instant effect
            transform.position = smoothPosition + shakeOffset;
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

        public void TriggerShake(float intensity, float duration)
        {
            if (!enableShake)
                return;

            shakeIntensity = intensity;
            shakeTimeRemaining = duration;
            shakeDuration = duration;
        }

        private void UpdateShake()
        {
            if (shakeTimeRemaining > 0f)
            {
                shakeTimeRemaining -= Time.deltaTime;

                // Generate random shake offset
                shakeOffset = new Vector3(
                    Random.Range(-1f, 1f) * shakeIntensity,
                    Random.Range(-1f, 1f) * shakeIntensity,
                    0f
                );

                // Reduce intensity over time for smooth fade out
                if (shakeDuration > 0f)
                {
                    float fadeOut = shakeTimeRemaining / shakeDuration;
                    shakeOffset *= fadeOut;
                }
            }
            else
            {
                shakeOffset = Vector3.zero;
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
