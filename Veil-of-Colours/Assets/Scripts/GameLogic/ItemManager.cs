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
        public GameObject triggerDoor;      // Die Tür die aktiviert werden muss
        public GameObject[] targetDoors;    // Die Türen die sich öffnen sollen
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
            DontDestroyOnLoad(gameObject);

            collectedKeys = new NetworkList<NetworkObjectReference>();
            activatedDoors = new NetworkList<NetworkObjectReference>();
        }
    

    

     [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void ActivateDoorServerRpc(NetworkObjectReference doorRef)
    {
        if (!IsServer) return;
        
        // Prüfe ob bereits aktiviert
        foreach (var activatedDoor in activatedDoors)
        {
            if (activatedDoor.Equals(doorRef))
                return; // Bereits aktiviert
        }
        
        // Füge zur Liste hinzu
        activatedDoors.Add(doorRef);
        Debug.Log($"Server: Door activated. Total: {activatedDoors.Count}/{allDoors.Count}");


        
        // Prüfe ob alle Türen aktiviert sind
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
                    Debug.Log($"Triggering doors for level: {mapping.levelName}");
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
        Debug.Log($"Activating {targetDoorRefs.Length} specific doors on client");
        
        foreach (var doorRef in targetDoorRefs)
        {
            if (doorRef.TryGet(out NetworkObject doorNetObj))
            {
                Item doorItem = doorNetObj.GetComponent<Item>();
                if (doorItem != null)
                {
                    doorItem.ChangeDoorToActive();
                    Debug.Log($"Activated door: {doorNetObj.gameObject.name}");
                }
            }
        }
    }

     [Rpc(SendTo.Everyone)]
    private void TriggerOpenGateClientRpc()
    {
        Debug.Log("All doors activated! Triggering gate opening on all clients...");
        
        // Finde alle Items mit Gates und öffne sie
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
    if (!IsServer) return;
    
    // Prüfe ob Key bereits gesammelt
    foreach (var collectedKey in collectedKeys)
    {
        if (collectedKey.Equals(keyRef))
            return;
    }
    
    // Füge Key hinzu
    collectedKeys.Add(keyRef);
    isKeyCollected.Value = true;
    
    Debug.Log($"Server: Key collected. Total keys: {collectedKeys.Count}");
}

// Prüfe ob Key gesammelt wurde
public bool HasKey(GameObject keyGameObject)
{
    if (keyGameObject == null) return false;
    
    var keyNetObj = keyGameObject.GetComponent<NetworkObject>();
    if (keyNetObj == null) return false;
    
    NetworkObjectReference keyRef = keyNetObj;
    
    foreach (var collectedKey in collectedKeys)
    {
        if (collectedKey.Equals(keyRef))
            return true;
    }
    
    return false;
}

}
