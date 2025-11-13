using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.UI;

namespace VeilOfColours.Network
{
    public class NetworkUI : MonoBehaviour
    {
        private const string DefaultIP = "127.0.0.1";
        private const ushort DefaultPort = 7777;
        private const string ListenAddress = "0.0.0.0";

        [Header("UI Elements")]
        [SerializeField]
        private Button hostButton;

        [SerializeField]
        private Button clientButton;

        [SerializeField]
        private TMP_InputField ipAddressInput;

        [SerializeField]
        private TextMeshProUGUI statusText;

        [SerializeField]
        private TextMeshProUGUI localIPText;

        private void Start()
        {
            InitializeUI();
        }

        private void InitializeUI()
        {
            if (hostButton != null)
                hostButton.onClick.AddListener(OnHostClicked);

            if (clientButton != null)
                clientButton.onClick.AddListener(OnClientClicked);

            DisplayLocalIP();

            if (ipAddressInput != null)
                ipAddressInput.text = DefaultIP;

            UpdateStatusText("Ready to connect...");
        }

        private void DisplayLocalIP()
        {
            if (localIPText != null)
                localIPText.text = $"Your IP: {GetLocalIPAddress()}";
        }

        private string GetLocalIPAddress()
        {
            try
            {
                var host = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName());
                foreach (var ip in host.AddressList)
                {
                    if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    {
                        return ip.ToString();
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error getting local IP: {e.Message}");
            }
            return "Unable to get IP";
        }

        private void OnHostClicked()
        {
            if (!ValidateNetworkManager())
                return;

            UpdateStatusText("Starting as Host...");
            ConfigureTransport(ListenAddress);
            NetworkManager.Singleton.StartHost();
        }

        private void OnClientClicked()
        {
            if (!ValidateNetworkManager())
                return;

            if (string.IsNullOrWhiteSpace(ipAddressInput?.text))
            {
                UpdateStatusText("Please enter Host IP address!");
                return;
            }

            UpdateStatusText($"Connecting to {ipAddressInput.text}...");
            ConfigureTransport(ipAddressInput.text);
            NetworkManager.Singleton.StartClient();
        }

        private bool ValidateNetworkManager()
        {
            if (NetworkManager.Singleton != null)
                return true;

            UpdateStatusText("Network Manager not found!");
            return false;
        }

        private void ConfigureTransport(string address)
        {
            var transport = NetworkManager.Singleton?.GetComponent<UnityTransport>();
            if (transport == null)
                return;

            transport.ConnectionData.Address = address;
            transport.ConnectionData.Port = DefaultPort;
        }

        private void UpdateStatusText(string message)
        {
            if (statusText != null)
                statusText.text = message;
        }

        private void OnDestroy()
        {
            if (hostButton != null)
                hostButton.onClick.RemoveListener(OnHostClicked);
            if (clientButton != null)
                clientButton.onClick.RemoveListener(OnClientClicked);
        }
    }
}
