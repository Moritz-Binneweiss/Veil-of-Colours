using System.Collections.Generic;
using UnityEngine;

public class CheckpointScript : MonoBehaviour
{
    public Sprite activatedCheckpointSprite;
    private bool activated = false;

    private void Reset() => GetComponent<Collider2D>().isTrigger = true;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;
        if (activated)
            return;
        activated = true;
        CheckpointManager.Instance?.ActivateCheckpoint(gameObject);
        SetSpriteActive(true);
        Debug.Log($"Checkpoint '{name}' activated");
    }

    public void SetSpriteActive(bool isActive)
    {
        var spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null && activatedCheckpointSprite != null)
        {
            spriteRenderer.sprite = isActive ? activatedCheckpointSprite : spriteRenderer.sprite;
        }
    }
}
