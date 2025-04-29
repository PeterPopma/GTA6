using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Target : MonoBehaviour
{
    [SerializeField] private Transform vfxHit;
    [SerializeField] private bool isDisplaceable;
    new Rigidbody rigidbody;

    // Start is called before the first frame update
    void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
    }

    public void Hit(Vector3 hitPosition)
    {
        SoundManager.Instance.PlaySoundAt("WallHit" + Random.Range(1, 3), transform.position);
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
