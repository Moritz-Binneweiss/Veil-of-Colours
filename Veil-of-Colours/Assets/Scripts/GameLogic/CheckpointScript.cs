using UnityEngine;
using System.Collections.Generic;

public class CheckpointScript : MonoBehaviour
{

   
   private bool activated = false;

    private void Reset() => GetComponent<Collider2D>().isTrigger = true;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (activated) return;
        activated = true;
        CheckpointManager.Instance?.ActivateCheckpoint(gameObject);
        Debug.Log($"Checkpoint '{name}' activated");
    }
}
