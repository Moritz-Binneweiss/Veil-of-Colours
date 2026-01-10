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
        // Find all lamps in the scene automatically
        RegisterAllLampsInScene();
    }

    public void RegisterLamp(Lamp lamp)
    {
        if (!allLamps.Contains(lamp))
        {
            allLamps.Add(lamp);
        }
    }

    public void UnregisterLamp(Lamp lamp)
    {
        if (allLamps.Contains(lamp))
        {
            allLamps.Remove(lamp);
        }
    }

    public void RegisterAllLampsInScene()
    {
        Lamp[] foundLamps = FindObjectsByType<Lamp>(FindObjectsSortMode.None);
        foreach (Lamp lamp in foundLamps)
        {
            RegisterLamp(lamp);
        }
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

    public void ApplyColorToAllLamps(int colorIndex)
    {
        foreach (Lamp lamp in allLamps)
        {
            if (lamp != null)
            {
                lamp.SetLampColor(colorIndex);
            }
        }
    }

    public List<Lamp> GetAllLamps()
    {
        // Remove null references
        allLamps.RemoveAll(lamp => lamp == null);
        return allLamps;
    }

    public int GetLampCount()
    {
        allLamps.RemoveAll(lamp => lamp == null);
        return allLamps.Count;
    }
}
