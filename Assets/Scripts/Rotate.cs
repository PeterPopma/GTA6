using UnityEngine;

public class Rotate : MonoBehaviour
{
    private void Start()
    {
        transform.rotation = Quaternion.Euler(0,0,Random.value * 360);
    }

    void Update()
    {
        transform.Rotate(0, 0, Time.deltaTime * 15);
    }
}
