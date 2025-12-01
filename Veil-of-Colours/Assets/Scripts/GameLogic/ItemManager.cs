using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ItemManager : MonoBehaviour
{
    [Header("Key Settings")]
    public string keyID = "KeyA";
    public string requiredKeyID = "KeyA";

    [SerializeField]
    public UnityEvent OnUse;

    private static Dictionary<string, bool> playerKeys = new Dictionary<string, bool>();

    public static ItemManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (playerKeys.Count == 0)
            {
                playerKeys["KeyA"] = false;
                playerKeys["KeyB"] = false;
                playerKeys["KeyC"] = false;
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
            playerKeys[requiredKeyID] = false;
            Debug.Log($"Door opened with {requiredKeyID}!");
            Destroy(gameObject);
        }
    }
}
