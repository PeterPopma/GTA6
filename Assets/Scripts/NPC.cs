using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum NPCState_
{
    WalkingAround,
    FollowingPlayer,
    Falling,
    StandingStill,
    Patrol
}

public class NPC : MonoBehaviour
{
    const int LAYER_SHOOT = 4;
    const int TIME_BEFORE_DYING_PLAYER_IS_REMOVED = 60;
    const float MOVE_SPEED = 3.0f;

    [SerializeField] bool isFemale;
    [SerializeField] bool useRagdoll;
    [SerializeField] NPCState_ initialState = NPCState_.WalkingAround;
    [SerializeField] private Transform hips;
    [SerializeField] private new Rigidbody rigidbody;
    [SerializeField] private GameObject handPosition;
    [SerializeField] private GameObject gunFirePistol;
    [SerializeField] private Transform vfxFireGun;
    [SerializeField] private GameObject pistol;
    [SerializeField] private float aggresiveness = 0.5f;

    private AudioSource soundGunshot;
    private CharacterController characterController;
    private Animator animator;
    private AudioSource soundScreamMale, soundScreamMale2, soundScreamFemale;
    private AudioSource soundDyingMale, soundDyingMale2, soundDyingFemale;
    private NPCState_ npcState, initialNPCState;
    private Vector3 storedPosition;
    private Vector3 positionBeforeDying;
    private Player player;
    private int animIDSpeed;
    private int timesStuck;
    private int layer;
    private float timeLastDistanceMeasurement;
    private float currentSpeed;
    private float timeLeftDying;
    private float timeLeftShooting;
    private float timeLeftExploded;
    private float playerLowerValue;
    private Vector2 destination;
    private readonly List<Vector2> patrolRallyPoints = new();
    private int currentRallyPointIndex;
    private bool hasDied;
    private int timesHit;
    private bool pistolActive;
    private bool smokeCreated;

    public Vector2 Destination { get => destination; set => destination = value; }
    public NPCState_ NpcState { get => npcState; set => npcState = value; }
    public int TimesHit { get => timesHit; set => timesHit = value; }
    public bool UseRagdoll { get => useRagdoll; set => useRagdoll = value; }
    public bool HasDied { get => hasDied; set => hasDied = value; }

    void Awake()
    {
        animIDSpeed = Animator.StringToHash("Speed");
        animator = GetComponent<Animator>();
        characterController = GetComponent<CharacterController>();
        soundScreamMale = GameObject.Find("/Sound/ScreamMale").GetComponent<AudioSource>();
        soundScreamMale2 = GameObject.Find("/Sound/ScreamMale2").GetComponent<AudioSource>();
        soundScreamFemale = GameObject.Find("/Sound/ScreamFemale").GetComponent<AudioSource>();
        soundDyingMale = GameObject.Find("/Sound/DyingMale").GetComponent<AudioSource>();
        soundDyingMale2 = GameObject.Find("/Sound/DyingMale2").GetComponent<AudioSource>();
        soundDyingFemale = GameObject.Find("/Sound/DyingFemale").GetComponent<AudioSource>();
        soundGunshot = GameObject.Find("/Sound/Gunshot").GetComponent<AudioSource>();
    }

    private void Start()
    {
        initialNPCState = npcState = initialState;
        player = GameObject.Find("Player").GetComponent<Player>();

        if (pistol != null)
        {
            if (Random.value <= aggresiveness)
            {
                pistolActive = true;
            }
            else
            {
                pistolActive = false;
                pistol.SetActive(false);
            }
        }
        if (!useRagdoll)
        {
            // if it is not a default ragdoll, use it sometimes anyway for variation
            useRagdoll = Random.value > 0.7f;
        }
        if (initialState.Equals(NPCState_.Patrol))
        {
            patrolRallyPoints.Add(new Vector2(transform.position.x, transform.position.z));
            patrolRallyPoints.Add(new Vector2(transform.position.x - 5 + Random.value * 10, transform.position.z - 5 + Random.value * 10));
        }
        NewDestination();
    }

    public void SetPatrolDestinations(Vector2 destination)
    {
        npcState = NPCState_.Patrol;
        patrolRallyPoints.Clear();
        patrolRallyPoints.Add(new Vector2(transform.position.x, transform.position.z));
        patrolRallyPoints.Add(destination);
    }

