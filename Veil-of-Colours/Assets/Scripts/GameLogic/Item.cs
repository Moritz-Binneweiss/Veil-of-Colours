using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public class Item : MonoBehaviour
{

    [Header("Item Configuration")]
    [SerializeField]
    private ItemType itemType;
    
    [Header("Key Configuration")]
    [SerializeField]
    private GameObject keyGameObject; // Das Key GameObject (für Keys)
    
    [Header("Door Configuration")]
    [SerializeField]
    private GameObject requiredKeyGameObject; // Welcher Key wird benötigt (für Doors)
    
    [SerializeField]
    private DoorController[] doorControllers; // Türen die geöffnet werden
    
    [Header("Events")]
    public UnityEvent OnKeyCollected;
    public UnityEvent OnDoorOpened;
    public UnityEvent OnDoorLocked;

    public bool isDoorActive;

    public Sprite openLock; 

    public Sprite activatedDoor;

    public enum ItemType
    {
        Key,
        Door
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
         
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;
            switch (itemType)
            {
                case ItemType.Key:
                    CollectKey();
                    break;
                case ItemType.Door:
                    TryOpenDoor();
                    break;
            }
    }

    private void CollectKey()
    {
        
    }

    private void TryOpenDoor()
    {
        
    }

    private void OpenDoor()
    {
        
    }

    private void ChangeDoorToActive()
    {
        
    }

    private void OpenLock()
    {
        
    }

    
}
