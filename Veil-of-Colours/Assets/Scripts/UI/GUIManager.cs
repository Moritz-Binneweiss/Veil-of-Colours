using Unity.Netcode;
using UnityEngine;
using VeilOfColours.General;

namespace VeilOfColours.Network
{
    /// <summary>
    /// Manages GUI visibility during gameplay.
    /// </summary>
    public class GUIManager : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField]
        private GameObject networkUICanvas;

        [Header("Manager References")]
        [SerializeField]
        private LevelManager levelManager;

        private void Start()
        {
            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
                NetworkManager.Singleton.OnServerStarted += OnServerStarted;
            }
        }

        private void OnDestroy()
        {
            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
                NetworkManager.Singleton.OnServerStarted -= OnServerStarted;
            }
        }

        private void OnServerStarted()
        {
            HideNetworkUI();
        }

        private void OnClientConnected(ulong clientId)
        {
            HideNetworkUI();
        }

        private void HideNetworkUI()
        {
            if (networkUICanvas != null)
                networkUICanvas.SetActive(false);

            if (levelManager != null)
                levelManager.DisableMainCamera();
        }

        public void ShowNetworkUI()
        {
            if (networkUICanvas != null)
            {
                networkUICanvas.SetActive(true);
            }
        }
    }
}
