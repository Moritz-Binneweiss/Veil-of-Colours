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
        private GameObject panel;

        [SerializeField]
        private TextMeshProUGUI climbStaminaText;

        private RelayManager relayManager;
        private VeilOfColours.Players.PlayerMovement localPlayer;

        private void Start()
        {
            InitializeUI();
            SubscribeToNetworkEvents();
        }

        private void Update()
        {
            UpdateStaminaDisplay();
        }

        private void UpdateStaminaDisplay()
        {
            if (climbStaminaText == null)
                return;

            // Find local player if not found yet
            if (localPlayer == null)
            {
                var players = FindObjectsByType<VeilOfColours.Players.PlayerMovement>(
                    FindObjectsSortMode.None
                );
                foreach (var player in players)
                {
                    if (player.IsOwner)
                    {
                        localPlayer = player;
                        break;
                    }
                }
            }

            // Update climb stamina text
            if (localPlayer != null)
            {
                float stamina = localPlayer.GetCurrentClimbStamina();
                climbStaminaText.text = $"Climb Stamina: {stamina:F0}";
            }
            else
            {
                climbStaminaText.text = "Climb Stamina: --";
            }
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
                    UpdateCodeText("");
                }
                else if (!string.IsNullOrEmpty(currentCode))
                {
                    UpdateCodeText($"Join Code: {currentCode}");
                }
            }
        }

        private void DisplayJoinCode(string joinCode)
        {
            if (string.IsNullOrEmpty(joinCode))
                return;

            ShowCodeDisplay();
            UpdateCodeText($"Join Code: {joinCode}");
        }

        private void UpdateCodeText(string text)
        {
            if (codeDisplayText != null)
                codeDisplayText.text = text;
        }

        private void ShowCodeDisplay()
        {
            if (panel != null)
                panel.SetActive(true);
        }

        private void HideCodeDisplay()
        {
            if (panel != null)
                panel.SetActive(false);
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
