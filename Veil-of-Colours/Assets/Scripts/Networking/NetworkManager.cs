using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace VeilOfColours.Networking
{
    public class VeilNetworkManager : MonoBehaviour
    {
        [Header("Player Prefabs")]
        [SerializeField]
        private GameObject playerOnePrefab;

        [SerializeField]
        private GameObject playerTwoPrefab;

        [Header("Scene Names")]
        [SerializeField]
        private string mainMenuScene = "MainMenu";

        [SerializeField]
        private string gameScene = "Game";

        public static VeilNetworkManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            SubscribeToNetworkEvents();
        }

        private void OnDestroy()
        {
            UnsubscribeFromNetworkEvents();

            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void SubscribeToNetworkEvents()
        {
            if (NetworkManager.Singleton == null)
                return;

            NetworkManager.Singleton.OnServerStarted += OnServerStarted;
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        }

        private void UnsubscribeFromNetworkEvents()
        {
            if (NetworkManager.Singleton == null)
                return;

            NetworkManager.Singleton.OnServerStarted -= OnServerStarted;
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;

            // Unsubscribe scene events if available
            if (NetworkManager.Singleton.SceneManager != null)
            {
                NetworkManager.Singleton.SceneManager.OnLoadEventCompleted -= OnSceneLoadCompleted;
            }
        }

        private void OnServerStarted()
        {
            if (NetworkManager.Singleton.SceneManager != null)
            {
                NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += OnSceneLoadCompleted;
            }

            if (NetworkManager.Singleton.IsHost)
            {
                LoadGameSceneNetwork();
            }
        }

        private void OnClientConnected(ulong clientId)
        {
            // Spawn second player when they join
            if (
                NetworkManager.Singleton.IsServer
                && NetworkManager.Singleton.ConnectedClients.Count == 2
            )
            {
                Invoke(nameof(SpawnPlayersDelayed), 0.5f);
            }
        }

        private void OnSceneLoadCompleted(
            string sceneName,
            LoadSceneMode loadSceneMode,
            System.Collections.Generic.List<ulong> clientsCompleted,
            System.Collections.Generic.List<ulong> clientsTimedOut
        )
        {
            // Spawn solo host player if no one else has joined yet
            if (sceneName == gameScene && NetworkManager.Singleton.IsServer)
            {
                if (NetworkManager.Singleton.ConnectedClients.Count == 1)
                {
                    Invoke(nameof(SpawnHostPlayerOnly), 0.5f);
                }
            }
        }

        private void SpawnHostPlayerOnly()
        {
            if (!NetworkManager.Singleton.IsServer)
                return;

            ulong hostClientId = NetworkManager.Singleton.LocalClientId;
            SpawnPlayerAtLocation("Spawn_A", playerOnePrefab, hostClientId);
        }

        private void SpawnPlayersDelayed()
        {
            SpawnPlayerTwoServerRpc();
        }

        private void OnClientDisconnected(ulong clientId)
        {
            // Handle disconnections if needed
        }

        private void LoadGameSceneNetwork()
        {
            if (string.IsNullOrEmpty(gameScene))
            {
                Debug.LogError("Game scene name is empty!");
                return;
            }

            if (!NetworkManager.Singleton.IsServer)
            {
                Debug.LogWarning("Only server can load scenes!");
                return;
            }

            var status = NetworkManager.Singleton.SceneManager.LoadScene(
                gameScene,
                LoadSceneMode.Single
            );

            if (status != SceneEventProgressStatus.Started)
            {
                Debug.LogError($"Failed to load scene: {status}");
            }
        }

        [Rpc(SendTo.Server)]
        private void SpawnPlayerTwoServerRpc()
        {
            if (!NetworkManager.Singleton.IsServer)
                return;

            ulong hostClientId = NetworkManager.Singleton.LocalClientId;
            ulong clientClientId = 0;

            foreach (var kvp in NetworkManager.Singleton.ConnectedClients)
            {
                if (kvp.Key != hostClientId)
                {
                    clientClientId = kvp.Key;
                    break;
                }
            }

            SpawnPlayerAtLocation("Spawn_B", playerTwoPrefab, clientClientId);
        }

        private void SpawnPlayerAtLocation(string spawnTag, GameObject playerPrefab, ulong clientId)
        {
            if (playerPrefab == null)
            {
                Debug.LogError($"Player prefab is null for spawn tag {spawnTag}");
                return;
            }

            GameObject spawnPoint = GameObject.FindGameObjectWithTag(spawnTag);
            if (spawnPoint == null)
            {
                Debug.LogError($"Spawn point with tag {spawnTag} not found");
                return;
            }

            Vector3 spawnPosition = spawnPoint.transform.position;
            GameObject playerInstance = Instantiate(
                playerPrefab,
                spawnPosition,
                Quaternion.identity
            );

            NetworkObject networkObject = playerInstance.GetComponent<NetworkObject>();
            if (networkObject != null)
            {
                networkObject.SpawnAsPlayerObject(clientId);
            }
            else
            {
                Debug.LogError("NetworkObject missing on player prefab");
                Destroy(playerInstance);
            }
        }

        public void DisconnectAndReturnToMenu()
        {
            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.Shutdown();
            }

            SceneManager.LoadScene(mainMenuScene);
        }
    }
}
