using Unity.Netcode;
using UnityEngine;

namespace VeilOfColours.Networking
{
    /// <summary>
    /// Optional: Debug UI overlay to show network status
    /// Attach this to a GameObject in the Game scene to see network info
    /// </summary>
    public class NetworkDebugUI : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField]
        private bool showDebugInfo = true;

        [SerializeField]
        private KeyCode toggleKey = KeyCode.F1;

        private GUIStyle labelStyle;
        private bool initialized = false;

        private void Update()
        {
            if (Input.GetKeyDown(toggleKey))
            {
                showDebugInfo = !showDebugInfo;
            }
        }

        private void OnGUI()
        {
            if (!showDebugInfo || NetworkManager.Singleton == null)
                return;

            InitializeStyles();
            DrawDebugPanel();
        }

        private void InitializeStyles()
        {
            if (initialized)
                return;

            labelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 16,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.white }
            };

            initialized = true;
        }

        private void DrawDebugPanel()
        {
            GUILayout.BeginArea(new Rect(10, 10, 300, 300));
            GUILayout.BeginVertical("box");

            GUILayout.Label("=== Network Debug ===", labelStyle);
            GUILayout.Space(10);

            DrawNetworkStatus();
            GUILayout.Space(5);
            DrawConnectionInfo();
            GUILayout.Space(5);
            DrawRelayInfo();
            GUILayout.Space(10);

            GUILayout.Label($"Press {toggleKey} to toggle", GUI.skin.label);

            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        private void DrawNetworkStatus()
        {
            GUILayout.Label($"Is Host: {NetworkManager.Singleton.IsHost}");
            GUILayout.Label($"Is Client: {NetworkManager.Singleton.IsClient}");
            GUILayout.Label($"Is Server: {NetworkManager.Singleton.IsServer}");
        }

        private void DrawConnectionInfo()
        {
            int connectedClients = NetworkManager.Singleton.ConnectedClients.Count;
            GUILayout.Label($"Connected Clients: {connectedClients}");

            if (NetworkManager.Singleton.IsServer)
            {
                GUILayout.Label($"Client IDs:");
                foreach (var clientId in NetworkManager.Singleton.ConnectedClientsIds)
                {
                    GUILayout.Label($"  - Client {clientId}");
                }
            }
        }

        private void DrawRelayInfo()
        {
            if (RelayManager.Instance != null && NetworkManager.Singleton.IsHost)
            {
                string joinCode = RelayManager.Instance.GetCurrentJoinCode();
                if (!string.IsNullOrEmpty(joinCode))
                {
                    GUILayout.Label($"Join Code: {joinCode}");
                }
            }
        }
    }
}
