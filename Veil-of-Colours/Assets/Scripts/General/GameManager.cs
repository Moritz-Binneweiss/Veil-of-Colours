using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace VeilOfColours.General
{
    public class GameManager : NetworkBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Level References")]
        [SerializeField]
        private GameObject levelA;

        [SerializeField]
        private GameObject levelB;

        [Header("Input")]
        [SerializeField]
        private InputActionReference pauseAction;

        [Header("UI References")]
        [SerializeField]
        private GameObject gameUICanvas;

        [SerializeField]
        private GameObject pauseUICanvas;

        [SerializeField]
        private GameObject gameOverUICanvas;

        [SerializeField]
        private GameObject victoryUICanvas;

        private NetworkVariable<bool> isPaused = new NetworkVariable<bool>(
            false,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void Start()
        {
            SubscribeToNetworkEvents();
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            isPaused.OnValueChanged += OnPauseStateChanged;
            UpdateUIState(isPaused.Value);
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            if (isPaused != null)
            {
                isPaused.OnValueChanged -= OnPauseStateChanged;
            }
        }

        private void Update()
        {
            // Prevent pausing during player connection
            if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsListening)
                return;

            // Check for pause input - any player can pause
            if (pauseAction != null && pauseAction.action.WasPressedThisFrame())
            {
                if (isPaused.Value)
                {
                    RequestUnpause();
                }
                else
                {
                    RequestPause();
                }
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
            if (NetworkManager.Singleton.IsServer)
            {
                EnableAllLevels();
            }
        }

        private void EnableAllLevels()
        {
            EnableLevelA();
            EnableLevelB();
        }

        public void EnableLevelA()
        {
            SetLevelActive(levelA, true);
        }

        public void EnableLevelB()
        {
            SetLevelActive(levelB, true);
        }

        public void DisableLevelA()
        {
            SetLevelActive(levelA, false);
        }

        public void DisableLevelB()
        {
            SetLevelActive(levelB, false);
        }

        private void SetLevelActive(GameObject level, bool active)
        {
            if (level != null)
                level.SetActive(active);
        }

        // ==================== PAUSE SYSTEM ====================

        public void RequestPause()
        {
            if (IsServer)
            {
                isPaused.Value = true;
            }
            else
            {
                RequestPauseServerRpc();
            }
        }

        public void RequestUnpause()
        {
            if (IsServer)
            {
                isPaused.Value = false;
            }
            else
            {
                RequestUnpauseServerRpc();
            }
        }

        [Rpc(SendTo.Server)]
        private void RequestPauseServerRpc()
        {
            isPaused.Value = true;
        }

        [Rpc(SendTo.Server)]
        private void RequestUnpauseServerRpc()
        {
            isPaused.Value = false;
        }

        private void OnPauseStateChanged(bool previousValue, bool newValue)
        {
            UpdateUIState(newValue);
        }

        private void UpdateUIState(bool paused)
        {
            // Use unscaled time to prevent blocking network updates
            Time.timeScale = paused ? 0f : 1f;

            if (gameUICanvas != null)
                gameUICanvas.SetActive(!paused);

            if (pauseUICanvas != null)
                pauseUICanvas.SetActive(paused);
        }

        // ==================== GAME OVER ====================

        public void ShowGameOver()
        {
            if (IsServer)
            {
                ShowGameOverClientRpc();
            }
            else
            {
                RequestShowGameOverServerRpc();
            }
        }

        [Rpc(SendTo.Server)]
        private void RequestShowGameOverServerRpc()
        {
            ShowGameOverClientRpc();
        }

        [Rpc(SendTo.Everyone)]
        private void ShowGameOverClientRpc()
        {
            Time.timeScale = 0f;

            if (gameUICanvas != null)
                gameUICanvas.SetActive(false);

            if (pauseUICanvas != null)
                pauseUICanvas.SetActive(false);

            if (gameOverUICanvas != null)
                gameOverUICanvas.SetActive(true);
        }

        // ==================== VICTORY ====================

        public void ShowVictory()
        {
            if (IsServer)
            {
                ShowVictoryClientRpc();
            }
            else
            {
                RequestShowVictoryServerRpc();
            }
        }

        [Rpc(SendTo.Server)]
        private void RequestShowVictoryServerRpc()
        {
            ShowVictoryClientRpc();
        }

        [Rpc(SendTo.Everyone)]
        private void ShowVictoryClientRpc()
        {
            Time.timeScale = 0f;

            if (gameUICanvas != null)
                gameUICanvas.SetActive(false);

            if (pauseUICanvas != null)
                pauseUICanvas.SetActive(false);

            if (victoryUICanvas != null)
                victoryUICanvas.SetActive(true);
        }

        // ==================== SCENE MANAGEMENT ====================

        public void RestartGame()
        {
            if (IsServer)
            {
                RestartGameClientRpc();
            }
            else
            {
                RequestRestartGameServerRpc();
            }
        }

        [Rpc(SendTo.Server)]
        private void RequestRestartGameServerRpc()
        {
            RestartGameClientRpc();
        }

        [Rpc(SendTo.Everyone)]
        private void RestartGameClientRpc()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        public void QuitToMainMenu()
        {
            if (IsServer)
            {
                QuitToMainMenuClientRpc();
            }
            else
            {
                RequestQuitToMainMenuServerRpc();
            }
        }

        [Rpc(SendTo.Server)]
        private void RequestQuitToMainMenuServerRpc()
        {
            QuitToMainMenuClientRpc();
        }

        [Rpc(SendTo.Everyone)]
        private void QuitToMainMenuClientRpc()
        {
            Time.timeScale = 1f;

            if (NetworkManager.Singleton != null)
            {
                if (NetworkManager.Singleton.IsHost)
                {
                    NetworkManager.Singleton.Shutdown();
                }
                else if (NetworkManager.Singleton.IsClient)
                {
                    NetworkManager.Singleton.Shutdown();
                }
            }

            SceneManager.LoadScene("MainMenu");
        }

        // ==================== CLEANUP ====================

        private new void OnDestroy()
        {
            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            }

            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
}
