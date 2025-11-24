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
                SetupPlayerLevel();
            }
        }

        private void SetupPlayerLevel()
        {
            // Additional player setup can be done here
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
