using UnityEngine;
using System.Collections.Generic;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    private Dictionary<string, GameObject> soundPrefabs = new Dictionary<string, GameObject>();
    private Dictionary<string, Queue<GameObject>> soundPools = new Dictionary<string, Queue<GameObject>>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;

            // Cache original sound prefabs
            foreach (Transform child in transform)
            {
                string name = child.name;
                var audioSource = child.GetComponent<AudioSource>();
                audioSource.spatialBlend = 1f;      // 3D sound by default
                soundPrefabs[name] = child.gameObject;
                soundPools[name] = new Queue<GameObject>();

                // Disable originals to avoid accidental playback
                child.gameObject.SetActive(false);
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public void PlaySound(string soundName)
    {
        if (!soundPrefabs.ContainsKey(soundName))
        {
            Debug.LogWarning($"Sound '{soundName}' not found!");
            return;
        }

        GameObject soundObj;

        if (soundPools[soundName].Count > 0)
        {
            soundObj = soundPools[soundName].Dequeue();
            soundObj.SetActive(true);
        }
        else
        {
            soundObj = Instantiate(soundPrefabs[soundName]);
            soundObj.name = $"{soundName}_2DInstance";
            soundObj.SetActive(true);
        }

        // Place it at the origin or attach to manager (doesn’t matter in 2D)
        soundObj.transform.SetParent(this.transform);
        soundObj.transform.localPosition = Vector3.zero;

        AudioSource source = soundObj.GetComponent<AudioSource>();
        source.spatialBlend = 0f; // Force 2D
        source.Play();

        StartCoroutine(ReturnToPoolAfterPlayback(soundName, soundObj, source.clip.length));
    }


    public GameObject PlaySoundAt(string soundName, Vector3 position)
    {
        if (!soundPrefabs.ContainsKey(soundName))
        {
            Debug.Log($"Sound '{soundName}' not found!");
            return null;
        }

        GameObject soundObj;

        // Reuse from pool or create new
        if (soundPools[soundName].Count > 0)
        {
            soundObj = soundPools[soundName].Dequeue();
        }
        else
        {
            soundObj = Instantiate(soundPrefabs[soundName]);
            soundObj.name = $"{soundName}_Instance";
        }

        // Move to position and play
        soundObj.transform.SetParent(this.transform); 
        soundObj.transform.position = position;
        soundObj.SetActive(true);
        AudioSource source = soundObj.GetComponent<AudioSource>();
        source.Play();

        // Start coroutine to return to pool
        StartCoroutine(ReturnToPoolAfterPlayback(soundName, soundObj, source.clip.length));

        return soundObj;
    }

    public AudioSource GetAudioSource(string soundName)
    {
        return soundPrefabs[soundName].GetComponent<AudioSource>();
    }

    private System.Collections.IEnumerator ReturnToPoolAfterPlayback(string soundName, GameObject obj, float delay)
    {
        yield return new WaitForSeconds(delay);

        var source = obj.GetComponent<AudioSource>();
        source.spatialBlend = 1f; // Reset to default (3D)

        obj.SetActive(false);
        soundPools[soundName].Enqueue(obj);
    }
}
