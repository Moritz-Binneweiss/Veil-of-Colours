using System.Collections;
using UnityEngine;

public class DoorController : MonoBehaviour
{
    [Header("Door Movement")]
    public float openDistance = 2f;
    public float moveSpeed = 2f;
    public bool startOpen = false;

    private Vector3 closedPos;
    private Vector3 openPos;
    private Coroutine moveRoutine;

    private void Awake()
    {
        closedPos = transform.localPosition;
        openPos = closedPos + Vector3.up * openDistance;
        if (startOpen) transform.localPosition = openPos;
    }

    public void Open()
    {
        StartMove(openPos);
    }

    public void Close()
    {
        StartMove(closedPos);
    }

    private void StartMove(Vector3 target)
    {
        if (moveRoutine != null) StopCoroutine(moveRoutine);
        moveRoutine = StartCoroutine(MoveTo(target));
    }

    private IEnumerator MoveTo(Vector3 target)
    {
        while ((transform.localPosition - target).sqrMagnitude > 0.0001f)
        {
            transform.localPosition = Vector3.MoveTowards(transform.localPosition, target, moveSpeed * Time.deltaTime);
            yield return null;
        }

        transform.localPosition = target;
        moveRoutine = null;
    }
}
