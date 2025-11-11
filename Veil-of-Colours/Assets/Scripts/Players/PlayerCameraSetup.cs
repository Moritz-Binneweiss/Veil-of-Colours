using Unity.Netcode;
using UnityEngine;

namespace VeilOfColours.Players
{
    /// <summary>
    /// Simple camera setup without Cinemachine.
    /// Camera follows the player with smooth lerp.
    /// </summary>
    public class PlayerCameraSetup : NetworkBehaviour
    {
        [Header("Camera Settings")]
        [SerializeField]
        private string levelACameraTag = "CameraA";

        [SerializeField]
        private string levelBCameraTag = "CameraB";

        [Header("Camera Follow")]
        [SerializeField]
        private Vector3 cameraOffset = new Vector3(0, 2, -10);

        [SerializeField]
        private float cameraSmoothing = 5f;

        private Camera assignedCamera;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            if (!IsOwner)
                return;

            AssignCamera();
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

        private void AssignCamera()
        {
            string cameraTag = IsServer ? levelACameraTag : levelBCameraTag;
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
    }
}
