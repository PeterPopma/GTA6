using UnityEngine;

public class Motorbike : MonoBehaviour
{
    public GameObject driver;

    public float maxSpeed = 200f;

    [Header("Steering Settings")]
    public float maxSteerAngle = 40f;
    public float steerSpeed = 200f;
    public Transform Steering; // Reference to the steering mechanism (handlebar)
    
    [Header("Motor / Brake")]
    public float motorForce = 5000f;
    public float brakeForce = 3000f;
    public float normalDrag = 0.1f; // Drag when not braking
    public float brakingDrag = 2f; // Drag while braking

    [Header("Bounce Settings")]
    public float bounceThreshold = -5f;   // Minimum downward velocity to trigger bounce
    public float bounceForce = 2000f;     // Upward force applied on bounce

    [Header("Deceleration Settings")]
    public float engineBrakingForce = 500f; // Force applied to slow down the car when no gas is pressed; Adjust this value to control how much the car’s speed decreases without gas input.
    public float brakeSpinStrength = 50f;  // Torque applied when braking to create a slip effect
    public float downforceCoefficient = 500f;           // Stabilizes the car

    [Header("Wheels")]
    public Transform WheelMeshF;
    public Transform WheelMeshR;
    public WheelCollider WheelColliderF;
    public WheelCollider WheelColliderR;        //  sidewaysFriction.stiffness -> how much the car drifts
    public Vector3 wheelsOrientation;

    [Header("Effects")]
    public Transform smokePosition;

    [Header("Sounds")]
    public AudioSource soundEngine;
    public AudioSource soundBrakes;

    [Header("Stabilization")]
    public float balanceForce = 300f;
    public float maxTiltAngle = 45f; // Tilt angle for leaning
    public float leanDampingFactor = 0.8f;
    public float downforceFactor = 1;
    public float maxDownforce = 500;
    public float tiltSpeed = 5f; // Speed of tilt adjustment
    public float bankingForceMultiplier = 10f; // Multiplier for banking force during turns
    public float sidewaysFrictionMultiplier = 0.8f; // Multiplier for sideways friction during turns
    public float stabilizationStrength = 2;
    public float gyroStabilizationStrength = 0.5f;
    public float stabilizationForceStrength = 10;

    new Rigidbody rigidbody;

    void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
    }
}
