using UnityEngine;

public class Spin : MonoBehaviour
{
    [SerializeField] float speed = 100;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(new Vector3(0, Time.deltaTime * speed, 0));
    }
}
