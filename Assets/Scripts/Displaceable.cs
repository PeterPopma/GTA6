using System.Collections.Generic;
using UnityEngine;

public class Displaceable : MonoBehaviour
{
    [SerializeField] bool DestroyAfterHit;
    int layerVehicle;
    Rigidbody rigidbody;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        layerVehicle = LayerMask.NameToLayer("Vehicle");
        rigidbody = GetComponent<Rigidbody>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.gameObject.layer == layerVehicle)
        {
            rigidbody.constraints = RigidbodyConstraints.None;
            SoundManager.Instance.PlaySoundAt("Impact" + Random.Range(1, 6), transform.position);
        }
    }
}
