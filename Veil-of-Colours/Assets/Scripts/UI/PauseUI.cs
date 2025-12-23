using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using VeilOfColours.General;
using VeilOfColours.Networking;

namespace VeilOfColours.UI
{
    public class PauseUI : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField]
        private TextMeshProUGUI codeDisplayText;

        [SerializeField]
        private GameObject codeDisplayPanel;

        [SerializeField]
        private Button continueButton;

        [SerializeField]
        private Button quitButton;

        private RelayManager relayManager;

        private void Awake()
        {
            if (continueButton != null)
                continueButton.onClick.AddListener(OnContinueClicked);

            if (quitButton != null)
                quitButton.onClick.AddListener(OnQuitClicked);
        }

        private void Start()
        {
            InitializeCodeDisplay();
        }

        private void OnEnable()
        {
            UpdateCodeDisplay();
        }

        private void InitializeCodeDisplay()
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
                    UpdateCodeText("Both Players Connected");
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
            if (codeDisplayPanel != null)
                codeDisplayPanel.SetActive(true);
        }

        private void HideCodeDisplay()
        {
            if (codeDisplayPanel != null)
                codeDisplayPanel.SetActive(false);
        }

        private void OnContinueClicked()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.RequestUnpause();
            }
        }

        private void OnQuitClicked()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.QuitToMainMenu();
            }
        }

        private void OnDestroy()
        {
            if (relayManager != null)
            {
                relayManager.OnJoinCodeGenerated -= DisplayJoinCode;
            }

            if (continueButton != null)
                continueButton.onClick.RemoveListener(OnContinueClicked);

            if (quitButton != null)
                quitButton.onClick.RemoveListener(OnQuitClicked);
        }
    }
}
