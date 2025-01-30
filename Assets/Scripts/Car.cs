using UnityEngine;

public class Car : MonoBehaviour
{
    public GameObject driver;

    public bool fourWheelDrive;
    public bool useTractionControl;

    public float motorForce = 5000f;
    public float brakeForce = 3000f;
    public float maxSteerAngle = 40f;
    public float steerSpeed = 200f;

    public float maxSpeed = 200f;

    [Header("Suspension Settings")]
    public float suspensionDistance = 0.2f;
    public float springForce = 40000f;
    public float dampingForce = 4500f;

    [Header("Bounce Settings")]
    public float bounceThreshold = -5f;   // Minimum downward velocity to trigger bounce
    public float bounceForce = 2000f;     // Upward force applied on bounce

    [Header("Deceleration Settings")]
    public float engineBrakingForce = 500f; // Force applied to slow down the car when no gas is pressed; Adjust this value to control how much the car’s speed decreases without gas input.
    public float idleDrag = 1f;             // Drag applied when there?s no throttle; Increase this to make the car slow down faster when there’s no input.
    public float movingDrag = 0.0f;        // Drag applied while moving; Lower this value for smoother driving, especially at higher speeds.

    public float brakeSpinStrength = 50f;  // Torque applied when braking to create a slip effect
    public float downforceCoefficient = 500f;           // Stabilizes the car

    public Vector3 wheelsOrientation;

    new Rigidbody rigidbody;

    public Transform WheelMeshFL;
    public Transform WheelMeshFR;
    public Transform WheelMeshRL;
    public Transform WheelMeshRR;
    public WheelCollider WheelColliderFL;
    public WheelCollider WheelColliderFR;
    public WheelCollider WheelColliderRL;
    public WheelCollider WheelColliderRR;        //  sidewaysFriction.stiffness -> how much the car drifts

    public Transform smokePosition;

    public AudioSource soundEngine;
    public AudioSource soundBrakes;

    void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
    }
}
