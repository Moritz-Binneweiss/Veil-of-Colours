using System.Collections.Generic;
using UnityEngine;

public class LampManager : MonoBehaviour
{
    public static LampManager Instance { get; private set; }
    
    [Header("Lamp Management")]
    [SerializeField]
    private List<Lamp> allLamps = new List<Lamp>();
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
    }
    
    private void Start()
    {
        // Finde alle Lampen in der Szene automatisch
        RegisterAllLampsInScene();
    }
    
    public void RegisterLamp(Lamp lamp)
    {
        if (!allLamps.Contains(lamp))
        {
            allLamps.Add(lamp);
            Debug.Log($"Registered lamp: {lamp.gameObject.name}");
        }
    }
    
    public void UnregisterLamp(Lamp lamp)
    {
        if (allLamps.Contains(lamp))
        {
            allLamps.Remove(lamp);
            Debug.Log($"Unregistered lamp: {lamp.gameObject.name}");
        }
    }
    
    public void RegisterAllLampsInScene()
    {
        Lamp[] foundLamps = FindObjectsByType<Lamp>(FindObjectsSortMode.None);
        foreach (Lamp lamp in foundLamps)
        {
            RegisterLamp(lamp);
        }
        Debug.Log($"Found and registered {foundLamps.Length} lamps in scene");
    }
    
    public void ApplyColorToAllLamps()
    {
        foreach (Lamp lamp in allLamps)
        {
            if (lamp != null)
            {
                lamp.ApplyLampColor();
            }
        }
    }
    
    public List<Lamp> GetAllLamps()
    {
        // Entferne null-Referenzen
        allLamps.RemoveAll(lamp => lamp == null);
        return allLamps;
    }
    
    public int GetLampCount()
    {
        allLamps.RemoveAll(lamp => lamp == null);
        return allLamps.Count;
    }
}