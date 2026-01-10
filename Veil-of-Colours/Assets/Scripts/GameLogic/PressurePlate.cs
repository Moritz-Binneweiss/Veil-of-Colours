using UnityEngine;
using UnityEngine.Tilemaps;

public class PressurePlate : MonoBehaviour
{
    [Header("Plate Settings")]
    public Tilemap pressurePlate;
    public float pressDepth = 0.15f;
    public float moveSpeed = 4f;

    [Tooltip("Maximale Strecke, die die Platte bewegen darf")]
    public float maxMovementDistance = 0.2f;

    private Transform plateTransform;
    private Vector3 startLocalPos;
    private Vector3 pressedLocalPos;
    private Vector3 targetLocalPos;
    private bool hasMovedTooFar = false;

    private Transform playerOnPlatform = null;

    private void Awake()
    {
        plateTransform = (pressurePlate != null) ? pressurePlate.transform : transform;
        startLocalPos = plateTransform.localPosition;
        pressedLocalPos = startLocalPos + Vector3.down * pressDepth;
        targetLocalPos = startLocalPos;
    }

    private void Update()
    {
        Vector3 previousWorldPos = plateTransform.position;
        Vector3 newLocalPos = Vector3.MoveTowards(
            plateTransform.localPosition,
            targetLocalPos,
            moveSpeed * Time.deltaTime
        );

        float distanceFromStart = Vector3.Distance(newLocalPos, startLocalPos);

        if (distanceFromStart <= maxMovementDistance)
        {
            plateTransform.localPosition = newLocalPos;
            hasMovedTooFar = false;
        }
        else
        {
            Vector3 direction = (targetLocalPos - startLocalPos).normalized;
            plateTransform.localPosition = startLocalPos + direction * maxMovementDistance;
        }

        Vector3 currentWorldPos = plateTransform.position;
        Vector3 deltaMovement = currentWorldPos - previousWorldPos;
        MovePlayerWithPlatform(deltaMovement);
    }

    private void MovePlayerWithPlatform(Vector3 deltaMovement)
    {
        if (playerOnPlatform != null)
        {
            playerOnPlatform.position += deltaMovement;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerOnPlatform = other.transform;

        var mgr = PressurePlateManager.Instance;
        if (mgr != null && mgr.IsServer)
        {
            // Server führt die Aktivierung direkt aus (Server-Instanz des Managers entscheidet und broadcastet)
            mgr.RequestActivatePlate(gameObject, true);
        }
        else
        {
            mgr?.RequestActivatePlate(gameObject, true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerOnPlatform = null;

        var mgr = PressurePlateManager.Instance;
        if (mgr != null && mgr.IsServer)
        {
            mgr.RequestActivatePlate(gameObject, false);
        }
        else
        {
            mgr?.RequestActivatePlate(gameObject, false);
        }

        hasMovedTooFar = false;
    }

    // Bewegt nur die Plate lokal; Door-Zustand wird vom Manager gesetzt (ApplyDoorStateLocal)
    public void ActivateLocal(bool pressed)
    {
        if (!hasMovedTooFar || !pressed)
        {
            targetLocalPos = pressed ? pressedLocalPos : startLocalPos;
        }
    }

    private void Reset() => GetComponent<Collider2D>().isTrigger = true;
}
