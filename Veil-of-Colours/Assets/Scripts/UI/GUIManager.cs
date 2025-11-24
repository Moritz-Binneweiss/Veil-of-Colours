using Unity.Netcode;
using UnityEngine;

namespace VeilOfColours.UI
{
    public class GUIManager : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField]
        private GameObject gameUICanvas;

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
            ShowGameUI();
        }

        private void OnClientConnected(ulong clientId)
        {
            ShowGameUI();
        }

        private void ShowGameUI()
        {
            SetGameUIActive(true);
        }

        public void HideGameUI()
        {
            SetGameUIActive(false);
        }

        private void SetGameUIActive(bool active)
        {
            if (gameUICanvas != null)
                gameUICanvas.SetActive(active);
        }
    }
}
