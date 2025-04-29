using UnityEngine;

public class NPCAirplane : MonoBehaviour
{
    public float speed = 5.0f;
    private Vector3 startPosition;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        startPosition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position += transform.forward * speed * Time.deltaTime;
        if(transform.position.x > 3500)
        {
            transform.position = startPosition;
        }
    }
}
