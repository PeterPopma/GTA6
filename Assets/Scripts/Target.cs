using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Target : MonoBehaviour
{
    [SerializeField] private Transform vfxHit;
    private AudioSource soundSmash;
    new Rigidbody rigidbody;

    // Start is called before the first frame update
    void Start()
    {
        soundSmash = GameObject.Find("/Sound/Smash").GetComponent<AudioSource>();
        rigidbody = GetComponent<Rigidbody>();
    }

    public void Hit(Vector3 hitPosition)
    {
        soundSmash.Play();
        if (!hitPosition.Equals(Vector3.zero))
        {
            Instantiate(vfxHit, hitPosition, vfxHit.transform.rotation);
        }
        Vector3 forceDirection = (transform.position - hitPosition).normalized;
        rigidbody.AddForce(forceDirection * 70, ForceMode.VelocityChange);
        rigidbody.AddTorque(Random.insideUnitSphere * 4, ForceMode.VelocityChange);
    }
}
