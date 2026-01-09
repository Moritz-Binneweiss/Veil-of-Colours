using Unity.Netcode;
using UnityEngine;

public class PressurePlateManager : NetworkBehaviour
{
    public static PressurePlateManager Instance { get; private set; }

    [Header("Assign GameObjects (plates and optional doors)")]
    public GameObject[] plateObjects = new GameObject[0];
    public GameObject[] doorObjects = new GameObject[0];

    private PressurePlate[] plates = new PressurePlate[0];
    private bool[] plateStates = new bool[0];

    private void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        Instance = this;
    }

    private void Start()
    {
        BuildArraysFromGameObjects();
    }

    private void BuildArraysFromGameObjects()
    {
        plates = new PressurePlate[plateObjects.Length];
        for (int i = 0; i < plateObjects.Length; i++)
        {
            var go = plateObjects[i];
            if (go == null)
                continue;

            var plate = go.GetComponent<PressurePlate>();
            if (plate == null)
            {
                Debug.LogWarning(
                    $"PressurePlate component missing on plateObjects[{i}] ({go.name})."
                );
                continue;
            }

            plates[i] = plate;
        }

        EnsureStateArray();
    }

    private void EnsureStateArray()
    {
        if (plateStates == null || plateStates.Length != plates.Length)
        {
            plateStates = new bool[plates.Length];
            for (int i = 0; i < plateStates.Length; i++)
                plateStates[i] = false;
        }
    }

    // Anfrage by index (bestehende Methode)
    public void RequestActivatePlate(int id, bool pressed)
    {
        EnsureStateArray();
        if (id < 0 || id >= plates.Length)
            return;

        if (IsServer)
            ActivatePlateServer(id, pressed);
        else
            ActivatePlateServerRpc(id, pressed);
    }

    // Neue Überladung: Anfrage per Plate-GameObject -> find index -> weiterleiten
    public void RequestActivatePlate(GameObject plateGO, bool pressed)
    {
        if (plateGO == null)
            return;
        int id = -1;
        for (int i = 0; i < plateObjects.Length; i++)
        {
            if (plateObjects[i] == plateGO)
            {
                id = i;
                break;
            }
        }

        if (id == -1)
            return;
        RequestActivatePlate(id, pressed);
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void ActivatePlateServerRpc(int id, bool pressed)
    {
        ActivatePlateServer(id, pressed);
    }

    private void ActivatePlateServer(int id, bool pressed)
    {
        if (id < 0 || id >= plates.Length)
            return;
        EnsureStateArray();

        if (plateStates[id] == pressed)
            return;

        plateStates[id] = pressed;
        ActivatePlateClientRpc(id, pressed);
    }

    [ClientRpc]
    private void ActivatePlateClientRpc(int id, bool pressed)
    {
        if (id < 0 || id >= plates.Length)
            return;
        plates[id]?.ActivateLocal(pressed);
        ApplyDoorStateLocal(id, pressed);
    }

    // Verwendet nur die doorObjects-Referenz, die im Manager im Inspector gesetzt wird
    public void ApplyDoorStateLocal(int id, bool pressed)
    {
        if (id < 0 || id >= doorObjects.Length)
            return;
        var doorGo = doorObjects[id];
        if (doorGo == null)
            return;
        var door = doorGo.GetComponent<DoorController>();
        if (door == null)
            return;

        if (pressed)
            door.Open();
        else
            door.Close();
    }
}
