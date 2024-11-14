using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Component which will flicker a linked light while active by changing its
/// intensity between the min and max values given. The flickering can be
/// sharp or smoothed depending on the value of the smoothing parameter.
///
/// Just activate / deactivate this component as usual to pause / resume flicker
/// </summary>
public class LightFlicker : MonoBehaviour
{
    private new Light light;
    [Tooltip("Minimum random light intensity")]
    public float minIntensity = 0f;
    [Tooltip("Maximum random light intensity")]
    public float maxIntensity = 10000000f;
    [Tooltip("Time between light dims")]
    public float dimInterval = 5f;
    [Tooltip("How fast the light dims")]
    public float dimspeed = 10000f;

    private float dimTarget;
    private float timeLastDim;
    private int dimPhase;

    void Start()
    {
        light = GetComponent<Light>();
        timeLastDim = Time.time;
    }

    void FixedUpdate()
    {
        if (dimPhase == 0 && Time.time - timeLastDim > dimInterval)
        {
            timeLastDim = Time.time;
            dimTarget = Random.Range(minIntensity, maxIntensity);
            dimPhase = 1;
        }

        if (dimPhase == 1)
        {
            light.intensity -= dimspeed;
            if (light.intensity <= dimTarget)
            {
                light.intensity = dimTarget;
                dimPhase = 2;
            }
        }

        if (dimPhase == 2)
        {
            light.intensity += dimspeed;
            if (light.intensity >= maxIntensity)
            {
                light.intensity = maxIntensity;
                dimPhase = 0;
            }
        }
    }

}
