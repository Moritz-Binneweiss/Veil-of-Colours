using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

public class FarbMechanic: MonoBehaviour
{
     [Header("Farblayer GameObjects")]
    [SerializeField]
    private Tilemap Farblayer1;
    
    [SerializeField]
    private Tilemap Farblayer2;

     [Header("Input Actions")]
        [SerializeField]
        private InputActionReference Color1;

        [SerializeField]
        private InputActionReference Color2;


    void Update()
    {
        ReadInput(); 
    }

     private void ReadInput()
    {
        // Prüfe Input für Color1 (Taste 1)
        if (Color1.action.WasPressedThisFrame())
        {
            ActivateFarblayer1();
        }

        // Prüfe Input für Color2 (Taste 2) 
        if (Color2.action.WasPressedThisFrame())
        {
            ActivateFarblayer2();
        }
    }

    private void ActivateFarblayer1()
    {
        if (Farblayer1 != null)
        {
            Farblayer1.gameObject.SetActive(false);
            Debug.Log("Farblayer1 activated!");
            
            // Optional: Farblayer2 deaktivieren
            if (Farblayer2 != null)
            {
                Farblayer2.gameObject.SetActive(true);
            }
        }
    }

    private void ActivateFarblayer2()
    {
        if (Farblayer2 != null)
        {
            Farblayer2.gameObject.SetActive(false);
            Debug.Log("Farblayer2 activated!");
            
            // Optional: Farblayer1 deaktivieren
            if (Farblayer1 != null)
            {
                Farblayer1.gameObject.SetActive(true);
            }
        }
    }
    

     void OnEnable()
    {
        Color1.action.Enable();
        Color2.action.Enable();
    }

    void OnDisable()
    {
        Color1.action.Disable();
        Color2.action.Disable();
    }
}
