using TMPro;
using UnityEngine;

namespace VeilOfColours.UI
{
    public class GameUI : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField]
        private TextMeshProUGUI climbStaminaText;

        private VeilOfColours.Players.PlayerMovement localPlayer;

        private void Update()
        {
            UpdateStaminaDisplay();
        }

        private void UpdateStaminaDisplay()
        {
            if (climbStaminaText == null)
                return;

            // Find local player if not found yet
            if (localPlayer == null)
            {
                var players = FindObjectsByType<VeilOfColours.Players.PlayerMovement>(
                    FindObjectsSortMode.None
                );
                foreach (var player in players)
                {
                    if (player.IsOwner)
                    {
                        localPlayer = player;
                        break;
                    }
                }
            }

            // Update climb stamina text
            if (localPlayer != null)
            {
                float stamina = localPlayer.GetCurrentClimbStamina();
                climbStaminaText.text = $"Climb Stamina: {stamina:F0}";
            }
            else
            {
                climbStaminaText.text = "Climb Stamina: --";
            }
        }
    }
}
