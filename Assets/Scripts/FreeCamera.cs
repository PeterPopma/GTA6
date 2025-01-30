using TMPro;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class FreeCamera : MonoBehaviour
{
    CinemachineCamera vcamMainCamera;

    [SerializeField] private float moveByKeySpeed = 4f; 
    [SerializeField] private float lookSpeed = 2f;
    [SerializeField] private float zoomSpeed = 2f;
    Vector3 cameraPosition;

    private float cameraYaw = 0f;
    private float cameraPitch = 0f;
    private float cameraRoll = 0f;
    private float cameraDistance = 3f;
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
    }

    private void OnDecreaseScrollSpeed(InputValue value)
    {
        moveByKeySpeed *= 0.5f;
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

    private void Update()
    {
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
        Debug.Log(cameraDistance);
        if (buttonCameraForward)
        {
            cameraPosition += transform.forward * Time.deltaTime * moveSpeed;
        }
        if (buttonCameraBack)
        {
            cameraPosition -= transform.forward * Time.deltaTime * moveSpeed;
        }
        if (buttonCameraRight)
        {
            cameraPosition += transform.right * Time.deltaTime * moveSpeed;
        }
        if (buttonCameraLeft)
        {
            cameraPosition -= transform.right * Time.deltaTime * moveSpeed;
        }
        if (buttonCameraUp)
        {
            cameraPosition += transform.up * Time.deltaTime * moveSpeed;
        }
        if (buttonCameraDown)
        {
            cameraPosition -= transform.up * Time.deltaTime * moveSpeed;
        }

        // Look around when right mouse is pressed
        if (buttonCameraLookAround)
        {
            cameraYaw += lookSpeed * look.x;
            cameraPitch -= lookSpeed * look.y;
        }
    }
}