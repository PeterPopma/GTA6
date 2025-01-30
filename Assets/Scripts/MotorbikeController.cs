using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using Unity.Cinemachine;

public class MotorbikeController : MonoBehaviour
{
    [Header("Cinemachine")]
    [Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
    public GameObject CinemachineCameraTarget;

    [SerializeField] TextMeshProUGUI textSpeed;
    [SerializeField] Material materialRearLight;
    [SerializeField] Transform vfxRoot;
    [SerializeField] GameObject sparks;
    [SerializeField] private CinemachineCamera vcamPlayerFollow;
    [SerializeField] private bool useBalancingForce;
    [SerializeField] private bool useGyroscopicEffect;
    [SerializeField] private bool useDownforce;
    [SerializeField] private bool useTilting;
    [SerializeField] private bool useBankingForce;
    [SerializeField] private bool useSidewaysfriction;
    [SerializeField] private bool useStabilizeBike;
    [SerializeField] private Transform vfxSmoke;

    public Color gizmoColor = Color.red; // Color of the marker
    public float gizmoSize = 0.1f; // Size of the marker

    [Header("Input Values")]
    public Vector2 move;
    public Vector2 look;
    public bool sprint;
    private Motorbike motorbike;
    private const float _threshold = 0.01f;
    private float timeleftSmoke;

    private const float TIME_BETWEEN_SMOKE = 1.5f;

    // cinemachine
    private float cinemachineTargetYaw;
    private float cinemachineTargetPitch;
    private PlayerInput _playerInput;

    private int viewDistance = 5;

    private bool isAccelerating;

    private Transform steering; // Reference to the steering mechanism (handlebar)

    // WheelColliders
    private WheelCollider frontWheelCollider;
    private WheelCollider rearWheelCollider;

    // Visual Wheel Transforms
    private Transform frontWheelTransform;
    private Transform rearWheelTransform;

    private float currentSteerAngle; // Current angle of the steering

    private bool isBraking;
    private float skidmarkStrength;

    new private Rigidbody rigidbody;

    private float speed;
    private float currentEngineVolume;

    int lastSkid = -1; // Array index for the skidmarks controller. Index of last skidmark piece this wheel used
    float slipLastUpdateTime;
    WheelHit wheelHitInfo;
    private Skidmarks skidmarks;

    public void OnMove(InputValue value)
    {
        move = value.Get<Vector2>();
    }

    public void OnLook(InputValue value)
    {
        look = value.Get<Vector2>();
    }
        
    public void OnSprint(InputValue value)
    {
        sprint = value.isPressed;
    }

    public void OnEnterVehicle()
    {
        GetComponent<Player>().enabled = true;
        ExitMotorbike();
        GetComponent<Player>().ExitVehicle();
    }

    void Awake()
    {
        Cursor.visible = false;
        skidmarks = GameObject.Find("Scripts/Skidmarks").GetComponent<Skidmarks>();
        slipLastUpdateTime = Time.time;
        textSpeed.text = "";
        _playerInput = GetComponent<PlayerInput>();
        Motorbike motorbike = GetComponent<Motorbike>();
        if (motorbike != null)
        {
            SetMotorbike(motorbike);
        }
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
    public Motorbike Motorbike { get => motorbike; set => motorbike = value; }

    public void SetMotorbike(Motorbike motorbike)
    {
        this.motorbike = motorbike;
        if (motorbike.driver != null)
        {
            motorbike.driver.SetActive(true);
        }
        rigidbody = motorbike.gameObject.GetComponent<Rigidbody>();
        rigidbody.isKinematic = false;
        steering = motorbike.Steering;
        frontWheelTransform = motorbike.WheelMeshF;
        rearWheelTransform = motorbike.WheelMeshR;
        frontWheelCollider = motorbike.WheelColliderF;
        rearWheelCollider = motorbike.WheelColliderR;
        if (motorbike.soundEngine != null)
        {
            motorbike.soundEngine.Play();
        }
        textSpeed.enabled = true;
    }

    public void ExitMotorbike()
    {
        if (motorbike.driver != null)
        {
            motorbike.driver.SetActive(false);
        }
        motorbike = null;
    }

    void FixedUpdate()
    {
        if (motorbike == null)
        {
            return;
        }

        slipLastUpdateTime = Time.time;
        HandleMotor();
        HandleSteering();
        LimitMaxSpeed();
        UpdateGUI();
        UpdateEngineSound();
        ApplyEffects();
        ApplyBrakes();
        ApplyDeceleration();
        if (useBalancingForce)
            ApplyBalancingForce();
        if(useGyroscopicEffect)
            ApplyGyroscopicEffect();
        if (useDownforce)
            ApplyDownforce();
        if (useTilting)
            ApplyTilting();
        if (useBankingForce)
            ApplyBankingForce();
        if (useSidewaysfriction)
            AdjustSidewaysFriction();
        if (useStabilizeBike)
            StabilizeBike();
        speed = rigidbody.linearVelocity.magnitude;
    }

    void ApplyTilting()
    {
        // Calculate the desired tilt angle with speed-based damping
        float adjustedMaxTilt = Mathf.Lerp(motorbike.maxTiltAngle, 10f, speed / motorbike.maxSpeed); // Reduce tilt at higher speeds
        float desiredTilt = adjustedMaxTilt * (move.x * Mathf.Clamp(speed / motorbike.maxSpeed, 0f, 1f));

        // Smoothly rotate the bike body to simulate tilting
        Quaternion targetRotation = Quaternion.Euler(desiredTilt, motorbike.transform.eulerAngles.y, motorbike.transform.eulerAngles.z);
        motorbike.transform.rotation = Quaternion.Lerp(motorbike.transform.rotation, targetRotation, Time.fixedDeltaTime * motorbike.tiltSpeed);
    }

    void ApplyBankingForce()
    {
        // Add force to simulate the bike banking during turns
        Vector3 bankingForce = transform.right * move.x * speed * motorbike.bankingForceMultiplier;
        rigidbody.AddForce(bankingForce, ForceMode.Force);
    }
    void SetWheelFrictionStiffness(WheelCollider wheel, float newStiffness)
    {
        WheelFrictionCurve friction = wheel.sidewaysFriction;
        friction.stiffness = newStiffness;
        wheel.sidewaysFriction = friction;
    }

    void AdjustSidewaysFriction()
    {
        float stiffness = Mathf.Lerp(1f, motorbike.sidewaysFrictionMultiplier, Mathf.Abs(move.x));
        SetWheelFrictionStiffness(frontWheelCollider, stiffness);
        SetWheelFrictionStiffness(frontWheelCollider, stiffness);
    }

    void StabilizeBike()
    {
        // Add a torque to reduce wobble after a tilt (stabilization)
        Vector3 stabilizationTorque = -rigidbody.angularVelocity * 0.5f;
        rigidbody.AddTorque(stabilizationTorque, ForceMode.VelocityChange);
    }

    void ApplySpinTorque()
    {
        Vector3 spinTorque = motorbike.transform.up * motorbike.brakeSpinStrength * speed;
        rigidbody.AddTorque(Vector3.ClampMagnitude(spinTorque, 20), ForceMode.Impulse);
    }

    void ApplyDownforce()
    {
        float downforce = Mathf.Clamp(rigidbody.linearVelocity.magnitude * motorbike.downforceFactor, 0f, motorbike.maxDownforce);
        rigidbody.AddForce(-motorbike.transform.up * downforce);
    }

    private void Update()
    {
        UpdateVisualWheels();
    }
    protected void LateUpdate()
    {
        if (skidmarkStrength>0 && rearWheelCollider.GetGroundHit(out wheelHitInfo))
        {
            // Skid
            Vector3 skidPoint = wheelHitInfo.point + (rigidbody.linearVelocity * (Time.time - slipLastUpdateTime));
            lastSkid = skidmarks.AddSkidMark(skidPoint, wheelHitInfo.normal, skidmarkStrength, lastSkid);
        }
        else
        {
            lastSkid = -1;
        }

        CameraRotation();
    }

    private void UpdateEngineSound()
    {
        if (motorbike.soundEngine == null)
        {
            return;
        }

        if (move.y != 0)
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

        motorbike.soundEngine.volume = currentEngineVolume;

        float pitch = 0.7f + 2 * (speed / motorbike.maxSpeed) + 0.5f * motorbike.soundEngine.volume;
        motorbike.soundEngine.pitch = pitch;
    }

    private void ApplyBalancingForce()
    {
        // Calculate the torque needed to stabilize
        Vector3 currentUp = motorbike.transform.up;
        Vector3 targetUp = Vector3.up;
        Vector3 torque = Vector3.Cross(currentUp, targetUp) * motorbike.balanceForce;

        // Apply torque to the Rigidbody
        rigidbody.AddTorque(torque);
    }

    private void ApplyGyroscopicEffect()
    {
        if (rigidbody.linearVelocity.magnitude > 1f)
        {
            // Apply a stabilizing force proportional to the velocity
            Vector3 gyroForce = motorbike.transform.right * rigidbody.angularVelocity.magnitude * rigidbody.linearVelocity.magnitude;
            rigidbody.AddTorque(gyroForce);
        }
    }

    private void UpdateGUI()
    {
        textSpeed.text = speed.ToString("0") + " Km/h";
    }

    private bool IsMovingForward()
    {
        // Get the velocity of the car
        Vector3 velocity = rigidbody.linearVelocity;

        // Calculate the dot product of the velocity and the forward direction
        float dot = Vector3.Dot(motorbike.transform.forward, velocity);

        // If the dot product is positive, the car is moving forward;
        // if it's negative, the car is moving backward.
        return dot > 0.01f;
    }

    private void LimitMaxSpeed()
    {
        if (rigidbody.linearVelocity.magnitude > motorbike.maxSpeed)
        {
            rigidbody.linearVelocity = rigidbody.linearVelocity.normalized * motorbike.maxSpeed;
        }
    }

    private void ResetBraking()
    {
        // Release brakes
        frontWheelCollider.brakeTorque = 0;
        rearWheelCollider.brakeTorque = 0;
        rigidbody.linearDamping = motorbike.normalDrag;
    }

    private void ApplyDeceleration()
    {
        if (!isAccelerating)  // No gas input
        {
            // Optionally, apply a small brake force to slow down the wheels
            rearWheelCollider.brakeTorque = motorbike.engineBrakingForce;
        }
        else
        {
            // Release any braking force when accelerating
            rearWheelCollider.brakeTorque = 0;
        }
    }

    private void ApplyBrakes()
    {
        if (!isBraking && move.y < 0 && IsMovingForward())
        {
            isBraking = true;
            currentEngineVolume = 0;
            if (motorbike.soundBrakes != null)
            {
                motorbike.soundBrakes.Play();
            }
            materialRearLight.SetFloat("_EmissiveExposureWeight", 0.3f);
        }

        if (isBraking)
        {
            if (skidmarkStrength < 1)
            {
                skidmarkStrength += 0.05f;
            }

            // Apply brake torque to all wheels
            frontWheelCollider.brakeTorque = motorbike.brakeForce;
            rearWheelCollider.brakeTorque = motorbike.brakeForce;
            rigidbody.linearDamping = motorbike.brakingDrag;
            ApplySpinTorque();

            if (move.y >= 0 || !IsMovingForward())
            {
                isBraking = false;
                materialRearLight.SetFloat("_EmissiveExposureWeight", 0.9f);
                ResetBraking();
            }
        }
        else
        {
            if (skidmarkStrength > 0)
            {
                skidmarkStrength -= 0.05f;
            }
        }
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

    private void ApplyEffects()
    {
        if (motorbike.smokePosition != null)
        {
            timeleftSmoke -= Time.deltaTime * speed;
            if (timeleftSmoke < 0)
            {
                timeleftSmoke = TIME_BETWEEN_SMOKE;
                Transform newEffect = Instantiate(vfxSmoke, motorbike.smokePosition.position, Quaternion.identity);
                newEffect.parent = vfxRoot;
            }
        }
        if (move.y > 0)
        {
            sparks.SetActive(true);
        }
        else
        {
            sparks.SetActive(false);
        }
    }

    private void HandleMotor()
    {
        if (move.y != 0)
        {
            isAccelerating = true;
        }
        else
        {
            isAccelerating = false;
        }

        // Apply motor torque to the rear wheels
        rearWheelCollider.motorTorque = move.y * motorbike.motorForce;
    }

    private void HandleSteering()
    {
        // Adjust the steer angle based on input direction
        if (move.x != 0)
        {
            float speedSlowDownFactor = 1f;
            if (speed > motorbike.maxSpeed / 10)
            {
                // slow down the steering by a factor of max 10 
                speedSlowDownFactor = 1 / (10 * (speed / motorbike.maxSpeed));
            }
            currentSteerAngle += move.x * motorbike.steerSpeed * Time.fixedDeltaTime * speedSlowDownFactor;
            currentSteerAngle = Mathf.Clamp(currentSteerAngle, -motorbike.maxSteerAngle, motorbike.maxSteerAngle);
        }
        else
        {
            // gradually return the wheels to center when there's no input
            currentSteerAngle = Mathf.MoveTowards(currentSteerAngle, 0, motorbike.steerSpeed * Time.fixedDeltaTime);
        }

        // Apply rotation to the steering mechanism only on the Y-axis
        steering.localRotation = Quaternion.AngleAxis(currentSteerAngle, Vector3.up);

        frontWheelCollider.steerAngle = currentSteerAngle;
    }

    private void UpdateVisualWheels()
    {
        //   UpdateWheelPose(frontWheelCollider, frontWheelTransform, true);
        frontWheelTransform.Rotate(new Vector3(0, 0, 100*speed*Time.deltaTime));
        UpdateWheelPose(rearWheelCollider, rearWheelTransform, false);
    }

    private void UpdateWheelPose(WheelCollider collider, Transform wheelTransform, bool isFrontWheel)
    {
        Vector3 pos;
        Quaternion rot;
        collider.GetWorldPose(out pos, out rot);

        // Update position
        wheelTransform.position = pos;

        // Update rotation for the wheel based on speed
        float wheelRotationAngle = move.y * motorbike.motorForce * Time.deltaTime / collider.radius;
        wheelTransform.Rotate(wheelRotationAngle, 0, 0, Space.Self);

        // Correct rotation by aligning to the wheel collider's rotation
        wheelTransform.rotation = rot * Quaternion.Euler(motorbike.wheelsOrientation);
    }

    private void CameraRotation()
    {
        // if there is an input and camera position is not fixed
        if (look.sqrMagnitude >= _threshold)
        {
            // Don't multiply mouse input by Time.deltaTime;
            float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;

            cinemachineTargetYaw += look.x * deltaTimeMultiplier;
            cinemachineTargetPitch += look.y * deltaTimeMultiplier;
        }

        // Cinemachine will follow this target
        CinemachineCameraTarget.transform.rotation = Quaternion.Euler(cinemachineTargetPitch, cinemachineTargetYaw, 0.0f);
    }

    public void OnChangeView()
    {
        if (viewDistance == 1)
        {
            viewDistance = 2;
        }
        else if (viewDistance == 2)
        {
            viewDistance = 5;
        }
        else if (viewDistance == 5)
        {
            viewDistance = 10;
        }
        else
        {
            viewDistance = 1;
        }
        UpdateView();
    }

    public void UpdateView()
    {
        vcamPlayerFollow.GetComponent<CinemachineThirdPersonFollow>().CameraDistance = viewDistance;
    }

}
