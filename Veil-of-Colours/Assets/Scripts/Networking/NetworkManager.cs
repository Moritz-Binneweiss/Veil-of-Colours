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
        }

        private void OnServerStarted()
        {
            Debug.Log("Server started");

            if (NetworkManager.Singleton.IsHost)
            {
                LoadGameScene();
            }
        }

        private void OnClientConnected(ulong clientId)
        {
            Debug.Log($"Client {clientId} connected");

            if (NetworkManager.Singleton.IsServer)
            {
                // Both players are now connected, spawn players
                if (NetworkManager.Singleton.ConnectedClients.Count == 2)
                {
                    SpawnPlayersServerRpc();
                }
            }

            if (!NetworkManager.Singleton.IsHost && NetworkManager.Singleton.IsClient)
            {
                // Client successfully connected, load game scene
                LoadGameScene();
            }
        }

        private void OnClientDisconnected(ulong clientId)
        {
            Debug.Log($"Client {clientId} disconnected");
        }

        private void LoadGameScene()
        {
            if (!string.IsNullOrEmpty(gameScene))
            {
                SceneManager.LoadScene(gameScene);
            }
        }

        [Rpc(SendTo.Server)]
        private void SpawnPlayersServerRpc()
        {
            if (!NetworkManager.Singleton.IsServer)
                return;

            // Get connected client IDs
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

            // Spawn PlayerOne for host at Spawn_A
            SpawnPlayerAtLocation("Spawn_A", playerOnePrefab, hostClientId);

            // Spawn PlayerTwo for client at Spawn_B
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
            GameObject playerInstance = Instantiate(playerPrefab, spawnPosition, Quaternion.identity);

            NetworkObject networkObject = playerInstance.GetComponent<NetworkObject>();
            if (networkObject != null)
            {
                networkObject.SpawnAsPlayerObject(clientId);
                Debug.Log($"Spawned player for client {clientId} at {spawnTag}");
            }
            else
            {
                Debug.LogError($"NetworkObject component not found on player prefab");
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
