using UnityEngine;

/// <summary>
/// This script must be added to a child that has a collider that is is range of the dynamite blast.
/// It will then make the whole object tear apart by adding force to all child objects.
/// </summary>
public class Explodable : MonoBehaviour
{
    public void Explode(bool preserveParts = false)
    {
        foreach (Transform child in transform.parent)
        {
            Rigidbody newRigidBody = child.gameObject.AddComponent<Rigidbody>();
            if (preserveParts)
            {
                child.gameObject.AddComponent<BoxCollider>();
            }
            newRigidBody.AddForce(new Vector3(Random.value * 8, 10, Random.value * 8), ForceMode.VelocityChange);
            newRigidBody.AddTorque(Random.insideUnitSphere * 5, ForceMode.VelocityChange);
        }
    }
}
