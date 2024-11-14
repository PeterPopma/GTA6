using UnityEngine;

public class Car : MonoBehaviour
{
    Rigidbody rigidbody;
    float speed;
    float tireSteerRotation, tireRollRotation;
    Vector3 direction;
    public Transform LeftForeWheel;
    public Transform RightForeWheel;
    public Transform LeftBackWheel;
    public Transform RightBackWheel;

    public float Speed { get => speed; set => speed = value; }
    public float TireSteerRotation { get => tireSteerRotation; set => tireSteerRotation = value; }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        direction = transform.rotation.eulerAngles.normalized;
        rigidbody = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        Speed *= 0.95f;
    }

    public void Accelerate(float amount)
    {
        rigidbody.AddForce(transform.right * 1000 * Time.deltaTime * amount, ForceMode.Acceleration);
    }

    // Update is called once per frame
    void Update()
    {
        tireRollRotation -= Speed * 50;
        //     transform.position += transform.forward * Time.deltaTime * speed;
        // first x (90 degrees), then z, then y.

        // Create the rotation quaternions
        Quaternion initialRotation = Quaternion.Euler(90, 0, 0);  // Initial 90-degree rotation on the X-axis
        Quaternion rollRotation = Quaternion.Euler(0, tireRollRotation, 0);  // axis rotation for rolling
        Quaternion steeringRotation = Quaternion.Euler(0, 0, -tireSteerRotation);  // axis rotation for steering

        // Combine rotations: initial position, then steering, then rolling
        Quaternion finalRotation = initialRotation * steeringRotation * rollRotation;

        //     Quaternion wheelRotation = Quaternion.Euler(0, 0, -tiresZRotation);
        //   wheelRotation *= Quaternion.Euler(0, 90 + tiresYRotation, 0);
        // wheelRotation *= Quaternion.Euler(90, 0, 0);
        leftForeWheel.localRotation = finalRotation;
        rightForeWheel.localRotation = finalRotation;

    }
}
