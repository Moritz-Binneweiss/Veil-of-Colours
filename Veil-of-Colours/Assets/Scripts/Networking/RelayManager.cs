using System;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

namespace VeilOfColours.Networking
{
    public class RelayManager : MonoBehaviour
    {
        private const int MaxPlayers = 2;

        public static RelayManager Instance { get; private set; }

        public event Action<string> OnJoinCodeGenerated;
        public event Action<string> OnConnectionStatusChanged;
        public event Action<bool> OnHostStarted;
        public event Action<bool> OnClientJoined;

        private string currentJoinCode;

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

        public async Task<bool> InitializeUnityServices()
        {
            try
            {
                if (UnityServices.State == ServicesInitializationState.Initialized)
                {
                    return true;
                }

                UpdateStatus("Initializing Unity Services...");
                await UnityServices.InitializeAsync();

                if (!AuthenticationService.Instance.IsSignedIn)
                {
                    UpdateStatus("Signing in anonymously...");
                    await AuthenticationService.Instance.SignInAnonymouslyAsync();
                }

                UpdateStatus("Unity Services initialized successfully");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to initialize Unity Services: {e.Message}");
                UpdateStatus($"Initialization failed: {e.Message}");
                return false;
            }
        }

        public async Task<string> StartHostWithRelay()
        {
            try
            {
                UpdateStatus("Creating Relay allocation...");

                // Create Relay allocation
                Allocation allocation = await RelayService.Instance.CreateAllocationAsync(
                    MaxPlayers
                );

                // Get join code
                currentJoinCode = await RelayService.Instance.GetJoinCodeAsync(
                    allocation.AllocationId
                );

                UpdateStatus($"Host started with Join Code: {currentJoinCode}");

                // Configure Unity Transport
                var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
                transport.SetHostRelayData(
                    allocation.RelayServer.IpV4,
                    (ushort)allocation.RelayServer.Port,
                    allocation.AllocationIdBytes,
                    allocation.Key,
                    allocation.ConnectionData
                );

                // Start as host
                bool started = NetworkManager.Singleton.StartHost();

                OnJoinCodeGenerated?.Invoke(currentJoinCode);
                OnHostStarted?.Invoke(started);

                if (started)
                {
                    UpdateStatus("Host started successfully");
                    return currentJoinCode;
                }
                else
                {
                    UpdateStatus("Failed to start host");
                    return null;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to start host with Relay: {e.Message}");
                UpdateStatus($"Host start failed: {e.Message}");
                OnHostStarted?.Invoke(false);
                return null;
            }
        }

        public async Task<bool> JoinWithRelay(string joinCode)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(joinCode))
                {
                    UpdateStatus("Join code is empty!");
                    return false;
                }

                UpdateStatus($"Joining with code: {joinCode}...");

                // Join allocation
                JoinAllocation allocation = await RelayService.Instance.JoinAllocationAsync(
                    joinCode
                );

                // Configure Unity Transport
                var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
                transport.SetClientRelayData(
                    allocation.RelayServer.IpV4,
                    (ushort)allocation.RelayServer.Port,
                    allocation.AllocationIdBytes,
                    allocation.Key,
                    allocation.ConnectionData,
                    allocation.HostConnectionData
                );

                // Start as client
                bool joined = NetworkManager.Singleton.StartClient();

                OnClientJoined?.Invoke(joined);

                if (joined)
                {
                    UpdateStatus("Successfully joined game");
                }
                else
                {
                    UpdateStatus("Failed to join game");
                }

                return joined;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to join with Relay: {e.Message}");
                UpdateStatus($"Join failed: {e.Message}");
                OnClientJoined?.Invoke(false);
                return false;
            }
        }

        public string GetCurrentJoinCode()
        {
            return currentJoinCode;
        }

        private void UpdateStatus(string message)
        {
            OnConnectionStatusChanged?.Invoke(message);
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
}
