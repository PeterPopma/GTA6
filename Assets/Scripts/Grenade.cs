using UnityEngine;

public class Grenade : MonoBehaviour
{
    [SerializeField] float angularVelocity = 10000.0f;
    [SerializeField] float lifeTime = 3.4f;
    [SerializeField] private Transform vfxExplosion;
    private AudioSource soundGrenadeBounce;
    private AudioSource soundGrenadeExplosion;
    private Vector3 axisOfRotation;
    private Rigidbody myRigidbody;
    private Player player;
    private CameraShake cameraShake;

    public void Setup(Player player)
    {
        this.player = player;
    }

    // Start is called before the first frame update
    void Start()
    {
        angularVelocity = 1000.0f;
        myRigidbody = GetComponent<Rigidbody>();
        float speed = 20f;
        Vector3 velocity = transform.forward * speed;
//        velocity.y = 10f;
        myRigidbody.linearVelocity = velocity;
        //axisOfRotation = Random.onUnitSphere;
        axisOfRotation = new Vector3(1, 0.2f, 0.2f);
        soundGrenadeExplosion = GameObject.Find("/Sound/GrenadeExplosion").GetComponent<AudioSource>();
        soundGrenadeBounce = GameObject.Find("/Sound/GrenadeBounce").GetComponent<AudioSource>();
        cameraShake = GameObject.Find("FollowCamera").GetComponent<CameraShake>();
    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(axisOfRotation, angularVelocity * Time.smoothDeltaTime);

        lifeTime -= Time.deltaTime;
        if (lifeTime < 0f)
        {
            bool somethingHit = false;
            Collider[] colliders = Physics.OverlapSphere(transform.position, 5.0f);
            foreach (Collider collider in colliders)
            {
                if (collider.gameObject.GetComponent<NPC>() != null)
                {
                    collider.gameObject.GetComponent<NPC>().BlastImpact(gameObject);
                }
                else
                {
                    Rigidbody rigidBody = collider.gameObject.GetComponent<Rigidbody>();
                    if (rigidBody != null)
                    {
                        Vector3 forceDirection = (collider.gameObject.transform.position - transform.position).normalized;
                        forceDirection = new Vector3(forceDirection.x, 12, forceDirection.z) * 1.0f;
                        rigidBody.constraints = RigidbodyConstraints.None;
                        rigidBody.AddForce(forceDirection, ForceMode.VelocityChange);
                        rigidBody.AddTorque(Random.insideUnitSphere * 20, ForceMode.VelocityChange);
                    }
                }
                if (collider.gameObject.GetComponent<SelfDestruct>() != null)
                {
                    collider.gameObject.GetComponent<SelfDestruct>().DestroyAfterDelay();
                }
            }

            soundGrenadeExplosion.Play();
            cameraShake.ShakeCamera(10, 12f);
            Instantiate(vfxExplosion, transform.position, Quaternion.identity);
            Destroy(this.gameObject);
        }
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<Player>()==null)
        {
            soundGrenadeBounce.Play();
        }
    }

}
