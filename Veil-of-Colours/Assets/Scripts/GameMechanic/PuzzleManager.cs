using System;
using Unity.Netcode;
using UnityEngine;

namespace VeilOfColours.Puzzle
{
    /// <summary>
    /// Central manager for puzzle state synchronization between players.
    /// </summary>
    public class PuzzleManager : NetworkBehaviour
    {
        public static PuzzleManager Instance { get; private set; }

        [Header("Puzzle State")]
        public NetworkVariable<bool> SwitchA = new NetworkVariable<bool>(false);
        public NetworkVariable<bool> SwitchB = new NetworkVariable<bool>(false);
        public NetworkVariable<bool> SwitchC = new NetworkVariable<bool>(false);
        public NetworkVariable<bool> SwitchD = new NetworkVariable<bool>(false);

        public NetworkVariable<bool> DoorAOpen = new NetworkVariable<bool>(false);
        public NetworkVariable<bool> DoorBOpen = new NetworkVariable<bool>(false);

        public NetworkVariable<int> CurrentColorIndex = new NetworkVariable<int>(0);

        public event Action<bool> OnSwitchAChanged;
        public event Action<bool> OnSwitchBChanged;
        public event Action<bool> OnDoorAChanged;
        public event Action<bool> OnDoorBChanged;
        public event Action<int> OnColorChanged;

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

        public override void OnNetworkSpawn()
        {
            SwitchA.OnValueChanged += (oldValue, newValue) => OnSwitchAChanged?.Invoke(newValue);
            SwitchB.OnValueChanged += (oldValue, newValue) => OnSwitchBChanged?.Invoke(newValue);
            DoorAOpen.OnValueChanged += (oldValue, newValue) => OnDoorAChanged?.Invoke(newValue);
            DoorBOpen.OnValueChanged += (oldValue, newValue) => OnDoorBChanged?.Invoke(newValue);
            CurrentColorIndex.OnValueChanged += (oldValue, newValue) =>
                OnColorChanged?.Invoke(newValue);
        }

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
        public void ActivateSwitchServerRpc(string switchId, bool state)
        {
            switch (switchId)
            {
                case "A":
                    SwitchA.Value = state;
                    break;
                case "B":
                    SwitchB.Value = state;
                    break;
                case "C":
                    SwitchC.Value = state;
                    break;
                case "D":
                    SwitchD.Value = state;
                    break;
            }
        }

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
        public void SetDoorStateServerRpc(string doorId, bool open)
        {
            switch (doorId)
            {
                case "A":
                    DoorAOpen.Value = open;
                    break;
                case "B":
                    DoorBOpen.Value = open;
                    break;
            }
        }

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
        public void ChangeColorServerRpc(int colorIndex)
        {
            CurrentColorIndex.Value = colorIndex;
        }

        private void Update()
        {
            if (!IsServer)
                return;

            UpdateCrossLevelPuzzles();
        }

        private void UpdateCrossLevelPuzzles()
        {
            SyncDoorWithSwitch(SwitchA.Value, ref DoorBOpen);
            SyncDoorWithSwitch(SwitchB.Value, ref DoorAOpen);
        }

        private void SyncDoorWithSwitch(bool switchState, ref NetworkVariable<bool> door)
        {
            if (door.Value != switchState)
                door.Value = switchState;
        }
    }
}
