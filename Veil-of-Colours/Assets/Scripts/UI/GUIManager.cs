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
            SubscribeToNetworkEvents();
        }

        private void OnDestroy()
        {
            UnsubscribeFromNetworkEvents();
        }

        private void SubscribeToNetworkEvents()
        {
            if (NetworkManager.Singleton == null)
                return;

            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnServerStarted += OnServerStarted;
        }

        private void UnsubscribeFromNetworkEvents()
        {
            if (NetworkManager.Singleton == null)
                return;

            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.Singleton.OnServerStarted -= OnServerStarted;
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
            SetNetworkUIActive(false);
            levelManager?.DisableMainCamera();
        }

        public void ShowNetworkUI()
        {
            SetNetworkUIActive(true);
        }

        private void SetNetworkUIActive(bool active)
        {
            if (networkUICanvas != null)
                networkUICanvas.SetActive(active);
        }
    }
}
