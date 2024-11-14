using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class CarController : MonoBehaviour
{
    [Header("Cinemachine")]
    [Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
    public GameObject CinemachineCameraTarget;
    
    [SerializeField] TextMeshProUGUI textSpeed;
    [SerializeField] Material materialRearLight;
    public float skidSideForce = 50f;  // Side force applied when braking to create a slip effect
    public float slipFrictionMultiplier = 0.5f;  // Multiplier to reduce sideways friction when braking

    private Car car;
    private Player player;
    private const float _threshold = 0.01f;
    private AudioSource soundBrake;


    public Car Car { get => car; set => car = value; }

    // cinemachine
    private float cinemachineTargetYaw;
    private float cinemachineTargetPitch;
    private PlayerInput _playerInput;

    private bool isAccelerating;

    // WheelColliders
    private WheelCollider frontLeftWheelCollider;
    private WheelCollider frontRightWheelCollider;
    private WheelCollider rearLeftWheelCollider;
    private WheelCollider rearRightWheelCollider;

    // Visual Wheel Transforms
    private Transform frontLeftWheelTransform;
    private Transform frontRightWheelTransform;
    private Transform rearLeftWheelTransform;
    private Transform rearRightWheelTransform;

    private float originalSidewaysStiffness;

    private float currentSteerAngle = 0f; // Current angle of the steering

    private bool isBraking;
    private bool isAirborne = false;      // Track if the car is airborne

    private Rigidbody rigidbody;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        textSpeed.text = "";
        player = GetComponent<Player>();
        _playerInput = GetComponent<PlayerInput>();
        soundBrake = GameObject.Find("/Sound/Brake").GetComponent<AudioSource>();
    }
    private bool IsCurrentDeviceMouse
    {
        get
        {
            return _playerInput.currentControlScheme == "KeyboardMouse";
        }
    }

    public bool IsBraking { get => isBraking; set => isBraking = value; }

    public void SetCar(Car car)
    {
        this.car = car;
        rigidbody = car.gameObject.GetComponent<Rigidbody>();
        rigidbody.centerOfMass = new Vector3(0, -0.5f, 0);
        frontLeftWheelTransform = car.LeftForeWheel;
        frontRightWheelTransform = car.RightForeWheel;
        rearLeftWheelTransform = car.LeftBackWheel;
        rearRightWheelTransform = car.RightBackWheel;
        frontLeftWheelCollider = car.FrontLeftWheelCollider;
        frontRightWheelCollider = car.FrontRightWheelCollider;
        rearLeftWheelCollider = car.RearLeftWheelCollider;
        rearRightWheelCollider = car.RearRightWheelCollider;
        // Configure suspension for each WheelCollider
        SetupWheelColliders(frontLeftWheelCollider);
        SetupWheelColliders(frontRightWheelCollider);
        SetupWheelColliders(rearLeftWheelCollider);
        SetupWheelColliders(rearRightWheelCollider);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (car == null)
        {
            return;
        }

        HandleMotor();
        HandleSteering();
        ApplyBrakes();
        UpdateVisualWheels();
        CheckLanding();
        ApplyDeceleration(); 
        LimitMaxSpeed();
        UpdateGUI();
        player.transform.position = car.transform.position;
    }

    private void UpdateGUI()
    {
        textSpeed.text = rigidbody.linearVelocity.magnitude.ToString("0") + " Km/h";
    }

    private void ApplySkidEffect()
    {
        // Reduce sideways friction stiffness to make wheels slide more
        WheelFrictionCurve frictionCurve = frontLeftWheelCollider.sidewaysFriction;
        frictionCurve.stiffness = originalSidewaysStiffness * slipFrictionMultiplier;

        frontLeftWheelCollider.sidewaysFriction = frictionCurve;
        frontRightWheelCollider.sidewaysFriction = frictionCurve;
        rearLeftWheelCollider.sidewaysFriction = frictionCurve;
        rearRightWheelCollider.sidewaysFriction = frictionCurve;
    }
    private bool IsMovingForward()
    {
        // Get the velocity of the car
        Vector3 velocity = rigidbody.linearVelocity;

        // Calculate the dot product of the velocity and the forward direction
        float dot = Vector3.Dot(transform.forward, velocity);

        // If the dot product is positive, the car is moving forward;
        // if it's negative, the car is moving backward.
        return dot > 0;
    }

    private void LimitMaxSpeed()
    {
        if (rigidbody.linearVelocity.magnitude > car.maxSpeed)
        {
            rigidbody.linearVelocity = rigidbody.linearVelocity.normalized * car.maxSpeed;
        }
    }

    private void OnBrake(InputValue value)
    {
        soundBrake.Play();
        isBraking = value.isPressed;
        if (!isBraking)
        {
            ResetBraking();
        }
    }

    private void ResetBraking()
    {
        // Release brakes
        frontLeftWheelCollider.brakeTorque = 0;
        frontRightWheelCollider.brakeTorque = 0;
        rearLeftWheelCollider.brakeTorque = 0;
        rearRightWheelCollider.brakeTorque = 0;

        // Restore original sideways friction
        RestoreOriginalFriction();
    }


    private void ApplyDeceleration()
    {
        if (!isAccelerating)  // No gas input
        {
            rigidbody.linearDamping = car.idleDrag;  // Increase drag to slow down

            // Optionally, apply a small brake force to slow down the wheels
            rearLeftWheelCollider.brakeTorque = car.engineBrakingForce;
            rearRightWheelCollider.brakeTorque = car.engineBrakingForce;
        }
        else
        {
            // Release any braking force when accelerating
            rearLeftWheelCollider.brakeTorque = 0;
            rearRightWheelCollider.brakeTorque = 0;
        }
    }

    private void ApplyBrakes()
    {
        if (!isBraking && player.move.y < 0 && IsMovingForward())
        {
            isBraking = true;
            soundBrake.Play();
            materialRearLight.SetFloat("_EmissiveExposureWeight", 0.3f);
        }

        if (isBraking)
        {
            if (player.move.y >= 0 || !IsMovingForward())
            {
                isBraking = false;
                materialRearLight.SetFloat("_EmissiveExposureWeight", 0.9f);
                ResetBraking();
            }
            
            // Apply brake torque to all wheels
            frontLeftWheelCollider.brakeTorque = car.brakeForce;
            frontRightWheelCollider.brakeTorque = car.brakeForce;
            rearLeftWheelCollider.brakeTorque = car.brakeForce;
            rearRightWheelCollider.brakeTorque = car.brakeForce;

            // Reduce sideways friction to simulate slipping
            ApplySkidEffect();

            // Apply random sideways force to make the car slip
            float randomSideForce = Random.Range(-skidSideForce, skidSideForce);
            rigidbody.AddForce(transform.right * randomSideForce, ForceMode.Acceleration);
        }
    }

    private void RestoreOriginalFriction()
    {
        // Restore the original friction stiffness after braking
        WheelFrictionCurve frictionCurve = frontLeftWheelCollider.sidewaysFriction;
        frictionCurve.stiffness = originalSidewaysStiffness;

        frontLeftWheelCollider.sidewaysFriction = frictionCurve;
        frontRightWheelCollider.sidewaysFriction = frictionCurve;
        rearLeftWheelCollider.sidewaysFriction = frictionCurve;
        rearRightWheelCollider.sidewaysFriction = frictionCurve;
    }

    private void SetupWheelColliders(WheelCollider wheelCollider)
    {
        wheelCollider.suspensionDistance = car.suspensionDistance;
        JointSpring suspensionSpring = wheelCollider.suspensionSpring;
        suspensionSpring.spring = car.springForce;
        suspensionSpring.damper = car.dampingForce;
        wheelCollider.suspensionSpring = suspensionSpring;
        wheelCollider.radius = car.wheelRadius;
    }

    private void HandleMotor()
    {
        if (player.move.y != 0)
        {
            isAccelerating = true;
            rigidbody.linearDamping = car.movingDrag;  // Lower drag when accelerating
        }
        else
        {
            isAccelerating = false;
        }

        // Apply motor torque to the rear wheels
        rearLeftWheelCollider.motorTorque = player.move.y * car.motorForce;
        rearRightWheelCollider.motorTorque = player.move.y * car.motorForce;

    }

    private void HandleSteering()
    {
        // Adjust the steer angle based on input direction
        if (player.move.x != 0)
        {
            currentSteerAngle += player.move.x * car.steerSpeed * Time.fixedDeltaTime;
            currentSteerAngle = Mathf.Clamp(currentSteerAngle, -car.maxSteerAngle, car.maxSteerAngle);
        }
        else
        {
            // gradually return the wheels to center when there's no input
            currentSteerAngle = Mathf.MoveTowards(currentSteerAngle, 0, car.steerSpeed * Time.fixedDeltaTime);
        }

        frontLeftWheelCollider.steerAngle = currentSteerAngle;
        frontRightWheelCollider.steerAngle = currentSteerAngle;
    }

    private void UpdateVisualWheels()
    {
        UpdateWheelPose(frontLeftWheelCollider, frontLeftWheelTransform, true);
        UpdateWheelPose(frontRightWheelCollider, frontRightWheelTransform, true);
        UpdateWheelPose(rearLeftWheelCollider, rearLeftWheelTransform, false);
        UpdateWheelPose(rearRightWheelCollider, rearRightWheelTransform, false);
    }

    private void UpdateWheelPose(WheelCollider collider, Transform wheelTransform, bool isFrontWheel)
    {
        Vector3 pos;
        Quaternion rot;
        collider.GetWorldPose(out pos, out rot);

        // Update position
        wheelTransform.position = pos;

        // Update rotation for the wheel based on car's speed
        float wheelRotationAngle = player.move.y * car.motorForce * Time.deltaTime / car.wheelRadius;
        wheelTransform.Rotate(wheelRotationAngle, 0, 0, Space.Self);

        // Apply steering angle to front wheels only
        if (isFrontWheel)
        {
            Quaternion steeringRotation = Quaternion.Euler(0, collider.steerAngle, 0);
            wheelTransform.rotation = rot * Quaternion.Euler(0, 0, 90) * steeringRotation;
        }
        else
        {    
            // Correct rotation by aligning to the wheel collider's rotation
            wheelTransform.rotation = rot * Quaternion.Euler(0, 0, 90);
        }
    }

    private void CheckLanding()
    {
        bool allWheelsGrounded = frontLeftWheelCollider.isGrounded && frontRightWheelCollider.isGrounded
            && rearLeftWheelCollider.isGrounded && rearRightWheelCollider.isGrounded;

        if (isAirborne && allWheelsGrounded)
        {
            // Calculate the car's vertical velocity on landing
            float verticalVelocity = rigidbody.linearVelocity.y;

            if (verticalVelocity < car.bounceThreshold)
            {
                // Apply bounce force proportional to the downward velocity on landing
                rigidbody.AddForce(Vector3.up * car.bounceForce * Mathf.Abs(verticalVelocity), ForceMode.Impulse);
            }

            isAirborne = false; // Reset airborne state
        }
        else if (!allWheelsGrounded)
        {
            isAirborne = true; // Set airborne state if not all wheels are grounded
        }
    }

    private void LateUpdate()
    {
        CameraRotation();
    }

    private void CameraRotation()
    {
        // if there is an input and camera position is not fixed
        if (player.look.sqrMagnitude >= _threshold)
        {
            // Don't multiply mouse input by Time.deltaTime;
            float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;

            cinemachineTargetYaw += player.look.x * deltaTimeMultiplier;
            cinemachineTargetPitch += player.look.y * deltaTimeMultiplier;
        }

        // Cinemachine will follow this target
        CinemachineCameraTarget.transform.rotation = Quaternion.Euler(cinemachineTargetPitch, cinemachineTargetYaw, 0.0f);
    }
}
