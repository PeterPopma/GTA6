using UnityEngine;

public class Blink : MonoBehaviour
{
    [SerializeField] Material material;
    [SerializeField] float blinkPeriod = 2;
    [SerializeField] float maxIntensity = 1;

    void Update()
    {
        float intensity = Time.time % blinkPeriod / (blinkPeriod/2);        // 0..2 in 4 sec.
        if (intensity > 1)
        {
            intensity = 2 - intensity;      // 0..1..0
        }
        intensity *= maxIntensity;      // 0..max..0
        material.SetFloat("_EmissiveExposureWeight", intensity);
    }
}
