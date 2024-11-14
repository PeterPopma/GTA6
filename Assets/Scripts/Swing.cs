using UnityEngine;

public class Swing : MonoBehaviour
{
    [SerializeField] float offset;
    [SerializeField] float speedX;
    [SerializeField] float maxAngleX;
    [SerializeField] float speedZ;
    [SerializeField] float maxAngleZ;

    void Update()
    {
        transform.rotation = Quaternion.Euler(offset + maxAngleX * Mathf.Cos(speedX * Time.time), 0, maxAngleZ * Mathf.Sin(speedZ * Time.time));
    }
}
