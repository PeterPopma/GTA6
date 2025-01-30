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

    private Car car;
    private Player player;
    private const float _threshold = 0.01f;
    private float timeleftSmoke;

    private const float TIME_BETWEEN_SMOKE = 1.5f;

    public Car Car { get => car; set => car = value; }

    // cinemachine
    private float cinemachineTargetYaw;
    private float cinemachineTargetPitch;
    private PlayerInput _playerInput;

    private bool isAccelerating;

    // WheelColliders
    private WheelCollider WheelColliderFL;
    private WheelCollider WheelColliderFR;
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

    new private Rigidbody rigidbody;

    private float speed;
    private float currentEngineVolume = 0.3f;

    public Color gizmoColor = Color.red; // Color of the marker
    public float gizmoSize = 0.1f; // Size of the marker

    [SerializeField] private Transform vfxSmoke;

    void Awake()
    {
        textSpeed.text = "";
        player = GetComponent<Player>();
        _playerInput = GetComponent<PlayerInput>();
    }

    private bool IsCurrentDeviceMouse
    {
        get
        {
            return _playerInput.currentControlScheme == "KeyboardMouse";
        }
    }

    public bool IsBraking { get => isBraking; set => isBraking = value; }
    public float Speed { get => speed; set => speed = value; }

    public void SetCar(Car car)
    {
        this.car = car;
        if (car.driver!=null)
        {
            car.driver.SetActive(true);
        }
        rigidbody = car.gameObject.GetComponent<Rigidbody>();
        frontLeftWheelTransform = car.WheelMeshFL;
        frontRightWheelTransform = car.WheelMeshFR;
        rearLeftWheelTransform = car.WheelMeshRL;
        rearRightWheelTransform = car.WheelMeshRR;
        WheelColliderFL = car.WheelColliderFL;
        WheelColliderFR = car.WheelColliderFR;
        rearLeftWheelCollider = car.WheelColliderRL;
        rearRightWheelCollider = car.WheelColliderRR;
        // Configure suspension for each WheelCollider
        SetupWheelColliders(WheelColliderFL);
        SetupWheelColliders(WheelColliderFR);
        SetupWheelColliders(rearLeftWheelCollider);
        SetupWheelColliders(rearRightWheelCollider);
        originalSidewaysStiffness = WheelColliderFL.sidewaysFriction.stiffness;
        if (car.soundEngine != null)
        {
            car.soundEngine.Play();
        }
        textSpeed.enabled = true;
    }

    public void ExitCar()
    {
        if (car.driver != null)
        {
            car.driver.SetActive(false);
        }
        car = null;
    }

    void FixedUpdate()
    {
        if (car == null)
        {
            return;
        }

        HandleMotor();
        ApplyDownforce();
        LimitMaxSpeed();
        ShowSmoke();
        UpdateGUI();
        UpdateEngineSound();
        HandleSteering();
        ApplyBrakes();
        UpdateVisualWheels();
        CheckLanding();
        ApplyDeceleration();
        if (car.useTractionControl)
        {
            TractionControl();
        }

        speed = rigidbody.linearVelocity.magnitude; 
    }

    void ApplySpinTorque()
    {
        Vector3 spinTorque = transform.up * car.brakeSpinStrength * speed;
        rigidbody.AddTorque(spinTorque, ForceMode.Impulse);
    }

    void TractionControl()
    {
        foreach (var wheel in new[] { WheelColliderFL, WheelColliderFR, rearLeftWheelCollider, rearRightWheelCollider })
        {
            WheelHit wheelHit;
            if (wheel.GetGroundHit(out wheelHit))
            {
                if (wheelHit.forwardSlip > 0.2f) // Limit excessive slip
                {
                    wheel.motorTorque *= 0.9f;
                }
                if (wheelHit.sidewaysSlip > 0.3f) // Adjust slip threshold
                {
                    WheelFrictionCurve friction = wheel.sidewaysFriction;
                    friction.stiffness *= 0.9f; // Reduce stiffness to prevent sliding
                    wheel.sidewaysFriction = friction;
                }
            }
        }
    }

    private void Update()
    {
        player.transform.position = car.transform.position;
    }

    private void UpdateEngineSound()
    {
        if (car.soundEngine == null)
        {
            return;
        }

        if (player.move.y != 0)
        {
            if (currentEngineVolume < 1)
            {
                currentEngineVolume += 0.05f;
            }
        }
        else if (currentEngineVolume > 0)
        {
            currentEngineVolume -= 0.03f; 
            if (currentEngineVolume < 0.3f)
            {
                currentEngineVolume = 0.3f;
            }
        }

        car.soundEngine.volume = currentEngineVolume;

        float pitch = 0.8f + speed / car.maxSpeed;
        car.soundEngine.pitch = pitch;
    }

    private void UpdateGUI()
    {
        textSpeed.text = speed.ToString("0") + " Km/h";
    }
    private void ApplyDownforce()
    {
        rigidbody.AddForce(-transform.up * car.downforceCoefficient * rigidbody.linearVelocity.magnitude);
    }

    private bool IsMovingForward()
    {
        // Get the velocity of the car
        Vector3 velocity = rigidbody.linearVelocity;

        // Calculate the dot product of the velocity and the forward direction
        float dot = Vector3.Dot(car.transform.forward, velocity);

        // If the dot product is positive, the car is moving forward;
        // if it's negative, the car is moving backward.
        return dot > 0.01f;
    }

    private void LimitMaxSpeed()
    {
        if (rigidbody.linearVelocity.magnitude > car.maxSpeed)
        {
            rigidbody.linearVelocity = rigidbody.linearVelocity.normalized * car.maxSpeed;
        }
    }

    private void ResetBraking()
    {
        // Release brakes
        WheelColliderFL.brakeTorque = 0;
        WheelColliderFR.brakeTorque = 0;
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
            currentEngineVolume = 0;
            if(car.soundBrakes != null)
            {
                car.soundBrakes.Play();
            }
            materialRearLight.SetFloat("_EmissiveExposureWeight", 0.3f);
        }

        if (isBraking)
        {                       
            // Apply brake torque to all wheels
            WheelColliderFL.brakeTorque = car.brakeForce;
            WheelColliderFR.brakeTorque = car.brakeForce;
            rearLeftWheelCollider.brakeTorque = car.brakeForce;
            rearRightWheelCollider.brakeTorque = car.brakeForce;

            ApplySpinTorque();
            
            if (player.move.y >= 0 || !IsMovingForward())
            {
                isBraking = false;
                materialRearLight.SetFloat("_EmissiveExposureWeight", 0.9f);
                ResetBraking();
            }
            
        }
    }

    private void RestoreOriginalFriction()
    {
        // Restore the original friction stiffness after braking
        WheelFrictionCurve frictionCurve = WheelColliderFL.sidewaysFriction;
        frictionCurve.stiffness = originalSidewaysStiffness;

        WheelColliderFL.sidewaysFriction = frictionCurve;
        WheelColliderFR.sidewaysFriction = frictionCurve;
        rearLeftWheelCollider.sidewaysFriction = frictionCurve;
        rearRightWheelCollider.sidewaysFriction = frictionCurve;
    }

    private void SetupWheelColliders(WheelCollider wheelCollider)
    {
        wheelCollider.suspensionDistance = car.suspensionDistance;
        JointSpring suspensionSpring = wheelCollider.suspensionSpring;
        suspensionSpring.spring = car.springForce;
        suspensionSpring.damper = car.dampingForce;
        suspensionSpring.targetPosition = 0.5f;  // Midpoint of the suspension travel
        wheelCollider.suspensionSpring = suspensionSpring;
        wheelCollider.mass = 20f;  // Ensure wheel mass is proportional to car mass
        wheelCollider.wheelDampingRate = 1f;
    }

    private void OnDrawGizmos()
    {
        if (rigidbody != null)
        {
            // Set the Gizmo color
            Gizmos.color = gizmoColor;

            // Calculate the world position of the center of mass
            Vector3 comPosition = rigidbody.worldCenterOfMass;

            // Draw a sphere at the center of mass position
            Gizmos.DrawSphere(comPosition, gizmoSize);
        }
    }

    private void ShowSmoke()
    {
        if (car.smokePosition != null)
        {
            timeleftSmoke -= Time.deltaTime * speed;
            if (timeleftSmoke < 0)
            {
                timeleftSmoke = TIME_BETWEEN_SMOKE;
                Transform newEffect = Instantiate(vfxSmoke, car.smokePosition.position, Quaternion.identity);
                newEffect.parent = Game.Instance.EffectsParent.transform;
            }
        }
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
        if (car.fourWheelDrive)
        {
            WheelColliderFL.motorTorque = player.move.y * car.motorForce;
            WheelColliderFR.motorTorque = player.move.y * car.motorForce;
        }
    }

    private void HandleSteering()
    {
        // Adjust the steer angle based on input direction
        if (player.move.x != 0)
        {
            float speedSlowDownFactor = 1f;
            if (speed > car.maxSpeed/10)
            {
                // slow down the steering by a factor of max 10 
                speedSlowDownFactor = 1 / (10 * ( speed / car.maxSpeed));
            }
            currentSteerAngle += player.move.x * car.steerSpeed * Time.fixedDeltaTime * speedSlowDownFactor;
            currentSteerAngle = Mathf.Clamp(currentSteerAngle, -car.maxSteerAngle, car.maxSteerAngle);
        }
        else
        {
            // gradually return the wheels to center when there's no input
            currentSteerAngle = Mathf.MoveTowards(currentSteerAngle, 0, car.steerSpeed * Time.fixedDeltaTime);
        }

        WheelColliderFL.steerAngle = currentSteerAngle;
        WheelColliderFR.steerAngle = currentSteerAngle;
    }

    private void UpdateVisualWheels()
    {
        UpdateWheelPose(WheelColliderFL, frontLeftWheelTransform, true);
        UpdateWheelPose(WheelColliderFR, frontRightWheelTransform, true);
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
        float wheelRotationAngle = player.move.y * car.motorForce * Time.deltaTime / collider.radius;
        wheelTransform.Rotate(wheelRotationAngle, 0, 0, Space.Self);

        // Correct rotation by aligning to the wheel collider's rotation
        wheelTransform.rotation = rot * Quaternion.Euler(car.wheelsOrientation);
    }

    private void CheckLanding()
    {
        bool allWheelsGrounded = WheelColliderFL.isGrounded && WheelColliderFR.isGrounded
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
