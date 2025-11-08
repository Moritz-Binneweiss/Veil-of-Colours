using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace VeilOfColours.Network
{
    public class NetworkUI : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField]
        private Button hostButton;

        [SerializeField]
        private Button clientButton;

        [SerializeField]
        private TextMeshProUGUI statusText;

        private void Start()
        {
            hostButton.onClick.AddListener(OnHostClicked);
            clientButton.onClick.AddListener(OnClientClicked);
            UpdateStatusText("Ready to connect...");
        }

        private void OnHostClicked()
        {
            UpdateStatusText("Starting as Host...");
            NetworkManager.Singleton.StartHost();
        }

        private void OnClientClicked()
        {
            UpdateStatusText("Connecting as Client...");
            NetworkManager.Singleton.StartClient();
        }

        private void UpdateStatusText(string message)
        {
            if (statusText != null)
                statusText.text = message;
        }

        private void OnDestroy()
        {
            if (hostButton != null)
                hostButton.onClick.RemoveListener(OnHostClicked);
            if (clientButton != null)
                clientButton.onClick.RemoveListener(OnClientClicked);
        }
    }
}
