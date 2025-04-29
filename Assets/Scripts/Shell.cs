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
        var velocity = transform.right * ((Random.value / 1f) + 0.5f);
        velocity = new Vector3(velocity.x, velocity.y + 6f, velocity.z);
        axisOfRotation = Random.onUnitSphere;
        myRigidbody.linearVelocity = velocity;
    }

    void Update()
    {
        transform.Rotate(axisOfRotation, angularVelocity * Time.smoothDeltaTime);
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
