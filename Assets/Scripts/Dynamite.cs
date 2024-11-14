using Unity.Cinemachine;
using UnityEngine;

public class Dynamite : MonoBehaviour
{
    [SerializeField] float lifeTime = 3f;
    [SerializeField] private Transform vfxExplosion;
    private AudioSource soundBounce;
    private AudioSource soundExplosion;
    private Vector3 axisOfRotation;
    private Rigidbody myRigidbody;

    void Start()
    {
        myRigidbody = GetComponent<Rigidbody>();
        float speed = 20f;
        Vector3 velocity = transform.forward * speed;
        velocity.y = 10f;
        myRigidbody.linearVelocity = velocity;
        //axisOfRotation = Random.onUnitSphere;
        axisOfRotation = new Vector3(1, 0.2f, 0.2f);
        soundExplosion = GameObject.Find("/Sound/Explosion").GetComponent<AudioSource>();
        soundBounce = GameObject.Find("/Sound/Bounce").GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(axisOfRotation, 1000.0f * Time.smoothDeltaTime);

        lifeTime -= Time.deltaTime;
        if (lifeTime < 0)
        {
            Vector3 explosionPos = transform.position;
            Collider[] colliders = Physics.OverlapSphere(explosionPos, 20.0f);
            bool containsExplodable = false;

            foreach (Collider collider in colliders)
            {
                if (collider.gameObject.GetComponent<Explodable>() != null)
                {
                    bool cancelExplosion = false;

                    if (collider.gameObject.transform.name == "ExplosionTriggerSaloon")
                    {
                        if (Game.Instance.ActiveMission != null && Game.Instance.ActiveMission.Name == "bank")
                        {
                            Game.Instance.DisplayMission("Maybe that was not such a good idea.. Thanks anyway, good job!", 3);
                            Game.Instance.ActiveMission.CompleteMission(12);
                        }
                        else
                        {
                            // don't blow up the saloon yet!
                            cancelExplosion = true;
                        }
                    }
                    if (collider.gameObject.transform.name == "ExplosionTriggerFarm")
                    {
                        cancelExplosion = true;
                    }

                    if (!cancelExplosion)
                    {
                        collider.gameObject.GetComponent<Explodable>().Explode();
                    }
                    containsExplodable = true;
                }
            }
            // if we blow up something, the player does not get affected by the blast.
            // this is used to blow up the safe in the saloon and makes this second iteration necessary.
            foreach (Collider collider in colliders)
            {
                if (!containsExplodable && collider.gameObject.GetComponent<Player>() != null)
                {
                    collider.gameObject.GetComponent<Player>().Explode();
                }
                if (collider.gameObject.GetComponent<NPC>() != null)
                {
                    collider.gameObject.GetComponent<NPC>().Explode();
                }
            }
            soundExplosion.Play(); 
            
            CinemachineCamera vcamPlayer = (CinemachineCamera)CinemachineBrain.GetActiveBrain(0).ActiveVirtualCamera;
            if (vcamPlayer.GetComponent<CameraShake>() != null)
            {
                vcamPlayer.GetComponent<CameraShake>().ShakeCamera(10, 12f);
            }
            Instantiate(vfxExplosion, transform.position, Quaternion.identity);
            Destroy(gameObject);
        }
    }

    public void OnTriggerEnter(Collider other)
    {
        soundBounce.Play();
    }

}
