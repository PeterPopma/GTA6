using UnityEngine;

public class MedKit : MonoBehaviour
{
    AudioSource soundMedKit;
    
    private void Awake()
    {
        soundMedKit = GameObject.Find("/Sound/MedKit").GetComponent<AudioSource>();
    }

    void FixedUpdate()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, 2);

        foreach (var collider in colliders)
        {
            if (collider.GetComponent<Player>() != null)
            {
                soundMedKit.Play();
                collider.GetComponent<Player>().IncreaseHealth(40);
                Destroy(gameObject);
            }
        }
    }
}
