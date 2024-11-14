using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance { get; private set; }

    private CinemachineBasicMultiChannelPerlin cinemachineBasicMultiChannelPerlin;
    private float slowdownRate;

    // Start is called before the first frame update
    void Awake()
    {
        Instance = this;
        //        cinemachineBasicMultiChannelPerlin = cinemachineFreeLook.GetRig(1).GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        cinemachineBasicMultiChannelPerlin = GetComponent<CinemachineBasicMultiChannelPerlin>();
        cinemachineBasicMultiChannelPerlin.enabled = false;
    }

    public void ShakeCamera(float intensity, float slowdownRate)
    {
        cinemachineBasicMultiChannelPerlin.enabled = true;
        cinemachineBasicMultiChannelPerlin.AmplitudeGain = intensity;
        this.slowdownRate = slowdownRate;
    }

    // Update is called once per frame
    void Update()
    {
        if (cinemachineBasicMultiChannelPerlin.AmplitudeGain > 0)
        {
            cinemachineBasicMultiChannelPerlin.AmplitudeGain -= slowdownRate * Time.deltaTime;
        }
        else
        {
            cinemachineBasicMultiChannelPerlin.AmplitudeGain = 0;
            cinemachineBasicMultiChannelPerlin.enabled = false;
        }
    }
}
