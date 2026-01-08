using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;


public class Lamp : MonoBehaviour
{

   Transform mainLight;
   Transform flickerLight;

   public float minRandomIntensity;
    public float maxRandomIntensity;
    
   
   public float minRandomTime;
   public float maxRandomTime;
    
    UnityEngine.Rendering.Universal.Light2D mainLightComponent;
    UnityEngine.Rendering.Universal.Light2D flickerLightComponent;


    // Start is called before the first frame update
    void Start()
    {
        mainLight = this.transform.GetChild(0);
        flickerLight = this.transform.GetChild(1);
        mainLightComponent = mainLight.GetComponent<UnityEngine.Rendering.Universal.Light2D>();
        flickerLightComponent = flickerLight.GetComponent<UnityEngine.Rendering.Universal.Light2D>();

        StartCoroutine(Timer());
    }

    IEnumerator Timer()
    {
        for (; ; ) //this is while(true)
        {
            float randomIntensity = Random.Range(minRandomIntensity, maxRandomIntensity);
            flickerLightComponent.intensity = randomIntensity;


            float randomTime = Random.Range(minRandomTime, maxRandomTime);
            yield return new WaitForSeconds(randomTime);
        }
    }
}