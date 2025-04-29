using UnityEngine;
using UnityEngine.Audio;

public class FaceAnimation : MonoBehaviour
{
    [SerializeField] private GameObject blendShapesMesh;
    [SerializeField] private string BlendShapeKiss;
    [SerializeField] private string BlendShapeLipsClosed;
    [SerializeField] private string BlendShapeMouthOpen;
    [SerializeField] private float volumeTreshold = 0.0001f;
    [SerializeField] private float maxLipsSpeed = 10f;
    [SerializeField] private string BlendShapeBlink;

    private AudioSource currentAudioSource;
    private bool isBlinking;
    private float blinkValue;
    private float[] energy = new float[8];
    private float[] lipsyncBSW = new float[3];
    private float[] refFBins = { 0, 500, 700, 3000, 6000 };
    private float[] fBins;
    private float pitch;
    private float[] spectrumData;
    private int blendshapeIndexKiss = 0;
    private int blendshapeIndexLipsClosed = 1;
    private int blendshapeIndexMouthOpen = 2;
    private float currentBlendshapeValueKiss;
    private float currentBlendshapeValueLipsClosed;
    private float currentBlendshapeValueMouthOpen;

    SkinnedMeshRenderer skinnedMeshRenderer;

    public void Awake()
    {
        Init(); 
        skinnedMeshRenderer = blendShapesMesh.GetComponent<SkinnedMeshRenderer>();
    }

    public void Start()
    {
        if (BlendShapeKiss.Length>0)
        {
            blendshapeIndexKiss = skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex(BlendShapeKiss);
        }
        if (BlendShapeLipsClosed.Length > 0)
        {
            blendshapeIndexLipsClosed = skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex(BlendShapeLipsClosed);
        }
        if (BlendShapeMouthOpen.Length > 0)
        {
            blendshapeIndexMouthOpen = skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex(BlendShapeMouthOpen);
        }
    }

    // Initialize Lipsync with default values for threshold, smoothness, and pitch
    public void Init(float pitch = 1.0f)
    {
        this.pitch = pitch;
        DefineFBins(this.pitch);
        spectrumData = new float[256];  // Unity's default FFT size = 1024
    }

    private void DefineFBins(float pitch)
    {
        fBins = new float[refFBins.Length];
        for (int i = 0; i < refFBins.Length; i++)
        {
            fBins[i] = refFBins[i] * pitch;
        }
    }

    // Start playing the audio and processing lipsync
    public void SetAudioSource(AudioSource audioSource)
    {
        currentAudioSource = audioSource;
    }

    // Update is called once per frame
    void Update()
    {
        if (currentAudioSource.isPlaying)
        {
            ProcessAudioForLipsync();
        }
    }

    // Process the audio using FFT and compute lipsync blendshape weights
    private void ProcessAudioForLipsync()
    {
        // Get the current spectrum data (FFT)
        currentAudioSource.GetSpectrumData(spectrumData, 0, FFTWindow.BlackmanHarris);

        // Analyze the energy in different frequency bins
        BinAnalysis(spectrumData);

        // Calculate lipsync blendshape weights based on frequency analysis
        LipAnalysis();

        MoveLips();
    }

    private void BlinkEyes()
    {
        if (!isBlinking)
        {
            if (Random.value > .99f)
            {
                isBlinking = true;
                blinkValue = 0;
            }
        }
        else
        {
            blinkValue += 10f;
            float blendValue = blinkValue;
            if (blendValue > 100)
            {
                blendValue = 100 - (blendValue - 100);
            }
            if (blinkValue > 200)
            {
                isBlinking = false;
            }
            else
            {
                skinnedMeshRenderer.SetBlendShapeWeight(skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex(BlendShapeBlink), blendValue);
            }
        }
    }

    void FixedUpdate()
    {
        if (BlendShapeBlink != null)
        {
            BlinkEyes();
        }
    }

