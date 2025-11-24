using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VeilOfColours.Networking;

namespace VeilOfColours.UI
{
    public class MainMenuUI : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField]
        private GameObject menuPanel;

        [SerializeField]
        private TextMeshProUGUI statusText;

        [SerializeField]
        private TMP_InputField codeInput;

        [SerializeField]
        private Button hostButton;

        [SerializeField]
        private Button joinButton;

        private RelayManager relayManager;
        private bool isInitialized = false;

        private void Start()
        {
            InitializeUI();
            InitializeServices();
        }

        private void InitializeUI()
        {
            if (hostButton != null)
                hostButton.onClick.AddListener(OnHostButtonClicked);

            if (joinButton != null)
                joinButton.onClick.AddListener(OnJoinButtonClicked);

            SetButtonsInteractable(false);
            UpdateStatus("Initializing...");
        }

        private async void InitializeServices()
        {
            relayManager = RelayManager.Instance;

            if (relayManager == null)
            {
                Debug.LogError("RelayManager not found in scene!");
                UpdateStatus("Error: RelayManager missing!");
                return;
            }

            // Subscribe to events
            relayManager.OnConnectionStatusChanged += UpdateStatus;

            // Initialize Unity Services
            bool success = await relayManager.InitializeUnityServices();

            if (success)
            {
                isInitialized = true;
                SetButtonsInteractable(true);
                UpdateStatus("Ready to play!");
            }
            else
            {
                UpdateStatus("Initialization failed. Check console.");
            }
        }

        private async void OnHostButtonClicked()
        {
            if (!isInitialized)
            {
                UpdateStatus("Services not initialized yet!");
                return;
            }

            SetButtonsInteractable(false);
            UpdateStatus("Creating game...");

            string joinCode = await relayManager.StartHostWithRelay();

            if (!string.IsNullOrEmpty(joinCode))
            {
                UpdateStatus($"Game created! Code: {joinCode}");
                HideMenuPanel();
            }
            else
            {
                UpdateStatus("Failed to create game. Try again.");
                SetButtonsInteractable(true);
            }
        }

        private async void OnJoinButtonClicked()
        {
            if (!isInitialized)
            {
                UpdateStatus("Services not initialized yet!");
                return;
            }

            if (codeInput == null || string.IsNullOrWhiteSpace(codeInput.text))
            {
                UpdateStatus("Please enter a join code!");
                return;
            }

            SetButtonsInteractable(false);
            UpdateStatus($"Joining game...");

            string joinCode = codeInput.text.Trim().ToUpper();
            bool success = await relayManager.JoinWithRelay(joinCode);

            if (!success)
            {
                UpdateStatus("Failed to join. Check code and try again.");
                SetButtonsInteractable(true);
            }
            else
            {
                HideMenuPanel();
            }
        }

        private void HideMenuPanel()
        {
            if (menuPanel != null)
            {
                menuPanel.SetActive(false);
            }
        }

        private void UpdateStatus(string message)
        {
            if (statusText != null)
                statusText.text = message;
        }

        private void SetButtonsInteractable(bool interactable)
        {
            if (hostButton != null)
                hostButton.interactable = interactable;

            if (joinButton != null)
                joinButton.interactable = interactable;
        }

        private void OnDestroy()
        {
            if (hostButton != null)
                hostButton.onClick.RemoveListener(OnHostButtonClicked);

            if (joinButton != null)
                joinButton.onClick.RemoveListener(OnJoinButtonClicked);

            if (relayManager != null)
                relayManager.OnConnectionStatusChanged -= UpdateStatus;
        }
    }
}
