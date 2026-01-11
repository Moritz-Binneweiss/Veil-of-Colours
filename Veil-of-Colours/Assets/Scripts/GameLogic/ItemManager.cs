using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ItemManager : NetworkBehaviour
{
    [Header("Keys")]
    public NetworkList<NetworkObjectReference> collectedKeys;
    public NetworkVariable<bool> isKeyCollected = new NetworkVariable<bool>(false);

    [System.Serializable]
    public class LevelDoorMapping
    {
        public string levelName;
        public GameObject triggerDoor; // The door that must be activated
        public GameObject[] targetDoors; // The doors that should open
    }

    [Header("Level Door Management")]
    [SerializeField]
    private List<LevelDoorMapping> levelMappings = new List<LevelDoorMapping>();

    [Header("Door Management")]
    [SerializeField]
    private List<GameObject> allDoors = new List<GameObject>();

    private NetworkList<NetworkObjectReference> activatedDoors;

    public static ItemManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        // Note: ItemManager should be a root GameObject if you want to use DontDestroyOnLoad
        // DontDestroyOnLoad(gameObject);

        collectedKeys = new NetworkList<NetworkObjectReference>();
        activatedDoors = new NetworkList<NetworkObjectReference>();
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void ActivateDoorServerRpc(NetworkObjectReference doorRef)
    {
        if (!IsServer)
            return;

        // Check if already activated
        foreach (var activatedDoor in activatedDoors)
        {
            if (activatedDoor.Equals(doorRef))
                return; // Already activated
        }

        // Add to list
        activatedDoors.Add(doorRef);

        // Check if all doors are activated
        if (activatedDoors.Count >= allDoors.Count)
        {
            TriggerOpenGateClientRpc();
        }

        if (doorRef.TryGet(out NetworkObject doorNetObj))
        {
            GameObject activatedDoorObj = doorNetObj.gameObject;

            foreach (var mapping in levelMappings)
            {
                if (mapping.triggerDoor == activatedDoorObj)
                {
                    TriggerSpecificDoorsClientRpc(GetNetworkReferences(mapping.targetDoors));
                    break;
                }
            }
        }
    }

    private NetworkObjectReference[] GetNetworkReferences(GameObject[] gameObjects)
    {
        List<NetworkObjectReference> refs = new List<NetworkObjectReference>();

        foreach (var go in gameObjects)
        {
            if (go != null)
            {
                var netObj = go.GetComponent<NetworkObject>();
                if (netObj != null)
                {
                    refs.Add(netObj);
                }
            }
        }

        return refs.ToArray();
    }

    [Rpc(SendTo.Everyone)]
    private void TriggerSpecificDoorsClientRpc(NetworkObjectReference[] targetDoorRefs)
    {
        foreach (var doorRef in targetDoorRefs)
        {
            if (doorRef.TryGet(out NetworkObject doorNetObj))
            {
                Item doorItem = doorNetObj.GetComponent<Item>();
                if (doorItem != null)
                {
                    doorItem.ChangeDoorToActive();
                }
            }
        }
    }

    [Rpc(SendTo.Everyone)]
    private void TriggerOpenGateClientRpc()
    {
        // Find all items with gates and open them
        Item[] allItems = FindObjectsByType<Item>(FindObjectsSortMode.None);
        foreach (Item item in allItems)
        {
            if (item.gateToOpen != null)
            {
                item.OpenGate();
            }
        }
    }

    public void ActivateDoorClient()
    {
        Item[] allItems = FindObjectsByType<Item>(FindObjectsSortMode.None);
        foreach (Item item in allItems)
        {
            if (item.activatedDoor != null)
            {
                item.ChangeDoorToActive();
            }
        }
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void CollectKeyServerRpc(NetworkObjectReference keyRef)
    {
        if (!IsServer)
            return;

        // Check if key is already collected
        foreach (var collectedKey in collectedKeys)
        {
            if (collectedKey.Equals(keyRef))
                return;
        }

        // Add key
        collectedKeys.Add(keyRef);
        isKeyCollected.Value = true;
    }

    // Check if key was collected
    public bool HasKey(GameObject keyGameObject)
    {
        if (keyGameObject == null)
            return false;

        var keyNetObj = keyGameObject.GetComponent<NetworkObject>();
        if (keyNetObj == null)
            return false;

        NetworkObjectReference keyRef = keyNetObj;

        foreach (var collectedKey in collectedKeys)
        {
            if (collectedKey.Equals(keyRef))
                return true;
        }

        return false;
    }
}
