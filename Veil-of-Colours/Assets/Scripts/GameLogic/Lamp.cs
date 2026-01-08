using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using VeilOfColours.GameLogic;

public class Lamp : MonoBehaviour
{
    public static Lamp Instance { get; private set; }
    Transform mainLight;
    Transform flickerLight;

    public float minRandomIntensity;
    public float maxRandomIntensity;
    
    public float minRandomTime;
    public float maxRandomTime;

    [System.Serializable]
    public class LampColorData
    {
        [Header("Main Light")]
        public Color mainLightColor = Color.white;
        public float mainLightIntensity = 1f;
        
        [Header("Flicker Light")]
        public Color flickerLightColor = Color.white;
        public float flickerLightBaseIntensity = 1f;
    }

    [SerializeField]
    private LampColorData[] lampColorConfigs = new LampColorData[4];

    Light2D mainLightComponent;
    Light2D flickerLightComponent;

    private void Awake()
    {
        if (LampManager.Instance != null)
        {
            LampManager.Instance.RegisterLamp(this);
        }
    }
    
    // Start is called before the first frame update
    void Start()
    {
        mainLight = this.transform.GetChild(0);
        flickerLight = this.transform.GetChild(1);
        mainLightComponent = mainLight.GetComponent<Light2D>();
        flickerLightComponent = flickerLight.GetComponent<Light2D>();

        StartCoroutine(Timer());
    }

    IEnumerator Timer()
    {
        for (; ; ) //this is while(true)
        {
            float randomIntensity = Random.Range(minRandomIntensity, maxRandomIntensity);
            
            // Nutze die Base-Intensität der aktuellen Farbe als Grundlage
            int currentColorIndex = ColorManager.Instance.GetActiveColorIndex();
            if (currentColorIndex >= 0 && currentColorIndex < lampColorConfigs.Length)
            {
                float baseIntensity = lampColorConfigs[currentColorIndex].flickerLightBaseIntensity;
                flickerLightComponent.intensity = baseIntensity * randomIntensity;
            }
            else
            {
                flickerLightComponent.intensity = randomIntensity;
            }

            float randomTime = Random.Range(minRandomTime, maxRandomTime);
            yield return new WaitForSeconds(randomTime);
        }
    }

    public void ApplyLampColor()
    {
        int activeColorIndex = ColorManager.Instance.GetActiveColorIndex();
        
        if (activeColorIndex >= 0 && activeColorIndex < lampColorConfigs.Length)
        {
            SetLampColor(activeColorIndex);
        }
    }

    public void SetLampColor(int colorIndex)
    {
        if (colorIndex < 0 || colorIndex >= lampColorConfigs.Length)
        {
            Debug.LogWarning($"Invalid color index: {colorIndex}");
            return;
        }

        LampColorData colorData = lampColorConfigs[colorIndex];
        
        // Setze die Farben und Intensitäten
        mainLightComponent.color = colorData.mainLightColor;
        mainLightComponent.intensity = colorData.mainLightIntensity;
        
        flickerLightComponent.color = colorData.flickerLightColor;
        flickerLightComponent.intensity = colorData.flickerLightBaseIntensity;
        
    }

    // Legacy method für String-basierte Aufrufe (falls noch verwendet)
    public void SetLampColor(string colorName)
    {
        int colorIndex = colorName switch
        {
            "Blue" => 0,
            "Red" => 1,
            "Green" => 2,
            "Yellow" => 3,
            _ => -1
        };
        
        if (colorIndex >= 0)
        {
            SetLampColor(colorIndex);
        }
    }
}