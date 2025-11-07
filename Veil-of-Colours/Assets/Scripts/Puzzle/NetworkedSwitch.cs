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
            if (
                isPlayerNearby
                && Keyboard.current != null
                && Keyboard.current.eKey.wasPressedThisFrame
            )
            {
                ActivateSwitch();
            }
        }

        private void ActivateSwitch()
        {
            if (!isToggle && isActive)
                return;

            bool newState = isToggle ? !isActive : true;

            if (PuzzleManager.Instance != null)
            {
                PuzzleManager.Instance.ActivateSwitchServerRpc(switchId, newState);
            }
        }

        private void OnSwitchStateChanged(bool newState)
        {
            isActive = newState;
            UpdateVisuals();
        }

        private void UpdateVisuals()
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.color = isActive ? activeColor : inactiveColor;
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                isPlayerNearby = true;
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
