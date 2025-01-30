using UnityEngine;

public class TrafficLight : MonoBehaviour
{
    [SerializeField] Material materialRed;
    [SerializeField] Material materialOrange;
    [SerializeField] Material materialGreen;
    [SerializeField] Material materialGreenWalk;
    [SerializeField] Material materialRedWalk;
    float timeLeftLightPhase = 4f;
    int lightNumber = 0;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        UpdateLight();
    }

    private void UpdateLight()
    {
        if (lightNumber == 0)
        {
            materialRed.SetFloat("_EmissiveExposureWeight", 1);
            materialOrange.SetFloat("_EmissiveExposureWeight", 1);
            materialGreen.SetFloat("_EmissiveExposureWeight", 0);
            materialGreenWalk.SetFloat("_EmissiveExposureWeight", 1);
            materialRedWalk.SetFloat("_EmissiveExposureWeight", 0);
        }
        else if (lightNumber == 1)
        {
            materialRed.SetFloat("_EmissiveExposureWeight", 1);
            materialOrange.SetFloat("_EmissiveExposureWeight", 0);
            materialGreen.SetFloat("_EmissiveExposureWeight", 1);
            materialGreenWalk.SetFloat("_EmissiveExposureWeight", 1);
            materialRedWalk.SetFloat("_EmissiveExposureWeight", 0);
        }
        else if (lightNumber == 2)
        {            
            materialRed.SetFloat("_EmissiveExposureWeight", 0);
            materialOrange.SetFloat("_EmissiveExposureWeight", 1);
            materialGreen.SetFloat("_EmissiveExposureWeight", 1);
            materialGreenWalk.SetFloat("_EmissiveExposureWeight", 0);
            materialRedWalk.SetFloat("_EmissiveExposureWeight", 1);
        }
    }

    // Update is called once per frame
    void Update()
    {
        timeLeftLightPhase -= Time.deltaTime;
        if (timeLeftLightPhase <= 0)
        {
            timeLeftLightPhase = 4f;
            lightNumber++;
            if (lightNumber >= 3)
            {
                lightNumber = 0;
            }
            UpdateLight();
        }
    }
}
