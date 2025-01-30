using UnityEngine;

public class Rocket : MonoBehaviour
{
    [SerializeField] private Transform vfxHit;
    [SerializeField] private Transform vfxSmoke;
    [SerializeField] private Transform vfxFire;
    private AudioSource soundRocketExplosion;
    private Rigidbody myRigidbody;
    private float timeLastSmoke;

    private void Awake()
    {
        myRigidbody = GetComponent<Rigidbody>();
        soundRocketExplosion = GameObject.Find("/Sound/RocketExplosion").GetComponent<AudioSource>();
    }

    void Start()
    {
        Vector3 direction = transform.forward;
        direction = Quaternion.Euler(5, 0, 0) * direction;
        myRigidbody.linearVelocity = direction * 80f;
//        transform.Rotate(Vector3.left, 90);
    }

    private void Update()
    {
        if (Time.time >= timeLastSmoke + 0.04f)
        {
            timeLastSmoke = Time.time;
            Instantiate(vfxSmoke, transform.position, Quaternion.identity);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        Vector3 explosionPos = transform.position;
        Collider[] colliders = Physics.OverlapSphere(explosionPos, 5.0f);       // todo: add layer mask to exclude bodyparts layer

        bool somethingHit = false;
        foreach (Collider collider in colliders)
        {
            if (collider.gameObject.GetComponent<NPC>()!=null)
            {
                collider.gameObject.GetComponent<NPC>().BlastImpact(gameObject);
            }
            else
            {
                Rigidbody rigidBody = collider.gameObject.GetComponent<Rigidbody>();
                if (rigidBody != null)
                {
                    if (collider.gameObject.GetComponent<NPC>() == null && Random.value<0.5f)
                    {
                        Transform newfire = Instantiate(vfxFire, collider.gameObject.transform.position, Quaternion.identity);
                        Transform firePosition = collider.gameObject.transform.Find("FirePosition");
                        if (firePosition != null)
                        {
                            newfire.position = firePosition.position;
                        }
                        newfire.parent = collider.gameObject.transform;
                    }
                    Vector3 forceDirection = (collider.gameObject.transform.position - transform.position).normalized;
                    forceDirection = new Vector3(forceDirection.x, 12, forceDirection.z) * 1.0f;
                    rigidBody.constraints = RigidbodyConstraints.None;
                    rigidBody.AddForce(forceDirection, ForceMode.VelocityChange);
                    rigidBody.AddTorque(Random.insideUnitSphere * 20, ForceMode.VelocityChange);
                    if (collider.gameObject.GetComponent<SelfDestruct>() != null)
                    {
                        collider.gameObject.GetComponent<SelfDestruct>().DestroyAfterDelay();
                    }
                }
            }
        }
        soundRocketExplosion.Play();
        Instantiate(vfxHit, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }
}
