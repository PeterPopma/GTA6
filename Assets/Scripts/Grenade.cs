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
            Vector3 explosionPos = transform.position;
            Collider[] colliders = Physics.OverlapSphere(explosionPos, 15.0f);

            bool somethingHit = false;
            foreach (Collider collider in colliders)
            {
                if (collider.gameObject.GetComponent<Target>() != null)
                {
                    somethingHit = true;
                    // TODO..
                }
            }
            if (somethingHit)
            {
                //player.ShotsHit++;
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
