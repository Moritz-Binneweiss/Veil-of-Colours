using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public class ItemManager : MonoBehaviour
{
   
   [Header("Key Settings")]
    public string keyID = "KeyA"; // Für Keys: welcher Key wird gesammelt?
    public string requiredKeyID = "KeyA"; // Für Doors: welcher Key wird benötigt?


    [SerializeField]
    public UnityEvent OnUse;

    private static Dictionary<string, bool> playerKeys = new Dictionary<string, bool>();

     public static ItemManager Instance { get; private set; }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    private void Awake()
    {
        
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            // Initialize keys wenn noch nicht vorhanden
            if (playerKeys.Count == 0)
            {
                playerKeys["KeyA"] = false;
                playerKeys["KeyB"] = false;
                playerKeys["KeyC"] = false; // Für weitere Keys
            }
        }
    
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            UseItem();
            
        }

    }

    public void UseItem()
    {
        OnUse.Invoke();
    }

    public void CollectKey()
    {
         if (playerKeys.ContainsKey(keyID))
        {
            playerKeys[keyID] = true;
            Debug.Log($"Key {keyID} collected!");
        Destroy(gameObject);
        }
        
        
    }

    public void UseKey()
    {
           if (playerKeys.ContainsKey(requiredKeyID) && playerKeys[requiredKeyID])
        {
            playerKeys[requiredKeyID] = false; // Key verbrauchen
            Debug.Log($"Door opened with {requiredKeyID}!");
            Destroy(gameObject); // Door zerstören
        }
    }
}
