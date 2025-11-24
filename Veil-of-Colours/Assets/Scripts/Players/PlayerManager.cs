using Unity.Netcode;
using UnityEngine;

namespace VeilOfColours.Players
{
    public class PlayerManager : NetworkBehaviour
    {
        [Header("Player Settings")]
        [SerializeField]
        private string playerName = "Player";

        [SerializeField]
        private bool isPlayerOne = true;

        [Header("Level References")]
        [SerializeField]
        private GameObject targetLevel;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            if (IsOwner)
            {
                Debug.Log($"{playerName} spawned and controlled by this client");
                SetupPlayerLevel();
            }
            else
            {
                Debug.Log($"{playerName} spawned but controlled by another client");
            }
        }

        private void SetupPlayerLevel()
        {
            // Additional setup for the player's assigned level can be done here
            if (targetLevel != null)
            {
                Debug.Log($"{playerName} assigned to level: {targetLevel.name}");
            }
        }

        public string GetPlayerName()
        {
            return playerName;
        }

        public bool IsPlayerOne()
        {
            return isPlayerOne;
        }
    }
}
