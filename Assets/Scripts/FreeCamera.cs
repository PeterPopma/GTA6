using TMPro;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class FreeCamera : MonoBehaviour
{
    CinemachineCamera vcamMainCamera;

    [SerializeField] private float moveByKeySpeed = 4f; 
    [SerializeField] private float lookSpeed = 2f;
    Vector3 cameraPosition;

    private float cameraYaw = 0f;
    private float cameraPitch = 0f;
    private float cameraRoll = 0f;
    private bool buttonCameraForward;
    private bool buttonCameraBack;
    private bool buttonCameraLeft;
    private bool buttonCameraRight;
    private bool buttonCameraUp;
    private bool buttonCameraDown;
    private bool buttonCameraLookAround;
    private bool buttonCameraRollLeft;
    private bool buttonCameraRollRight;
    private Vector2 look;
    private float timeMovingStarted;
    private float moveSpeed;
    private bool movingCamera;
    float speedUpDown;
    float speedLeftRight;
    float speedBackForth;

    public Vector3 CameraPosition { get => cameraPosition; set => cameraPosition = value; }
    public float MoveSpeed { get => moveSpeed; set => moveSpeed = value; }

    public void Awake()
    {
        vcamMainCamera = GetComponent<CinemachineCamera>();
    }

    public void Start()
    {
    }

    private void OnLook(InputValue value)
    {
        look = value.Get<Vector2>();
    }

    private void UpdateCameraMoveStatus(bool isMoving)
    {
        if (isMoving)
        {
            timeMovingStarted = Time.time;
            movingCamera = true;
        }
        else
        {
            movingCamera = false;
            moveSpeed = 0;
        }
    }

    private void OnCameraForward(InputValue value)
    {
        UpdateCameraMoveStatus(value.isPressed);
        buttonCameraForward = value.isPressed;
    }

    private void OnCameraBack(InputValue value)
    {
        UpdateCameraMoveStatus(value.isPressed);
        buttonCameraBack = value.isPressed;
    }

    private void OnCameraLeft(InputValue value)
    {
        UpdateCameraMoveStatus(value.isPressed);
        buttonCameraLeft = value.isPressed;
    }

    private void OnCameraRight(InputValue value)
    {
        UpdateCameraMoveStatus(value.isPressed);
        buttonCameraRight = value.isPressed;
    }

    private void OnCameraUp(InputValue value)
    {
        UpdateCameraMoveStatus(value.isPressed);
        buttonCameraUp = value.isPressed;
    }

    private void OnCameraDown(InputValue value)
    {
        UpdateCameraMoveStatus(value.isPressed);
        buttonCameraDown = value.isPressed;
    }

    private void OnIncreaseScrollSpeed(InputValue value)
    {
        moveByKeySpeed *= 2f;
        Game.Instance.ShowMessage("Scroll speed: " + moveByKeySpeed);
    }

    private void OnDecreaseScrollSpeed(InputValue value)
    {
        moveByKeySpeed *= 0.5f;
        Game.Instance.ShowMessage("Scroll speed: " + moveByKeySpeed);
    }

    private void OnCameraLookAround(InputValue value)
    {
        buttonCameraLookAround = value.isPressed;
    }

    private void OnCameraRollLeft(InputValue value)
    {
        buttonCameraRollLeft = value.isPressed;
    }

    private void OnCameraRollRight(InputValue value)
    {
        buttonCameraRollRight = value.isPressed;
    }

    private void OnCameraReset()
    {
        cameraPitch = 0;
        cameraYaw = 0;
        cameraRoll = 0;
    }

    public void LateUpdate()
    {
        vcamMainCamera.transform.eulerAngles = new Vector3(cameraPitch, cameraYaw, cameraRoll);
        vcamMainCamera.transform.position = cameraPosition;
    }

    private void FixedUpdate()
    {
        if (!buttonCameraUp && !buttonCameraDown)
        {
            speedUpDown *= 0.99f;
        }
        if (!buttonCameraRight && !buttonCameraLeft)
        {
            speedLeftRight *= 0.99f;
        }
        if (!buttonCameraBack && !buttonCameraForward)
        {
            speedBackForth *= 0.99f; 
        }
    }

    private void Update()
    {
        if (!vcamMainCamera.isActiveAndEnabled)
        {
            return;
        }
        if (movingCamera)
        {
            moveSpeed = moveByKeySpeed; // * Mathf.Pow(2, (Time.time - timeMovingStarted));
        }

        if (buttonCameraRollLeft)
        {
            cameraRoll -= Time.deltaTime * 80;
        }
        if (buttonCameraRollRight)
        {
            cameraRoll += Time.deltaTime * 80;
        }
        if (buttonCameraForward)
        {
            speedBackForth += Time.deltaTime * moveSpeed;
        }
        if (buttonCameraBack)
        {
            speedBackForth -= Time.deltaTime * moveSpeed;
        }
        if (buttonCameraRight)
        {
            speedLeftRight += Time.deltaTime * moveSpeed;
        }
        if (buttonCameraLeft)
        {
            speedLeftRight -= Time.deltaTime * moveSpeed;
        }
        if (buttonCameraUp)
        {
            speedUpDown += Time.deltaTime * moveSpeed;
        }
        if (buttonCameraDown)
        {
            speedUpDown -= Time.deltaTime * moveSpeed;
        }

        cameraPosition += transform.up * speedUpDown;
        cameraPosition += transform.forward * speedBackForth;
        cameraPosition += transform.right * speedLeftRight;

        // Look around when right mouse is pressed
        if (buttonCameraLookAround)
        {
            cameraYaw += lookSpeed * look.x;
            cameraPitch -= lookSpeed * look.y;
        }
    }
}