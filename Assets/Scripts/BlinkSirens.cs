using UnityEngine;

public class BlinkSirens : MonoBehaviour
{
    [SerializeField] Material materialSiren1;
    [SerializeField] Material materialSiren2;
    [SerializeField] float blinkPeriod = 2;
    [SerializeField] float maxIntensity = 1;

    void Update()
    {
        float intensity = Time.time % blinkPeriod / (blinkPeriod/2);        // 0..2 in 4 sec.
        if (intensity > 1)
        {
            intensity = 2 - intensity;      // 0..1..0
        }
        intensity *= 2;
        intensity--;    // -1..1..-1
        intensity *= maxIntensity;      // -max..max..-max
        materialSiren1.SetFloat("_EmissiveExposureWeight", intensity);
        materialSiren2.SetFloat("_EmissiveExposureWeight", 1-intensity);
    }
}
