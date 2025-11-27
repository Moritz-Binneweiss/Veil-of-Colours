using Unity.Netcode;
using UnityEngine;

namespace VeilOfColours.Players
{
    public class PlayerManager : NetworkBehaviour
    {
        [Header("Player Settings")]
        [SerializeField]
        private string playerName = "Player";

        [SerializeField]
        private bool isPlayerOne = true;

        [Header("Level References")]
        [SerializeField]
        private GameObject targetLevel;

        [Header("Camera Reference")]
        [SerializeField]
        private GameObject playerCameraObject;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            if (IsOwner)
            {
                SetupPlayerLevel();
                SetupCamera();
            }
            else
            {
                DisableNonOwnerCamera();
            }
        }

        private void SetupPlayerLevel()
        {
            // Additional player setup can be done here
        }

        private void SetupCamera()
        {
            if (playerCameraObject != null)
            {
                // Set this player as the camera target using reflection to avoid compile errors
                var cameraFollow = playerCameraObject.GetComponent("CameraFollow");
                if (cameraFollow != null)
                {
                    var setTargetMethod = cameraFollow.GetType().GetMethod("SetTarget");
                    if (setTargetMethod != null)
                    {
                        setTargetMethod.Invoke(cameraFollow, new object[] { transform });
                    }
                }

                var camera = playerCameraObject.GetComponent<Camera>();
                if (camera != null)
                {
                    camera.enabled = true;
                }
            }
        }

        private void DisableNonOwnerCamera()
        {
            if (playerCameraObject != null)
            {
                var camera = playerCameraObject.GetComponent<Camera>();
                if (camera != null)
                {
                    camera.enabled = false;
                }
            }
        }

        public string GetPlayerName()
        {
            return playerName;
        }

        public bool IsPlayerOne()
        {
            return isPlayerOne;
        }
    }
}
