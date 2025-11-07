using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace VeilOfColours.Puzzle
{
    /// <summary>
    /// A switch that can be activated by a player and syncs across the network
    /// When activated, it updates the PuzzleManager state
    /// </summary>
    public class NetworkedSwitch : MonoBehaviour
    {
        [Header("Switch Settings")]
        [SerializeField]
        private string switchId = "A"; // Which switch in PuzzleManager (A, B, C, D)

        [SerializeField]
        private bool isToggle = false; // Toggle or one-time activation?

        [Header("Visual Feedback")]
        [SerializeField]
        private SpriteRenderer spriteRenderer;

        [SerializeField]
        private Color inactiveColor = Color.gray;

        [SerializeField]
        private Color activeColor = Color.green;

        private bool isPlayerNearby = false;
        private bool isActive = false;

        private void Start()
        {
            if (spriteRenderer == null)
                spriteRenderer = GetComponent<SpriteRenderer>();

            UpdateVisuals();
        }

        private void OnEnable()
        {
            // Subscribe to puzzle manager events
            if (PuzzleManager.Instance != null)
            {
                SubscribeToEvents();
            }
        }

        private void OnDisable()
        {
            // Unsubscribe from events
            if (PuzzleManager.Instance != null)
            {
                UnsubscribeFromEvents();
            }
        }

        private void SubscribeToEvents()
        {
            switch (switchId)
            {
                case "A":
                    PuzzleManager.Instance.OnSwitchAChanged += OnSwitchStateChanged;
                    break;
                case "B":
                    PuzzleManager.Instance.OnSwitchBChanged += OnSwitchStateChanged;
                    break;
            }
        }

        private void UnsubscribeFromEvents()
        {
            switch (switchId)
            {
                case "A":
                    PuzzleManager.Instance.OnSwitchAChanged -= OnSwitchStateChanged;
                    break;
                case "B":
                    PuzzleManager.Instance.OnSwitchBChanged -= OnSwitchStateChanged;
                    break;
            }
        }

        private void Update()
        {
            // Debug: Show when player is nearby
            if (isPlayerNearby)
            {
                // Check if Keyboard is available
                if (Keyboard.current == null)
                {
                    Debug.LogWarning($"[NetworkedSwitch {switchId}] Keyboard.current is NULL!");
                    return;
                }

                // Check for E key press
                if (Keyboard.current.eKey.wasPressedThisFrame)
                {
                    Debug.Log($"[NetworkedSwitch {switchId}] E KEY PRESSED! Activating switch...");
                    ActivateSwitch();
                }
            }
        }

        private void ActivateSwitch()
        {
            Debug.Log(
                $"[NetworkedSwitch {switchId}] ActivateSwitch called! isToggle={isToggle}, isActive={isActive}"
            );

            if (!isToggle && isActive)
            {
                Debug.Log(
                    $"[NetworkedSwitch {switchId}] Already active and not a toggle - ignoring"
                );
                return; // Can't activate again if not a toggle
            }

            bool newState = isToggle ? !isActive : true;
            Debug.Log($"[NetworkedSwitch {switchId}] Sending RPC with newState={newState}");

            // Send to PuzzleManager (will sync across network)
            if (PuzzleManager.Instance != null)
            {
                PuzzleManager.Instance.ActivateSwitchServerRpc(switchId, newState);
            }
            else
            {
                Debug.LogError($"[NetworkedSwitch {switchId}] PuzzleManager.Instance is NULL!");
            }
        }

        private void OnSwitchStateChanged(bool newState)
        {
            isActive = newState;
            UpdateVisuals();
            Debug.Log($"[NetworkedSwitch {switchId}] State changed to: {newState}");
        }

        private void UpdateVisuals()
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.color = isActive ? activeColor : inactiveColor;
            }
        }

        // Trigger detection for player proximity
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                isPlayerNearby = true;
                Debug.Log($"[NetworkedSwitch {switchId}] Player nearby - Press E to activate");
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                isPlayerNearby = false;
            }
        }
    }
}
