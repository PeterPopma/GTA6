using UnityEngine;

public class Airplane : MonoBehaviour
{
    public Transform spawnPositionSmoke;
	public AudioSource soundEngine;
	public AudioSource soundCrash;
	public GameObject[] propellors;
	public Transform cameraRoot;

	[Header("Rotating speeds")]
	[Range(5f, 500f)]
	public float yawSpeed = 50f;

	[Range(5f, 500f)]
	public float pitchSpeed = 100f;

	[Range(5f, 500f)]
	public float rollSpeed = 200f;

	[Header("Acceleration / Deceleration")]
	[Range(0.1f, 150f)]
	public float throttleAcceleration = 1f;
	[Range(0.1f, 150f)]
	public float speedAcceleration = 1f;

	[Header("Engine propellor settings")]
	[Range(10f, 10000f)]
	public float propelSpeedMultiplier = 100f;

	public Transform vfxCrash;
	public Transform vfxSmoke;
}
