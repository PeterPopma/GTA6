using UnityEngine;
using UnityEngine.InputSystem;

public class CarController : MonoBehaviour
{
    [Header("Cinemachine")]
    [Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
    public GameObject CinemachineCameraTarget;
    
    private Car car;
    private Player player;
    private const float _threshold = 0.01f;
    public Car Car { get => car; set => car = value; }

    // cinemachine
    private float cinemachineTargetYaw;
    private float cinemachineTargetPitch;
    private PlayerInput _playerInput;

    [SerializeField] private float motorForce = 1500f;  // Force applied to move the car forward
    [SerializeField] private float brakeForce = 3000f;  // Force applied when braking
    private const float maxSteerAngle = 30f; // Maximum angle for steering

    private Rigidbody rigidbody;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
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

    public void SetCar(Car car)
    {
        this.car = car;
        rigidbody = car.gameObject.GetComponent<Rigidbody>();
    }
    private void HandleMotor()
    {
        // Apply force for acceleration and braking
        if (player.move.y > 0)
        {
            rigidbody.AddForce(transform.forward * player.move.y, ForceMode.Acceleration);
        }
        if (player.move.y < 0)
        {
            rigidbody.AddForce(-transform.forward * brakeForce, ForceMode.Acceleration);
        }
    }

    private void HandleSteering()
    {
        if (player.move.x > 0 && car.TireSteerRotation < maxSteerAngle)
        {
            car.TireSteerRotation += Time.deltaTime * 300f;
        }
        if (player.move.x < 0 && car.TireSteerRotation > -maxSteerAngle)
        {
            car.TireSteerRotation -= Time.deltaTime * 300f;
        }
        
        // Rotate the car based on the steer angle
        Quaternion turnRotation = Quaternion.Euler(0, car.TireSteerRotation * Time.deltaTime, 0);
        rigidbody.MoveRotation(rigidbody.rotation * turnRotation);
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

        /*

        if (player.move.x > 0 && car.TireSteerRotation<30)
        {
            car.TireSteerRotation += Time.deltaTime * 300f;
            Debug.Log(car.TireSteerRotation);
        }
        if (player.move.x < 0 && car.TireSteerRotation > -30)
        {
            car.TireSteerRotation -= Time.deltaTime * 300f;
            Debug.Log(car.TireSteerRotation);
        }
        if (player.move.y > 0)
        {
            car.Speed += Time.deltaTime;
            car.Accelerate(player.move.y);
        }
        if (player.move.y < 0)
        {
            car.Speed -= Time.deltaTime;
            car.Accelerate(player.move.y);
        }

        player.transform.position = car.transform.position;
        */
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
