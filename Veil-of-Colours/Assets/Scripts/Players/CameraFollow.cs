using UnityEngine;

namespace VeilOfColours.Players
{
    public class CameraFollow : MonoBehaviour
    {
        [SerializeField]
        private Transform target;

        [SerializeField]
        private Vector3 offset = new Vector3(0, 0, -10f);

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
                Vector3 targetPos = target.position;
                targetPos.z = offset.z;
                transform.position = targetPos;
            }
        }

        private void LateUpdate()
        {
            if (target != null)
            {
                Vector3 targetPos = target.position;
                targetPos.z = offset.z;
                transform.position = targetPos;
            }
        }

        public void SetTarget(Transform newTarget)
        {
            target = newTarget;

            if (target != null)
            {
                Vector3 targetPos = target.position;
                targetPos.z = offset.z;
                transform.position = targetPos;
            }
        }
    }
}
