using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using TMPro;
using Unity.Cinemachine;
using UnityEngine.SceneManagement;

public class AirplaneController : MonoBehaviour
{
	// cinemachine
	private float cinemachineTargetYaw;
	private float cinemachineTargetPitch;
	private PlayerInput _playerInput;
	private const float _threshold = 0.01f;

	private const float MINIMUM_FLY_SPEED = 30;
	private float currentSpeed;
	private float throttle;
	private bool isCrashed;

	[SerializeField] private TextMeshProUGUI textSpeed;
	[SerializeField] private TextMeshProUGUI textAltitude;

	private Vector3 speed;

	float minimapPositionX;
	float minimapPositionY;
	float minimapRotationY;

	private float altitude, previousAltitude;
	private float heightAboveGround;

	private float movementX;
	private float movementY;
	bool buttonAccelerate;
	bool buttonDecelerate;
	bool buttonYawLeft;
	bool buttonYawRight;
	private float smokeAmount;
	private float smokeAmountLimit = 0;

	float distanceTravelled;
	private Airplane airplane;
	private Player player;

	public Vector3 Speed { get => speed; set => speed = value; }
	public float CurrentSpeed { get => currentSpeed; set => currentSpeed = value; }

	public float DistanceTravelled { get => distanceTravelled; set => distanceTravelled = value; }
	public float MinimapPositionX { get => minimapPositionX; set => minimapPositionX = value; }
	public float MinimapPositionY { get => minimapPositionY; set => minimapPositionY = value; }
	public float MinimapRotationY { get => minimapRotationY; set => minimapRotationY = value; }
    public Airplane Airplane { get => airplane; set => airplane = value; }

    private void Awake()
	{
		player = GetComponent<Player>();
		_playerInput = GetComponent<PlayerInput>();
	}

	private void Start()
	{
	}

	private bool IsCurrentDeviceMouse
	{
		get
		{
			return _playerInput.currentControlScheme == "KeyboardMouse";
		}
	}

	public void SetAirplane(Airplane airplane)
	{
		Airplane = airplane;
		Game.Instance.SetFollowCamera(Airplane.cameraRoot);
	}

	public void ExitAirplane()
    {
		GetComponent<AirplaneController>().Airplane = null;
		GetComponent<AirplaneController>().enabled = false;
		textSpeed.text = "";
		textAltitude.text = "";
	}

	private void Update()
	{
		if (airplane == null)
        {
			return;
        }
		previousAltitude = altitude;
		altitude = airplane.transform.position.y;
		textAltitude.text = "throttle: " + throttle.ToString("0") + "     alt: " + altitude.ToString("0");
		heightAboveGround = transform.position.y-Terrain.activeTerrain.SampleHeight(transform.position);

		Movement();
		if (currentSpeed < MINIMUM_FLY_SPEED)
		{
			Airplane.GetComponent<Rigidbody>().useGravity = true;
			Airplane.GetComponent<Rigidbody>().isKinematic = false;
		}
		else
		{
			Airplane.GetComponent<Rigidbody>().useGravity = false;
			Airplane.GetComponent<Rigidbody>().isKinematic = true;
		}

		smokeAmount += throttle * Time.deltaTime * Random.value;
		if (smokeAmount > smokeAmountLimit)
		{
			smokeAmountLimit = Random.value * 25;
			smokeAmount = 0;
			Instantiate(Airplane.vfxSmoke, Airplane.spawnPositionSmoke.position, Quaternion.identity); 
		}

		// Rotate propellers if any
		if (Airplane.propellors.Length > 0)
		{
			RotatePropellors(Airplane.propellors);
		}
	}
	private void OnYawLeft(InputValue value)
	{
		buttonYawLeft = value.isPressed;
	}

	private void OnYawRight(InputValue value)
	{
		buttonYawRight = value.isPressed;
	}
	
	private void OnAccelerate(InputValue value)
	{
		buttonAccelerate = value.isPressed;
	}

	private void OnDecelerate(InputValue value)
	{
		buttonDecelerate = value.isPressed;
	}

