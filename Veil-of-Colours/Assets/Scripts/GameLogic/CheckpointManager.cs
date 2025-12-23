using System.Collections.Generic;
using UnityEngine;

public class CheckpointManager : MonoBehaviour
{
    public static CheckpointManager Instance { get; private set; }

    private List<GameObject> activeCheckpoints = new List<GameObject>();
    private int lastActiveIndex = -1;

    public Vector3 respawnOffset = Vector3.zero;
    public bool resetVelocity = true;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        // Note: CheckpointManager should be a root GameObject if you want to use DontDestroyOnLoad
        // DontDestroyOnLoad(gameObject);
    }

    public void RegisterCheckpoint(GameObject cp)
    {
        if (cp == null)
            return;
        if (!activeCheckpoints.Contains(cp))
            activeCheckpoints.Add(cp);
    }

    public void ActivateCheckpoint(GameObject cp)
    {
        if (cp == null)
            return;
        RegisterCheckpoint(cp);
        lastActiveIndex = activeCheckpoints.IndexOf(cp);
        Debug.Log($"Checkpoint aktiviert: {cp.name} (Index {lastActiveIndex})");
    }

    public GameObject GetLastCheckpoint()
    {
        return lastActiveIndex >= 0 ? activeCheckpoints[lastActiveIndex] : null;
    }

    [ContextMenu("Teleport To Last Checkpoint (Editor)")]
    public void TeleportToLastCheckpoint()
    {
        var last = GetLastCheckpoint();
        if (last == null)
        {
            Debug.Log("No active checkpoints to teleport to.");
            return;
        }

        GameObject playerToTeleport = GameObject.FindGameObjectWithTag("Player");
        if (playerToTeleport == null)
        {
            Debug.LogWarning("Player with tag 'Player' not found.");
            return;
        }

        Vector3 target = last.transform.position + respawnOffset;
        playerToTeleport.transform.position = target;

        var rb2d = playerToTeleport.GetComponent<Rigidbody2D>();
        if (rb2d != null && resetVelocity)
        {
            rb2d.linearVelocity = Vector2.zero;
            rb2d.angularVelocity = 0f;
            Physics2D.SyncTransforms();
        }

        Debug.Log("Player teleported to last checkpoint.");
    }

    public GameObject[] GetActiveCheckpointsArray() => activeCheckpoints.ToArray();
}
