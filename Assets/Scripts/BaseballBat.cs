using UnityEngine;

public class BaseballBat : MonoBehaviour
{
    const int MAX_LIFE_TIME = 4;
    float lifeTime = 0;
    private AudioSource soundBaseballBat;
    private AudioSource soundBaseballBatHit;
    private Rigidbody rigidbody;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        soundBaseballBat = GetComponent<AudioSource>();
        soundBaseballBat.Play();
        soundBaseballBatHit = GameObject.Find("/Sound/BaseballBatHit").GetComponent<AudioSource>();
        rigidbody = GetComponent<Rigidbody>();
        rigidbody.AddForce(transform.forward*50, ForceMode.VelocityChange);
        rigidbody.AddTorque(new Vector3(10, 0, 0), ForceMode.VelocityChange);
    }

    // Update is called once per frame
    void Update()
    {
        lifeTime += Time.deltaTime;
        if (lifeTime > MAX_LIFE_TIME)
        {
            soundBaseballBat.Stop();
            Destroy(gameObject);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        soundBaseballBatHit.Play();
        if (collision.gameObject.GetComponent<NPC>()!=null)
        {
            collision.gameObject.GetComponent<NPC>().Hit(transform.position); 
            soundBaseballBat.Stop();
            Destroy(gameObject);
        }
    }
}
