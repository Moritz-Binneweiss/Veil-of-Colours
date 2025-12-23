using Unity.Netcode;
using UnityEngine;

namespace VeilOfColours.UI
{
    public class GUIManager : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField]
        private GameObject gameUICanvas;

        [SerializeField]
        private GameObject pauseUICanvas;

        [SerializeField]
        private GameObject gameOverUICanvas;

        [SerializeField]
        private GameObject victoryUICanvas;

        private void Start()
        {
            InitializeUI();
            SubscribeToNetworkEvents();
        }

        private void OnDestroy()
        {
            UnsubscribeFromNetworkEvents();
        }

        private void InitializeUI()
        {
            // Start with game UI hidden, show when connected
            if (gameUICanvas != null)
                gameUICanvas.SetActive(false);

            if (pauseUICanvas != null)
                pauseUICanvas.SetActive(false);

            if (gameOverUICanvas != null)
                gameOverUICanvas.SetActive(false);

            if (victoryUICanvas != null)
                victoryUICanvas.SetActive(false);
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
            if (gameUICanvas != null)
                gameUICanvas.SetActive(true);
        }

        public void HideGameUI()
        {
            if (gameUICanvas != null)
                gameUICanvas.SetActive(false);
        }
    }
}
