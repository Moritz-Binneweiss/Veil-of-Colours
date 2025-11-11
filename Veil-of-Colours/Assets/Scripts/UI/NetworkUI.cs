using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.UI;

namespace VeilOfColours.Network
{
    public class NetworkUI : MonoBehaviour
    {
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
            hostButton.onClick.AddListener(OnHostClicked);
            clientButton.onClick.AddListener(OnClientClicked);

            DisplayLocalIP();

            // Setze Standard-IP für lokales Testen
            if (ipAddressInput != null)
            {
                ipAddressInput.text = "127.0.0.1"; //192.168.1.XXX
            }

            UpdateStatusText("Ready to connect...");
        }

        private void DisplayLocalIP()
        {
            string localIP = GetLocalIPAddress();
            if (localIPText != null)
            {
                localIPText.text = $"Your IP: {localIP}";
            }
            Debug.Log($"Local IP Address: {localIP}");
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
            UpdateStatusText("Starting as Host...");

            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            if (transport != null)
            {
                transport.ConnectionData.Address = "0.0.0.0";
                transport.ConnectionData.Port = 7777;
                Debug.Log($"Host starting on port 7777");
            }

            NetworkManager.Singleton.StartHost();
        }

        private void OnClientClicked()
        {
            if (ipAddressInput == null || string.IsNullOrEmpty(ipAddressInput.text))
            {
                UpdateStatusText("Please enter Host IP address!");
                return;
            }

            UpdateStatusText($"Connecting to {ipAddressInput.text}...");

            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            if (transport != null)
            {
                transport.ConnectionData.Address = ipAddressInput.text;
                transport.ConnectionData.Port = 7777;
                Debug.Log($"Client connecting to {ipAddressInput.text}:7777");
            }

            NetworkManager.Singleton.StartClient();
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