    private void MoveLips()
    {
        if (lipsyncBSW[0] > currentBlendshapeValueKiss)
        {
            currentBlendshapeValueKiss += maxLipsSpeed;
            if (currentBlendshapeValueKiss > lipsyncBSW[0])
            {
                currentBlendshapeValueKiss = lipsyncBSW[0];
            }
        }
        if (lipsyncBSW[0] < currentBlendshapeValueKiss)
        {
            currentBlendshapeValueKiss -= maxLipsSpeed;
            if (currentBlendshapeValueKiss < lipsyncBSW[0])
            {
                currentBlendshapeValueKiss = lipsyncBSW[0];
            }
        }
        if (lipsyncBSW[1] > currentBlendshapeValueLipsClosed)
        {
            currentBlendshapeValueLipsClosed += maxLipsSpeed;
            if (currentBlendshapeValueLipsClosed > lipsyncBSW[1])
            {
                currentBlendshapeValueLipsClosed = lipsyncBSW[1];
            }
        }
        if (lipsyncBSW[1] < currentBlendshapeValueLipsClosed)
        {
            currentBlendshapeValueLipsClosed -= maxLipsSpeed;
            if (currentBlendshapeValueLipsClosed < lipsyncBSW[1])
            {
                currentBlendshapeValueLipsClosed = lipsyncBSW[1];
            }
        }
        if (lipsyncBSW[2] > currentBlendshapeValueMouthOpen)
        {
            currentBlendshapeValueMouthOpen += maxLipsSpeed;
            if (currentBlendshapeValueMouthOpen > lipsyncBSW[2])
            {
                currentBlendshapeValueMouthOpen = lipsyncBSW[2];
            }
        }
        if (lipsyncBSW[2] < currentBlendshapeValueMouthOpen)
        {
            currentBlendshapeValueMouthOpen -= maxLipsSpeed;
            if (currentBlendshapeValueMouthOpen < lipsyncBSW[2])
            {
                currentBlendshapeValueMouthOpen = lipsyncBSW[2];
            }
        }

        // Use these blendshape values to control your character's lipsync animation
        skinnedMeshRenderer.SetBlendShapeWeight(blendshapeIndexKiss, currentBlendshapeValueKiss);
        skinnedMeshRenderer.SetBlendShapeWeight(blendshapeIndexLipsClosed, currentBlendshapeValueLipsClosed);
        skinnedMeshRenderer.SetBlendShapeWeight(blendshapeIndexMouthOpen, currentBlendshapeValueMouthOpen);

    }

    // Analyze frequency bins to determine energy in different frequency ranges
    private void BinAnalysis(float[] fftData)
    {
        int nfft = fftData.Length;
        float fs = AudioSettings.outputSampleRate;

        // Calculate the average output value for every frequency range (bin)
        // important ranges are 500-700 Hz, 700-3000 Hz and 3000-6000 Hz
        // these correspond to indices 5-4, 7-32, 32-64 
        for (int binInd = 0; binInd < fBins.Length - 1; binInd++)
        {
            int indxIn = Mathf.RoundToInt(fBins[binInd] * nfft / (fs / 2));
            int indxEnd = Mathf.RoundToInt(fBins[binInd + 1] * nfft / (fs / 2));

            energy[binInd] = 0;
            for (int i = indxIn; i < indxEnd; i++)
            {
                float value = fftData[i];
                value = value > 0 ? value : 0;
                energy[binInd] += value;
            }
            energy[binInd] /= (indxEnd - indxIn);
        }
    }

    // Compute lipsync blendshape weights based on energy in frequency bins
    private void LipAnalysis()
    {
        if (energy == null) return;

        // Scale up the energy (if above treshold)
        if (energy[1] + energy[2] + energy[3] > volumeTreshold)
        {
            float scaleFactor = 1 / (energy[1] + energy[2] + energy[3]);
            energy[1] *= scaleFactor;
            energy[2] *= scaleFactor;
            energy[3] *= scaleFactor;
        }

        float value;

        // Kiss blend shape
        value = (0.5f - (energy[2])) * 2;
        if (energy[1] < 0.2f)
            value *= energy[1] * 5;
        lipsyncBSW[0] = 100 * Mathf.Clamp(value, 0, 1);

        // Lips closed blend shape
        value = energy[3] * 3;
        lipsyncBSW[1] = 100 * Mathf.Clamp(value, 0, 1);

        // Mouth open blend shape
        value = energy[1] * 0.8f - energy[3] * 0.8f;
        lipsyncBSW[2] = 100 * Mathf.Clamp(value, 0, 1);

//        Debug.Log($"Kiss: {lipsyncBSW[0]}, Lips Closed: {lipsyncBSW[1]}, Mouth open: {lipsyncBSW[2]}");
    }

    // Stop the audio and lipsync processing
    public void StopAudio()
    {
        if (currentAudioSource.isPlaying)
        {
            currentAudioSource.Stop();
        }
    }
}
