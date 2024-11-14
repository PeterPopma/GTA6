using UnityEngine;

public class Car : MonoBehaviour
{
    public float motorForce = 1500f;
    public float brakeForce = 3000f;
    public float maxSteerAngle = 30f;
    public float steerSpeed = 50f;

    public float maxSpeed = 200f;

    [Header("Suspension Settings")]
    public float suspensionDistance = 0.2f;
    public float springForce = 20000f;
    public float dampingForce = 4500f;
    public float wheelRadius = 0.35f;

    [Header("Bounce Settings")]
    public float bounceThreshold = -5f;   // Minimum downward velocity to trigger bounce
    public float bounceForce = 2000f;     // Upward force applied on bounce

    [Header("Deceleration Settings")]
    public float engineBrakingForce = 500f; // Force applied to slow down the car when no gas is pressed; Adjust this value to control how much the car’s speed decreases without gas input.
    public float idleDrag = 1f;             // Drag applied when there?s no throttle; Increase this to make the car slow down faster when there’s no input.
    public float movingDrag = 0.0f;        // Drag applied while moving; Lower this value for smoother driving, especially at higher speeds.

    Rigidbody rigidbody;

    public Transform LeftForeWheel;
    public Transform RightForeWheel;
    public Transform LeftBackWheel;
    public Transform RightBackWheel;
    public WheelCollider FrontLeftWheelCollider;
    public WheelCollider FrontRightWheelCollider;
    public WheelCollider RearLeftWheelCollider;
    public WheelCollider RearRightWheelCollider;        //  sidewaysFriction.stiffness -> how much the car drifts
    public Material MaterialLightRL;
    public Material MaterialLightRR;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
    }

    // Update is called once per frame
    void Update()
    {       
    }
}
