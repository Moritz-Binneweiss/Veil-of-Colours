using Unity.Netcode;
using UnityEngine;

namespace VeilOfColours.Players
{
    /// <summary>
    /// Alternative camera setup with Cinemachine support.
    /// Can be used instead of the camera logic in SimplePlayer2D.cs
    /// Requires Cinemachine package and assembly reference.
    /// </summary>
    public class PlayerCameraSetup : NetworkBehaviour
    {
        [Header("Camera Settings")]
        [SerializeField]
        private string levelACameraTag = "CameraA";

        [SerializeField]
        private string levelBCameraTag = "CameraB";

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            if (!IsOwner)
                return;

            AssignCamera();
        }

        private void AssignCamera()
        {
            string cameraTag = IsServer ? levelACameraTag : levelBCameraTag;
            GameObject cameraObject = GameObject.FindGameObjectWithTag(cameraTag);

            if (cameraObject != null)
            {
                Camera cam = cameraObject.GetComponent<Camera>();
                if (cam != null)
                {
                    cam.enabled = true;
                    cam.rect = new Rect(0f, 0f, 1f, 1f);
                }

                // For Cinemachine support, uncomment and add assembly reference:
                /*
                var cinemachineCamera = cameraObject.GetComponent<Unity.Cinemachine.CinemachineCamera>();
                if (cinemachineCamera != null)
                {
                    cinemachineCamera.Follow = transform;
                    cinemachineCamera.LookAt = transform;
                    cinemachineCamera.Priority.Value = 10;
                }
                */
            }
        }
    }
}
