using UnityEngine;

namespace VeilOfColours.General
{
    /// <summary>
    /// Manages level state and camera configuration.
    /// </summary>
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
            InitializeCameras();
        }

        private void InitializeCameras()
        {
            EnableMainCamera();
            DisableLevelCameras();
        }

        private void EnableMainCamera()
        {
            if (mainCamera != null)
                mainCamera.enabled = true;
        }

        private void DisableLevelCameras()
        {
            ConfigureCamera(cameraA, false);
            ConfigureCamera(cameraB, false);
        }

        private void ConfigureCamera(Camera camera, bool enable)
        {
            if (camera == null)
                return;

            camera.rect = new Rect(0f, 0f, 1f, 1f);
            camera.enabled = enable;
        }

        public void DisableMainCamera()
        {
            if (mainCamera != null)
                mainCamera.enabled = false;
        }

        public void EnableLevelA()
        {
            SetLevelActive(levelA, true);
        }

        public void EnableLevelB()
        {
            SetLevelActive(levelB, true);
        }

        public void DisableLevelA()
        {
            SetLevelActive(levelA, false);
        }

        public void DisableLevelB()
        {
            SetLevelActive(levelB, false);
        }

        private void SetLevelActive(GameObject level, bool active)
        {
            if (level != null)
                level.SetActive(active);
        }
    }
}