    private void NextPatrolDestination()
    {
        currentRallyPointIndex++;
        if (currentRallyPointIndex >= patrolRallyPoints.Count)
        {
            currentRallyPointIndex = 0;
        }
        destination = patrolRallyPoints[currentRallyPointIndex];
    }

    void Update()
    {
        if (hasDied)
        {
            timeLeftDying -= Time.deltaTime;

            if (timeLeftDying < TIME_BEFORE_DYING_PLAYER_IS_REMOVED - 0.1 && layer == 1 && playerLowerValue < 1.4)
            {
                playerLowerValue += Time.deltaTime * 6;
                transform.position = positionBeforeDying - new Vector3(0, playerLowerValue, 0);
            }

            if (timeLeftDying < TIME_BEFORE_DYING_PLAYER_IS_REMOVED - 0.95f && layer == 2 && playerLowerValue < 1.4f)
            {
                playerLowerValue += Time.deltaTime * 2f;
                transform.position = positionBeforeDying - new Vector3(0, playerLowerValue, 0);
            }

            if (timeLeftDying < TIME_BEFORE_DYING_PLAYER_IS_REMOVED - 0.5f && layer == 3 && playerLowerValue < 1f)
            {
                playerLowerValue += Time.deltaTime * 6f;
                transform.position = positionBeforeDying - new Vector3(0, playerLowerValue, 0);
            }

            if (timeLeftDying < 0)
            {
                Game.Instance.NPCs.Remove(gameObject);
                Destroy(gameObject);
            }

            return;
        }

        if (timeLeftExploded > 0)
        {
            timeLeftExploded -= Time.deltaTime;
            if (timeLeftExploded < 0)
            {
                float y = Game.Instance.MainTerrain.SampleHeight(hips.position);
                if (timesStuck < 10 && hips.position.y - y > 1)
                {
                    // not on the ground yet
                    timeLeftExploded = 2f;
                    timesStuck++;     // prevent getting stuck when there is something wrong with sampling the terrain height
                }
                else
                {
                    transform.position = new Vector3(hips.position.x, y + 1, hips.position.z);
                    characterController.enabled = true;
                    animator.enabled = true;
                    rigidbody.isKinematic = false;
                    Die();
                }
            }
        }

        if (timeLeftShooting > 0)
        {
            timeLeftShooting -= Time.deltaTime;
            if (timeLeftShooting < 0)
            {
                animator.SetLayerWeight(LAYER_SHOOT, 0);
            }
            if (timeLeftShooting < 0.6 && !smokeCreated)
            {
                smokeCreated = true;
                Instantiate(vfxFireGun, handPosition.transform.position, Quaternion.identity);
                gunFirePistol.SetActive(true);
                if (Random.value > 0.5)
                {
                    player.Hit();
                }
            }
            if (timeLeftShooting < 0.5)
            {
                gunFirePistol.SetActive(false);
            }
        }
        else if (pistolActive)
        {
            Vector3 directionPlayer = player.transform.position - transform.position;
            if (directionPlayer.sqrMagnitude < 400)
            {
                npcState = NPCState_.StandingStill;
                animator.SetFloat(animIDSpeed, 0);

                // look at player
                transform.rotation = Quaternion.LookRotation(directionPlayer, Vector3.up);

                if (Random.value < 0.02)
                {
                    FirePistol();
                }
            }
            else
            {
                npcState = initialNPCState;
            }
        }

        if (!characterController.isGrounded && characterController.enabled /*&& npcState!=NPCState_.Falling*/)
        {
            // make sure characters stay on the ground
            characterController.Move(new Vector3(0.0f, -2f * Time.deltaTime, 0.0f));
        }

        switch (npcState)
        {
            case NPCState_.WalkingAround:
            case NPCState_.Patrol:
                Move();
                break;
            case NPCState_.StandingStill:
                break;
            case NPCState_.FollowingPlayer:
                FollowPlayer();
                break;
        }
    }

    private void FirePistol()
    {
        timeLeftShooting = 0.7f;
        animator.Play("Shoot", LAYER_SHOOT, 0);
        animator.SetLayerWeight(LAYER_SHOOT, 1);
        smokeCreated = false;
        soundGunshot.Play();
    }

