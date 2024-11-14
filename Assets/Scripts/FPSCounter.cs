using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FPSCounter : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI fpsText;

    private readonly float pollingTime = 1f;
    private double time;
    private long frameCount;

    // Update is called once per frame
    void Update()
    {
        time += Time.deltaTime;
        frameCount++;

        if (time > pollingTime)
        {
            int frameRate = Mathf.RoundToInt((float)(frameCount / time));
            fpsText.text = frameRate.ToString() + " FPS";

            // disable to get average framerate
            //time -= pollingTime;
            // disable to get average framerate
            //frameCount = 0;
        }
    }
}
