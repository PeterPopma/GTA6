using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Target : MonoBehaviour
{
    [SerializeField] private Transform vfxHit;
    [SerializeField] private bool isDisplaceable;
    private List<AudioSource> soundsHit = new List<AudioSource>();
    new Rigidbody rigidbody;

    private void Awake()
    {
        Transform soundsRoot = GameObject.Find("/Sound/WallHit").transform;
        foreach (Transform item in soundsRoot)
        {
            soundsHit.Add(item.gameObject.GetComponent<AudioSource>());
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
    }

    public void Hit(Vector3 hitPosition)
    {
        soundsHit[Random.Range(0, soundsHit.Count)].Play();
        if (!hitPosition.Equals(Vector3.zero))
        {
            Instantiate(vfxHit, hitPosition, vfxHit.transform.rotation);
        }

        if (isDisplaceable && rigidbody != null)
        {
            rigidbody.constraints = RigidbodyConstraints.None;
            Vector3 forceDirection = (transform.position - hitPosition).normalized;
            rigidbody.AddForce(forceDirection * 200, ForceMode.Impulse);
            rigidbody.AddTorque(Random.insideUnitSphere * 100, ForceMode.Impulse);
        }
    }
}
