using TMPro;
using Unity.Netcode;
using UnityEngine;
using VeilOfColours.Networking;

namespace VeilOfColours.UI
{
    public class GameUI : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField]
        private TextMeshProUGUI codeDisplayText;

        [SerializeField]
        private GameObject codeDisplayPanel;

        private RelayManager relayManager;

        private void Start()
        {
            InitializeUI();
            SubscribeToNetworkEvents();
        }

        private void InitializeUI()
        {
            relayManager = RelayManager.Instance;

            if (relayManager != null)
            {
                relayManager.OnJoinCodeGenerated += DisplayJoinCode;
            }

            // Check if we're host and display code if available
            if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsHost)
            {
                string currentCode = relayManager?.GetCurrentJoinCode();
                if (!string.IsNullOrEmpty(currentCode))
                {
                    DisplayJoinCode(currentCode);
                }
            }
            else
            {
                // Client doesn't need to see the join code
                HideCodeDisplay();
            }
        }

        private void SubscribeToNetworkEvents()
        {
            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            }
        }

        private void OnClientConnected(ulong clientId)
        {
            // Update UI when clients connect
            UpdateCodeDisplay();
        }

        private void UpdateCodeDisplay()
        {
            if (NetworkManager.Singleton == null)
                return;

            if (NetworkManager.Singleton.IsHost)
            {
                int connectedPlayers = NetworkManager.Singleton.ConnectedClients.Count;
                string currentCode = relayManager?.GetCurrentJoinCode();

                if (connectedPlayers >= 2)
                {
                    UpdateCodeText("Both players connected!");
                }
                else if (!string.IsNullOrEmpty(currentCode))
                {
                    UpdateCodeText($"Join Code: {currentCode}\nWaiting for player 2...");
                }
            }
        }

        private void DisplayJoinCode(string joinCode)
        {
            if (string.IsNullOrEmpty(joinCode))
                return;

            ShowCodeDisplay();
            UpdateCodeText($"Join Code: {joinCode}\nShare this with Player 2!");
        }

        private void UpdateCodeText(string text)
        {
            if (codeDisplayText != null)
                codeDisplayText.text = text;
        }

        private void ShowCodeDisplay()
        {
            if (codeDisplayPanel != null)
                codeDisplayPanel.SetActive(true);
        }

        private void HideCodeDisplay()
        {
            if (codeDisplayPanel != null)
                codeDisplayPanel.SetActive(false);
        }

        private void OnDestroy()
        {
            if (relayManager != null)
            {
                relayManager.OnJoinCodeGenerated -= DisplayJoinCode;
            }

            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            }
        }
    }
}
