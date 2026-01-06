using UnityEngine;
using UnityEngine.Tilemaps;

public class PressurePlate : MonoBehaviour
{
    [Header("Plate Settings")]
    public DoorController door;
    public Tilemap pressurePlate; // das GameObject, das bewegt werden soll
    public float pressDepth = 0.15f;
    public float moveSpeed = 4f;
    [Tooltip("Maximale Strecke, die die Platte bewegen darf")]
    public float maxMovementDistance = 0.2f;

    private Transform plateTransform;
    private Vector3 startLocalPos;
    private Vector3 pressedLocalPos;
    private Vector3 targetLocalPos;
    private bool playerOnPlate = false;
    private bool hasMovedTooFar = false;

    // Referenz zum Spieler auf der Platte
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
        Vector3 newLocalPos = Vector3.MoveTowards(plateTransform.localPosition, targetLocalPos, moveSpeed * Time.deltaTime);

        // Prüfen, ob die maximale Bewegungsdistanz überschritten würde
        float distanceFromStart = Vector3.Distance(newLocalPos, startLocalPos);

        if (distanceFromStart <= maxMovementDistance)
        {
            plateTransform.localPosition = newLocalPos;
            hasMovedTooFar = false;
        }
        else
        {
            // Bewegung auf maxMovementDistance begrenzen
            Vector3 direction = (targetLocalPos - startLocalPos).normalized;
            plateTransform.localPosition = startLocalPos + direction * maxMovementDistance;

        }

        // Spieler mit der Platte bewegen
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
        if (!other.CompareTag("Player")) return;

        playerOnPlate = true;
        playerOnPlatform = other.transform;

        if (!hasMovedTooFar)
        {
            SetPressed(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        playerOnPlate = false;
        playerOnPlatform = null;

        SetPressed(false);
        hasMovedTooFar = false; // Reset beim Verlassen
    }

    private void SetPressed(bool pressed)
    {
        if (!hasMovedTooFar || !pressed)
        {
            targetLocalPos = pressed ? pressedLocalPos : startLocalPos;
            if (door != null)
            {
                if (pressed) door.Open();
                else door.Close();
            }
        }
    }
}