using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace VeilOfColours.Network
{
    /// <summary>
    /// Simple UI for hosting or joining a game session
    /// </summary>
    public class NetworkUI : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField]
        private Button hostButton;

        [SerializeField]
        private Button clientButton;

        [SerializeField]
        private GameObject menuPanel;

        [SerializeField]
        private TextMeshProUGUI statusText;

        private void Start()
        {
            // Setup button listeners
            hostButton.onClick.AddListener(OnHostClicked);
            clientButton.onClick.AddListener(OnClientClicked);

            UpdateStatusText("Ready to connect...");
        }

        private void OnHostClicked()
        {
            UpdateStatusText("Starting as Host...");
            NetworkManager.Singleton.StartHost();
            HideMenu();
        }

        private void OnClientClicked()
        {
            UpdateStatusText("Connecting as Client...");
            NetworkManager.Singleton.StartClient();
            HideMenu();
        }

        private void HideMenu()
        {
            if (menuPanel != null)
                menuPanel.SetActive(false);
        }

        private void UpdateStatusText(string message)
        {
            if (statusText != null)
                statusText.text = message;
            Debug.Log($"[NetworkUI] {message}");
        }

        private void OnDestroy()
        {
            // Clean up listeners
            if (hostButton != null)
                hostButton.onClick.RemoveListener(OnHostClicked);
            if (clientButton != null)
                clientButton.onClick.RemoveListener(OnClientClicked);
        }
    }
}
