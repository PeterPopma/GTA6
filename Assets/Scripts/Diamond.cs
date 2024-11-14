using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Diamond : MonoBehaviour
{
    [SerializeField] Transform vfxHit;
    private AudioSource soundDiamond;

    private void Start()
    {
        soundDiamond = GameObject.Find("/Sound/Diamond").GetComponent<AudioSource>();
    }

    void Update()
    {
        transform.Rotate(new Vector3(0, 0, 100 * Time.deltaTime), Space.Self);
    }

    public void OnTriggerEnter()
    {
        Transform newEffect = Instantiate(vfxHit, transform.position, vfxHit.transform.rotation);
        newEffect.parent = Game.Instance.EffectsParent.transform; 
        soundDiamond.Play();
        Game.Instance.DecreaseDiamonds();
        Destroy(gameObject);
    }
}