    private void FollowPlayer()
    {
        Vector3 distanceToPlayer = player.transform.position - transform.position;
        if (distanceToPlayer.sqrMagnitude > 1.21f)
        {
            if (currentSpeed < 4)
            {
                currentSpeed += Time.deltaTime * 8f;   
            }
            distanceToPlayer.Normalize();
            Vector3 direction = new(distanceToPlayer.x, 0, distanceToPlayer.z);
            Vector3 newDirection = new(Mathf.Lerp(transform.forward.x, direction.x, Time.deltaTime * 4f), 0, Mathf.Lerp(transform.forward.z, direction.z, Time.deltaTime * 4f));

            transform.rotation = Quaternion.LookRotation(newDirection, Vector3.up);
            characterController.Move(2 * MOVE_SPEED * Time.deltaTime * newDirection);
        }
        else
        {
            if (currentSpeed > 0)
            {
                currentSpeed -= Time.deltaTime * 8f;
            }
            else
            {
                NewDestination();
            }
        }
        animator.SetFloat(animIDSpeed, currentSpeed);
    }

    public void OnFootstep()
    {

    }

    public void Explode()
    {
        characterController.enabled = false;
        animator.enabled = false;
        rigidbody.isKinematic = false;
        rigidbody.AddForce(new Vector3(0, 80, 0), ForceMode.VelocityChange);
        rigidbody.AddTorque(Random.insideUnitSphere, ForceMode.VelocityChange);
        timeLeftExploded = 4;
    }

    private void NewDestination()
    {
        if (npcState == NPCState_.Patrol)
        {
           NextPatrolDestination();
        }
        else
        {
            destination = new Vector2(Random.value * (Settings.PLAYFIELD_MAX_X - Settings.PLAYFIELD_MIN_X) + Settings.PLAYFIELD_MIN_X,
                Random.value * (Settings.PLAYFIELD_MAX_Z - Settings.PLAYFIELD_MIN_Z) + Settings.PLAYFIELD_MIN_Z);
        }
    }

    public void Hit(Vector3 hitPosition)
    {
        if (hasDied)
        {
            return;
        }

        timesHit++;

        // look at player
        Vector3 direction = hitPosition - transform.position;
        transform.rotation = Quaternion.LookRotation(direction, Vector3.up);

        if (timesHit > 2)
        {
            Die();
        }
        else
        {
            if (isFemale)
            {
                soundScreamFemale.Play();
            }
            else
            {   
                if(Random.value < .5)
                {
                    soundScreamMale.Play();
                }
                else
                {
                    soundScreamMale2.Play();
                }
            }
        }
    }

    public void Die()
    {
        hasDied = true;
        Progress.Instance.Kills++;
        positionBeforeDying = transform.position;
        timeLeftDying = TIME_BEFORE_DYING_PLAYER_IS_REMOVED;
        layer = Random.Range(1, 4);
        if (useRagdoll)
        {
            characterController.enabled = false;
            animator.enabled = false;
        }
        else
        {
            animator.SetLayerWeight(layer, 1);
            animator.Play("Fall", layer, 0);
        }
        if (isFemale)
        {
            soundDyingFemale.Play();
        }
        else
        {
            if (Random.value < .5)
            {
                soundDyingMale.Play();
            }
            else
            {
                soundDyingMale2.Play();
            }
        }
        npcState = NPCState_.Falling;
        playerLowerValue = 0;
    }

    private void Move()
    {
        if (npcState != NPCState_.Patrol && Time.time - timeLastDistanceMeasurement > 5)
        {
            timeLastDistanceMeasurement = Time.time;
            if ((storedPosition - transform.position).sqrMagnitude < 0.01f)
            {
                // stuck..
                timesStuck++;
                if (timesStuck > 10)
                {
                    Destroy(gameObject);
                }
                NewDestination();
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
            if (distanceToDestination.sqrMagnitude > 0.01f)
            {
                animator.SetFloat(animIDSpeed, 5f);
                distanceToDestination.Normalize();
                Vector3 direction = new(distanceToDestination.x, 0, distanceToDestination.y);
                transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
                if (characterController.enabled)
                {
                    characterController.Move(MOVE_SPEED * Time.deltaTime * direction);
                }
            }
            else
            {
                animator.SetFloat(animIDSpeed, 0);
                NewDestination();
            }
        }
    }


}
