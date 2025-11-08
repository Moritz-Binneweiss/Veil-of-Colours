using UnityEngine;

namespace VeilOfColours.General
{
    public class LevelManager : MonoBehaviour
    {
        [Header("Level References")]
        [SerializeField]
        private GameObject levelA;

        [SerializeField]
        private GameObject levelB;

        [Header("Camera References")]
        [SerializeField]
        private Camera mainCamera;

        [SerializeField]
        private Camera cameraA;

        [SerializeField]
        private Camera cameraB;

        private void Start()
        {
            SetupCameras();
        }

        private void SetupCameras()
        {
            // Main Camera is active for menu
            if (mainCamera != null)
            {
                mainCamera.enabled = true;
            }

            // Level cameras are disabled initially
            if (cameraA != null)
            {
                cameraA.rect = new Rect(0f, 0f, 1f, 1f);
                cameraA.enabled = false;
            }

            if (cameraB != null)
            {
                cameraB.rect = new Rect(0f, 0f, 1f, 1f);
                cameraB.enabled = false;
            }
        }

        public void DisableMainCamera()
        {
            if (mainCamera != null)
            {
                mainCamera.enabled = false;
            }
        }

        public void EnableLevelA()
        {
            if (levelA != null)
                levelA.SetActive(true);
        }

        public void EnableLevelB()
        {
            if (levelB != null)
                levelB.SetActive(true);
        }
    }
}
