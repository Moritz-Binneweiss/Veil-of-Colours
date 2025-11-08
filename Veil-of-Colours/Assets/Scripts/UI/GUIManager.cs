using Unity.Netcode;
using UnityEngine;

namespace VeilOfColours.Network
{
    /// <summary>
    /// Manages all GUI elements and their visibility during gameplay
    /// </summary>
    public class GUIManager : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField]
        private GameObject networkUICanvas;

        private void Start()
        {
            // Subscribe to network events
            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
                NetworkManager.Singleton.OnServerStarted += OnServerStarted;
            }
        }

        private void OnDestroy()
        {
            // Unsubscribe from events
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
            {
                networkUICanvas.SetActive(false);
            }
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
