using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public enum SoldierState_
{
    WalkingAround,
    FollowingPlayer
}

public class Enemy : MonoBehaviour {
    const float TIME_BEFORE_DYING_PLAYER_IS_REMOVED = 300;
    const float ANIMATION_LENGTH_SHOOTING = 4;
    const int ANIMATION_LAYER_FIRE_GUN = 1;

    [SerializeField] private Transform spawnFirePosition;
	[SerializeField] private GameObject gunFire;
	[SerializeField] private Transform effectsRoot;
    [SerializeField] private new Rigidbody rigidbody;

    private List<AudioClip> clipsScreamMale = new List<AudioClip>();
    private AudioSource soundGunshot;
	private Vector3 playerPosition;
	private bool isHit;
	private Animator animator;
	private float timeDied;
	private float fireDelay;
    private float timeLeftFiring;
    private bool firedGun;
    private CharacterController characterController;
    private SoldierState_ soldierState;
    private int animIDSpeed;
    private float currentSpeed;
    private float moveSpeed = 3.0f;
    private Vector2 destination;
    private int timesStuck;
    private Vector3 storedPosition;
    private float timeLastDistanceMeasurement;
    private float timeLeftDying;

    public bool IsHit { get => isHit; set => isHit = value; }

    void Awake()
    {
        animIDSpeed = Animator.StringToHash("Speed");
        animator = GetComponent<Animator>();
        characterController = GetComponent<CharacterController>();
    }

    void Start () {
        Transform soundsRoot = GameObject.Find("/Sound/MaleScreams").transform;
        foreach (Transform item in soundsRoot)
        {
            AudioClip clip = item.gameObject.GetComponent<AudioSource>().clip;
            clipsScreamMale.Add(clip);
        }
        soundGunshot = GameObject.Find("/Sound/Gunshot2").GetComponent<AudioSource>();
        playerPosition = GameObject.Find("Player").transform.position;
    }

    private void Scream()
    {
        AudioSource.PlayClipAtPoint(clipsScreamMale[UnityEngine.Random.Range(0, clipsScreamMale.Count)], transform.position);
    }

    public void Hit(Vector3 hitPosition)
    {
        isHit = true;
        timeLeftDying = TIME_BEFORE_DYING_PLAYER_IS_REMOVED;
        Scream();
        characterController.enabled = false;
        animator.enabled = false;
        Vector3 forceDirection = (hitPosition - transform.position).normalized;
        forceDirection = new Vector3(forceDirection.x, 12, forceDirection.z);
        forceDirection *= 2.5f;
        rigidbody.AddForce(forceDirection, ForceMode.VelocityChange);
        rigidbody.AddTorque(UnityEngine.Random.insideUnitSphere, ForceMode.VelocityChange);
    }

    private void Update()
    {
        if (timeLeftFiring > 0)
        {
            timeLeftFiring -= Time.deltaTime;
            animator.SetLayerWeight(ANIMATION_LAYER_FIRE_GUN, Mathf.Lerp(animator.GetLayerWeight(ANIMATION_LAYER_FIRE_GUN), 1f, Time.deltaTime * 4f));
            if (timeLeftFiring < 4 && !firedGun)
            {
                gunFire.SetActive(true);
                AudioSource.PlayClipAtPoint(soundGunshot.clip, spawnFirePosition.position);
                firedGun = true;
            }
            if (timeLeftFiring < 3)
            {
                gunFire.SetActive(false);
            }
        }
        else
        {
            animator.SetLayerWeight(ANIMATION_LAYER_FIRE_GUN, Mathf.Lerp(animator.GetLayerWeight(ANIMATION_LAYER_FIRE_GUN), 1f, Time.deltaTime * 4f));
            fireDelay -= Time.deltaTime;
            if (fireDelay <= 0)
            {
                fireDelay = UnityEngine.Random.Range(0.5f, 10f);
                timeLeftFiring = ANIMATION_LENGTH_SHOOTING;
            }
        }

        if (isHit)
        {
            Progress.Instance.Kills++;
            timeLeftDying -= Time.deltaTime;

            if (timeLeftDying < 0)
            {
                Destroy(gameObject);
            }
		}

        switch (soldierState)
        {
            case SoldierState_.WalkingAround:
                Move();
                break;
            case SoldierState_.FollowingPlayer:
                FollowPlayer();
                break;
        }

        if (!characterController.isGrounded)
        {
            // make sure characters stay on the ground
            characterController.Move(new Vector3(0.0f, -4f * Time.deltaTime, 0.0f));
        }
    }

    public void OnFootstep()
    {
    }

    private void FollowPlayer()
    {
        Vector3 distanceToPlayer = playerPosition - transform.position;
        if (distanceToPlayer.magnitude > 1.1f)
        {
            if (currentSpeed < 4)
            {
                currentSpeed += Time.deltaTime * 8f;
            }
            distanceToPlayer.Normalize();
            Vector3 direction = new Vector3(distanceToPlayer.x, 0, distanceToPlayer.z);
            Vector3 newDirection = new Vector3(Mathf.Lerp(transform.forward.x, direction.x, Time.deltaTime * 4f), 0, Mathf.Lerp(transform.forward.z, direction.z, Time.deltaTime * 4f));

            transform.rotation = Quaternion.LookRotation(newDirection, Vector3.up);

            //           transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
            characterController.Move(newDirection * Time.deltaTime * 2 * moveSpeed);
        }
        else
        {
            if (currentSpeed > 0)
            {
                currentSpeed -= Time.deltaTime * 8f;
            }
            else
            {
                NewRandomDestination();
            }
        }
        animator.SetFloat(animIDSpeed, currentSpeed);
    }

    private void NewRandomDestination()
    {
        destination = new Vector2(UnityEngine.Random.value * (Settings.PLAYFIELD_MAX_X - Settings.PLAYFIELD_MIN_X) + Settings.PLAYFIELD_MIN_X, UnityEngine.Random.value * (Settings.PLAYFIELD_MAX_Z - Settings.PLAYFIELD_MIN_Z) + Settings.PLAYFIELD_MIN_Z);
    }

    private void Move()
    {
        if (Time.time - timeLastDistanceMeasurement > 5)
        {
            timeLastDistanceMeasurement = Time.time;
            if ((storedPosition - transform.position).magnitude < 0.1f)
            {
                // stuck..
                timesStuck++;
                if (timesStuck > 10)
                {
                    Destroy(gameObject);
                }
                NewRandomDestination();
            }
            else
            {
                timesStuck = 0;
            }
            storedPosition = transform.position;
        }
        if (destination != Vector2.zero)
        {
            Vector2 distanceToDestination = destination - new Vector2(transform.position.x, transform.position.z);
            if (distanceToDestination.magnitude > 0.1f)
            {
                animator.SetFloat(animIDSpeed, 2f);
                distanceToDestination.Normalize();
                Vector3 direction = new Vector3(distanceToDestination.x, 0, distanceToDestination.y);
                transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
                characterController.Move(direction * Time.deltaTime * moveSpeed);
            }
            else
            {
                animator.SetFloat(animIDSpeed, 0);
                NewRandomDestination();
            }
        }
    }
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponent<Bullet>() != null)
        {
            Hit(other.gameObject.transform.position);
        }
    }
}
