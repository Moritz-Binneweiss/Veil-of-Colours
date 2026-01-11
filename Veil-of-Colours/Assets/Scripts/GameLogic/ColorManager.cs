using Unity.Netcode;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Tilemaps;

namespace VeilOfColours.GameLogic
{
    public class ColorManager : NetworkBehaviour
    {
        public static ColorManager Instance { get; private set; }

        [Header("Color Layer Groups")]
        [SerializeField]
        private ColorLayerGroup[] colorGroups;

        [Header("Background Colors")]
        [SerializeField]
        private Camera[] backgroundCameras;

        [SerializeField]
        private Color[] backgroundColors = new Color[]
        {
            new Color(0.2f, 0.2f, 0.8f),
            new Color(0.8f, 0.2f, 0.2f),
            new Color(0.2f, 0.8f, 0.2f),
            new Color(1f, 1f, 0.3f),
        };

        private NetworkVariable<int> activeColorIndex = new NetworkVariable<int>(
            0,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

        [System.Serializable]
        public class ColorLayerGroup
        {
            public string colorName;
            public Tilemap[] tilemaps;
        }

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
            if (!IsSpawned)
            {
                ApplyColorLayer(0);
            }
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            activeColorIndex.OnValueChanged += OnColorChanged;
            ApplyColorLayer(activeColorIndex.Value);
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();

            if (activeColorIndex != null)
            {
                activeColorIndex.OnValueChanged -= OnColorChanged;
            }
        }

        private void OnColorChanged(int previousValue, int newValue)
        {
            ApplyColorLayer(newValue);
            LampManager.Instance.ApplyColorToAllLamps();
        }

        public void RequestColorChange(int colorIndex)
        {
            if (!IsServer && !IsHost)
            {
                RequestColorChangeServerRpc(colorIndex);
            }
            else
            {
                activeColorIndex.Value = colorIndex;
            }
        }

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
        private void RequestColorChangeServerRpc(int colorIndex)
        {
            activeColorIndex.Value = colorIndex;
        }

        public void RequestColorPreview(int colorIndex)
        {
            if (!IsServer && !IsHost)
            {
                RequestColorPreviewServerRpc(colorIndex);
            }
            else
            {
                ApplyColorPreviewClientRpc(colorIndex);
            }
        }

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
        private void RequestColorPreviewServerRpc(int colorIndex)
        {
            ApplyColorPreviewClientRpc(colorIndex);
        }

        [Rpc(SendTo.Everyone)]
        private void ApplyColorPreviewClientRpc(int colorIndex)
        {
            // Apply visual preview without changing the actual active color
            ApplyColorLayer(colorIndex);

            // Also update lamp colors for preview with specific color index
            if (LampManager.Instance != null)
            {
                LampManager.Instance.ApplyColorToAllLamps(colorIndex);
            }
        }

        private void ApplyColorLayer(int colorIndex)
        {
            if (colorGroups == null || colorIndex < 0 || colorIndex >= colorGroups.Length)
                return;

            for (int i = 0; i < colorGroups.Length; i++)
            {
                bool shouldBeVisible = (i != colorIndex);

                if (colorGroups[i].tilemaps != null)
                {
                    foreach (var tilemap in colorGroups[i].tilemaps)
                    {
                        if (tilemap != null)
                        {
                            tilemap.gameObject.SetActive(shouldBeVisible);
                        }
                    }
                }
            }

            if (colorIndex < backgroundColors.Length)
            {
                SetBackgroundColor(backgroundColors[colorIndex]);
            }
        }

        private void SetBackgroundColor(Color color)
        {
            if (backgroundCameras == null || backgroundCameras.Length == 0)
            {
                backgroundCameras = FindObjectsByType<Camera>(FindObjectsSortMode.None);
            }

            if (backgroundCameras == null)
                return;

            foreach (var cam in backgroundCameras)
            {
                if (cam != null)
                {
                    cam.backgroundColor = color;
                }
            }
        }

        public int GetActiveColorIndex()
        {
            return activeColorIndex.Value;
        }

        public string GetActiveColorName()
        {
            if (activeColorIndex.Value >= 0 && activeColorIndex.Value < colorGroups.Length)
            {
                return colorGroups[activeColorIndex.Value].colorName;
            }
            return "Unknown";
        }
    }
}
