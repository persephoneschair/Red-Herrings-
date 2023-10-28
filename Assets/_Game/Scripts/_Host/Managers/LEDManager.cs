using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LEDManager : SingletonMonoBehaviour<LEDManager>
{
    public Led[] leds;
    public Light environmentLight;

    public float cycleDelay = 0.25f;
    public int cycleIterations = 36;

    private void Start()
    {
        for (int i = 0; i < leds.Length; i++)
            leds[i].SetBulbColor((Led.BulbColor)(i % 6));
    }

    [Button]
    public void LightChase()
    {
        StartCoroutine(Chase());
    }

    IEnumerator Chase()
    {
        environmentLight.intensity = 0;
        int col = 0;
        for (int i = 0; i < cycleIterations; i++)
        {
            for(int j = 0; j < leds.Length; j++)
            {
                leds[j].SetBulbColor((Led.BulbColor)col);
                col = ((col + 1) % 6);
            }
            col = (col + 1) % 6;
            yield return new WaitForSeconds(cycleDelay);
        }
        environmentLight.intensity = 0.9f;
    }

    public void DimLights()
    {
        StartCoroutine(DimLightsRoutine());
    }

    IEnumerator DimLightsRoutine()
    {
        yield return new WaitForSeconds(3f);
        while(environmentLight.intensity > 0)
        {
            environmentLight.intensity -= 0.01f;
            yield return new WaitForSeconds(0.1f);
        }
    }
}