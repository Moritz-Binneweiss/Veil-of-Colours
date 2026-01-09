using System.Collections.Generic;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering.Universal;
using VeilOfColours.General;

public class Item : NetworkBehaviour
{
    [Header("Item Configuration")]
    [SerializeField]
    private ItemType itemType;

    [Header("Door Configuration")]
    [SerializeField]
    private GameObject requiredKeyGameObject; // Which key is required (for doors)

    [Header("Events")]
    public UnityEvent OnUse;

    private bool isDoorActive;

    public Sprite openLock;

    public Sprite activatedDoor;

    public Light2D activatedDoorLight;

    [Header("Lock Visuals")]
    [SerializeField]
    public GameObject gateToOpen;

    public float openDistance = 3f;

    public float moveSpeed = 2f;

    public enum ItemType
    {
        Key,
        Door,
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() { }

    // Update is called once per frame
    void Update() { }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;
        switch (itemType)
        {
            case ItemType.Key:
                CollectKey();
                break;
            case ItemType.Door:
                TryOpenDoor();
                break;
        }
    }

    [ContextMenu("Collect Key (Editor)")]
    public void CollectKey()
    {
        if (Application.isPlaying && NetworkManager.Singleton != null)
        {
            // Network version
            var keyNetObj = gameObject.GetComponent<NetworkObject>();
            if (keyNetObj != null)
            {
                NetworkObjectReference keyRef = keyNetObj;
                ItemManager.Instance.CollectKeyServerRpc(keyRef);
                gameObject.SetActive(false);
            }
            else
            {
                Debug.LogError("Key GameObject needs NetworkObject for multiplayer!");
            }
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    [ContextMenu("Try Open Door (Editor)")]
    public void TryOpenDoor()
    {
        if (ItemManager.Instance.HasKey(requiredKeyGameObject))
        {
            OpenLock();
        }
    }

    [ContextMenu("Open Gate (Editor)")]
    public void OpenGate()
    {
        if (gateToOpen != null)
        {
            StartCoroutine(MoveGateCoroutine());
        }
    }

    private System.Collections.IEnumerator MoveGateCoroutine()
    {
        Vector3 startPosition = gateToOpen.transform.position;
        Vector3 targetPosition = startPosition + Vector3.up * openDistance;

        while (Vector3.Distance(gateToOpen.transform.position, targetPosition) > 0.01f)
        {
            gateToOpen.transform.position = Vector3.MoveTowards(
                gateToOpen.transform.position,
                targetPosition,
                moveSpeed * Time.deltaTime
            );
            yield return null;
        }

        gateToOpen.transform.position = targetPosition;
        WinGame();
    }

    [ContextMenu("Change Door To Active (Editor)")]
    public void ChangeDoorToActive()
    {
        isDoorActive = true;
        var spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null && activatedDoor != null)
        {
            spriteRenderer.sprite = isDoorActive ? activatedDoor : spriteRenderer.sprite;
            activatedDoorLight.enabled = isDoorActive;
        }
    }

    [ContextMenu("Open Lock (Editor)")]
    private void OpenLock()
    {
        if (gateToOpen != null)
        {
            var spriteRenderer = gateToOpen.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null && openLock != null)
            {
                spriteRenderer.sprite = openLock;
            }
        }
        // Send network notification (if NetworkObject is present)
        if (ItemManager.Instance != null && Application.isPlaying)
        {
            var networkObject = GetComponent<NetworkObject>();
            if (networkObject != null)
            {
                NetworkObjectReference doorRef = networkObject;
                ItemManager.Instance.ActivateDoorServerRpc(doorRef);
            }
            else
            {
                Debug.LogWarning($"Item {gameObject.name} missing NetworkObject for network sync");
            }
        }
    }

    public void WinGame()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ShowVictory();
        }
        else
        {
            Debug.LogError("GameManager.Instance not found!");
        }
    }
}
