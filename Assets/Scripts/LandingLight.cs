using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class LandingLight : MonoBehaviour
{
    [SerializeField] Material materialLandingLight;
    [SerializeField] float phase;
    private Material materialLandingLightLocal;

    // Start is called before the first frame update
    void Awake()
    {
        MeshRenderer renderer = GetComponent<MeshRenderer>();
        materialLandingLightLocal = new Material(Shader.Find("HDRP/Lit"));
        materialLandingLightLocal.CopyPropertiesFromMaterial(materialLandingLight);
//        materialLandingLightLocal.SetColor("_EmissionColor", new Color(1f, 0f, 0f, 1f));
        List<Material> materials = new List<Material> { materialLandingLightLocal };
        renderer.SetMaterials(materials);
    }

    // Update is called once per frame
    void Update()
    {
        float currentPhase = (Time.time + phase) % 4;
        float intensity;
        if (currentPhase < 2)
        {
            intensity = currentPhase * 2f;
        }
        else
        {
            intensity = (4 - currentPhase) * 2f;
        }
        if (intensity > 1)
        {
            intensity = 1;
        }
        materialLandingLightLocal.SetFloat("_EmissiveExposureWeight", intensity);
    }
}
