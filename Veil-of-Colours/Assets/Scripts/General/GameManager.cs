using Unity.Netcode;
using UnityEngine;

namespace VeilOfColours.General
{
    public class GameManager : MonoBehaviour
    {
        [Header("Level References")]
        [SerializeField]
        private GameObject levelA;

        [SerializeField]
        private GameObject levelB;

        public static GameManager Instance { get; private set; }

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
            // Levels are active by default in scene
            // They will be visible immediately for testing
            SubscribeToNetworkEvents();
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
            // Make sure levels are enabled when players connect
            if (NetworkManager.Singleton.IsServer)
            {
                Debug.Log($"GameManager: Client {clientId} connected, ensuring levels are active");
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

        private void OnDestroy()
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
