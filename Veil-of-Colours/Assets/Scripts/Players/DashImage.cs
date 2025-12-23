using UnityEngine;

public class DashImage : MonoBehaviour
{
    [SerializeField]
    private float activeTime = 0.1f;
    private float timeActivated;
    private float alpha;

    [SerializeField]
    private float alphaSet = 0.8f;
    private float alphaMultiplier = 0.85f;

    private Transform player;

    private SpriteRenderer spriteRenderer;
    private SpriteRenderer playerSpriteRenderer;

    private Color color;

    private void OnEnable()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Find player only if not cached yet
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
                playerSpriteRenderer = player.GetComponent<SpriteRenderer>();
            }
        }

        // Safety check: only proceed if player exists
        if (player == null || playerSpriteRenderer == null)
        {
            gameObject.SetActive(false);
            return;
        }

        alpha = alphaSet;
        spriteRenderer.sprite = playerSpriteRenderer.sprite;
        transform.position = player.position;
        transform.rotation = player.rotation;
        transform.localScale = player.localScale; // Copy player's facing direction
        timeActivated = Time.time;
    }

    private void Update()
    {
        alpha *= alphaMultiplier;
        color = new Color(1f, 1f, 1f, alpha);
        spriteRenderer.color = color;

        if (Time.time >= (timeActivated + activeTime))
        {
            DashAfterImage.Instance.AddToPool(gameObject);
        }
    }
}