	private void Movement()
	{
		Vector3 oldPosition = Airplane.transform.position;

		// Move forward
		Airplane.transform.Translate(Vector3.forward * currentSpeed * Time.deltaTime);

		if (player.move.x == 1 || player.move.x == -1)
		{
			movementX = player.move.x;
		}
		else if (movementX!=0)
		{
			// slowly die out
			movementX *= Mathf.Pow(0.01f, Time.deltaTime);
			if (Mathf.Abs(movementX) < 0.01f)
			{
				movementX = 0;
			}
		}

		if (player.move.y == 1 || player.move.y == -1)
		{
			movementY = player.move.y;
		}
		else if (movementY != 0)
		{
			// slowly die out
			movementY *= Mathf.Pow(0.01f, Time.deltaTime);
			if (Mathf.Abs(movementY) < 0.01f)
			{
				movementY = 0;
			}
		}

		// Rotate airplane by inputs
		if (currentSpeed > MINIMUM_FLY_SPEED)
		{
			Airplane.transform.Rotate(Vector3.forward * -movementX * Airplane.rollSpeed * Time.deltaTime);
			Airplane.transform.Rotate(Vector3.right * movementY * Airplane.pitchSpeed * Time.deltaTime);
		}

		// Move back to straight
		// TODO : not right yet
		/*
		if (transform.rotation.z < 0)
		{
			transform.Rotate(Vector3.forward * 125f * Time.deltaTime);
		}
		if (transform.rotation.z > 0)
		{
			transform.Rotate(Vector3.forward * -125f * Time.deltaTime);
		}
		
		Debug.Log("rotation:" + transform.rotation.x.ToString("0.00") + "," + transform.rotation.y.ToString("0.00") + "," + transform.rotation.z.ToString("0.00"));
		 */


		// Rotate yaw
		if (buttonYawRight)
		{
			Airplane.transform.Rotate(Vector3.up * Airplane.yawSpeed * Time.deltaTime);
		}
		else if (buttonYawLeft)
		{
			Airplane.transform.Rotate(-Vector3.up * Airplane.yawSpeed * Time.deltaTime);
		}
		if (buttonAccelerate)
		{
			if (throttle < 100)
			{
				throttle += Airplane.throttleAcceleration * Time.deltaTime;
				if (throttle > 100)
				{
					throttle = 100;
				}
			}
		}
		if (buttonDecelerate)
		{
			if (throttle > 0)
			{
				throttle -= Airplane.throttleAcceleration * Time.deltaTime;
				if (throttle < 0)
				{
					throttle = 0;
				}
			}
		}

		float gainFromDescending = (previousAltitude - altitude) * Time.deltaTime * 2000;
		if (gainFromDescending > 100)
		{
			gainFromDescending = 100;
		}
		if (gainFromDescending < -100)
		{
			gainFromDescending = -100;
		}

		if (throttle + gainFromDescending > currentSpeed)
		{
			currentSpeed += Airplane.speedAcceleration * Time.deltaTime;
		}

		if (throttle + gainFromDescending < currentSpeed)
		{
			currentSpeed -= Airplane.speedAcceleration * Time.deltaTime;
		}
		distanceTravelled += (Airplane.transform.position - oldPosition).magnitude;
		speed = (Airplane.transform.position - oldPosition) / Time.deltaTime;
		textSpeed.text = "Speed: " + ((int)speed.magnitude).ToString();

		float pitch = speed.magnitude / 100;
		if (pitch < 0.8)
		{
			pitch = 0.8f;
		}
		Airplane.soundEngine.pitch = pitch;
		Airplane.soundEngine.volume = throttle / 100;
	}

	private void RotatePropellors(GameObject[] _rotateThese)
	{
		float _propelSpeed = throttle * Airplane.propelSpeedMultiplier;

		for (int i = 0; i < _rotateThese.Length; i++)
		{
			_rotateThese[i].transform.Rotate(Vector3.forward * -_propelSpeed * Time.deltaTime);
		}
	}

	public void Crash()
	{
		if (isCrashed)
		{
			return;
		}

		// Set rigidbody to non-kinematic
		GetComponent<Rigidbody>().isKinematic = false;
		GetComponent<Rigidbody>().useGravity = true;

		isCrashed = true;
		Airplane.soundEngine.volume = 0f;

		Instantiate(Airplane.vfxCrash, Airplane.transform.position, Quaternion.identity);
		Airplane.soundCrash.Play();
	}

	private void LateUpdate()
	{
		if (airplane == null)
		{
			return;
		}
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
		Airplane.cameraRoot.transform.rotation = Quaternion.Euler(cinemachineTargetPitch, cinemachineTargetYaw, 0.0f);
	}
}
