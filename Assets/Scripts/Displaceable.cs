using System.Collections.Generic;
using UnityEngine;

public class Displaceable : MonoBehaviour
{
    [SerializeField] bool DestroyAfterHit;
    int layerVehicle;
    Rigidbody rigidbody;
    List<AudioClip> clipsCrash = new List<AudioClip>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        layerVehicle = LayerMask.NameToLayer("Vehicle");
        rigidbody = GetComponent<Rigidbody>();
        if (transform.Find("ImpactSound"))
        {
            clipsCrash.Add(transform.Find("ImpactSound").GetComponent<AudioSource>().clip);
        }
        else
        {
            Transform ImpactSoundsRoot = GameObject.Find("/Sound/Impacts").transform;
            foreach (Transform item in ImpactSoundsRoot)
            {
                AudioClip clip = item.gameObject.GetComponent<AudioSource>().clip;
                clipsCrash.Add(clip);
            }
        }

    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.gameObject.layer == layerVehicle)
        {
            rigidbody.constraints = RigidbodyConstraints.None;
            AudioSource.PlayClipAtPoint(clipsCrash[Random.Range(0, clipsCrash.Count)], transform.position);
        }
    }
}
