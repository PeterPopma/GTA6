using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shell : MonoBehaviour
{
    public float angularVelocity = 100.0f;
    private Rigidbody myRigidbody;
    private Vector3 axisOfRotation;

    private void Awake()
    {
        myRigidbody = GetComponent<Rigidbody>();
    }

    void Start()
    {
        Vector3 velocity = new Vector3(Random.value * 2f, 6f, Random.value * 2f);
        myRigidbody.AddForce(velocity, ForceMode.VelocityChange);
        myRigidbody.AddTorque(new Vector3(Random.value * 20f, Random.value * 20f, Random.value * 20f), ForceMode.VelocityChange);
    }

    void Update()
    {
    }
    void OnCollisionEnter(Collision collision)
    {
        // Check if hit object is terrain (by tag or other method)
        if (collision.collider.CompareTag("Terrain"))
        {
            // Optional: check for minimum impact force
            if (collision.relativeVelocity.magnitude > 1.0f)
            {
                SoundManager.Instance.PlaySoundAt("Shell", transform.position);
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.tag.Equals("Player"))
        {
            SoundManager.Instance.PlaySoundAt("Shell", transform.position);
            Destroy(gameObject);
        }
    }
}
