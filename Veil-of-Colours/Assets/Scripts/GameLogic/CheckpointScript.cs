using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class CheckpointScript : MonoBehaviour
{
    public Sprite activatedCheckpointSprite;
    public Light2D checkpointLight;
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
    }

    public void SetSpriteActive(bool isActive)
    {
        var spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null && activatedCheckpointSprite != null)
        {
            spriteRenderer.sprite = isActive ? activatedCheckpointSprite : spriteRenderer.sprite;
            checkpointLight.enabled = isActive;
            GetComponent<Collider2D>().enabled = false;
        }
    }
}
