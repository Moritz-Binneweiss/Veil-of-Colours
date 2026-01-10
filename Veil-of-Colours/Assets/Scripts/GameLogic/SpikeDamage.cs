using Unity.Netcode;
using UnityEngine;
using VeilOfColours.Players;

public class SpikeDamage : MonoBehaviour
{
    // If your spike colliders are triggers set this true, otherwise false
    public bool useTrigger = true;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!useTrigger)
            HandleCollision(collision.collider);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (useTrigger)
            HandleCollision(other);
    }

    private void HandleCollision(Collider2D other)
    {
        if (other == null)
            return;

        if (!other.CompareTag("Player"))
            return;

        // Only process if this spike is on the "Damage" layer
        if (!gameObject.CompareTag("Spikes"))
            return;

        if (CheckpointManager.Instance == null)
        {
            Debug.LogWarning("CheckpointManager instance not found. Cannot respawn player.");
            return;
        }

        // Check if this is the local player
        var playerMovement = other.GetComponent<PlayerMovement>();
        if (playerMovement != null && playerMovement.IsOwner)
        {
            CheckpointManager.Instance.TeleportToLastCheckpoint(other.gameObject);
            playerMovement.FreezeMovementForDuration(1f);
        }
        else if (playerMovement == null)
        {
            // Fallback for players without PlayerMovement component
            CheckpointManager.Instance.TeleportToLastCheckpoint(other.gameObject);
        }
    }
}
