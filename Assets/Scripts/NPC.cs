using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum NPCState_
{
    WalkingAround,
    StandingStill,
    Patrol,
    FollowingPlayer,
    Falling,
    Random
}

public class NPC : MonoBehaviour
{
    const int LAYER_SHOOT = 4;
    const int TIME_BEFORE_DYING_PLAYER_IS_REMOVED = 300;
    const int PATROL_AREA_SIZE = 20;
    const int MAX_WALK_DISTANCE = 50;

    [SerializeField] bool isFemale;
    [SerializeField] NPCState_ initialState = NPCState_.WalkingAround;
    [SerializeField] private Transform hips;
    [SerializeField] private new Rigidbody rigidbody;
    [SerializeField] private GameObject handPosition;
    [SerializeField] private GameObject gunFirePistol;
    [SerializeField] private Transform vfxFireGun;
    [SerializeField] private GameObject pistol;

    private AudioSource soundGunshot;
    private CharacterController characterController;
    private Animator animator;
    private List<AudioClip> clipsScreamMale = new List<AudioClip>();
    private List<AudioClip> clipsScreamFemale = new List<AudioClip>(); 
    private NPCState_ npcState, initialNPCState;
    private Vector3 storedPosition;
    private Player player;
    private int animIDSpeed;
    private int timesStuck;
    private float timeLastDistanceMeasurement;
    private float currentSpeed;
    private float walkingSpeed;
    private float timeLeftDying;
    private float timeLeftShooting;
    private float timeLeftKnockedOut;
    private float timeLeftCurrentState;
    private Vector2 destination;
    private readonly List<Vector2> patrolRallyPoints = new();
    private int currentRallyPointIndex;
    private bool hasDied;
    private int timesHit;
    private bool pistolActive;
    private bool shotFired;
    private int layerVehicle;

    public Vector2 Destination { get => destination; set => destination = value; }
    public NPCState_ NpcState { get => npcState; set => npcState = value; }
    public int TimesHit { get => timesHit; set => timesHit = value; }
    public bool HasDied { get => hasDied; set => hasDied = value; }

    void Awake()
    {
        layerVehicle = LayerMask.NameToLayer("Vehicle");
        animIDSpeed = Animator.StringToHash("Speed");
        animator = GetComponent<Animator>();
        characterController = GetComponent<CharacterController>();
        Transform soundsRoot = GameObject.Find("/Sound/FemaleScreams").transform;
        foreach (Transform item in soundsRoot)
        {
            AudioClip clip = item.gameObject.GetComponent<AudioSource>().clip;
            clipsScreamFemale.Add(clip);
        }
        soundsRoot = GameObject.Find("/Sound/MaleScreams").transform;
        foreach (Transform item in soundsRoot)
        {
            AudioClip clip = item.gameObject.GetComponent<AudioSource>().clip;
            clipsScreamMale.Add(clip);
        }
        soundGunshot = GameObject.Find("/Sound/Gunshot").GetComponent<AudioSource>();
    }

    private void Start()
    {
        initialNPCState = initialState;

        if (initialNPCState.Equals(NPCState_.Random))
        {
            SetNextRandomState();
        }

        initialNPCState = npcState = initialState;
        player = GameObject.Find("Player").GetComponent<Player>();

        if (pistol != null)
        {
            pistolActive = true;
        }
        if (npcState.Equals(NPCState_.Patrol))
        {
            SetRandomPatrolDestinations();
        }
        walkingSpeed = 2 + Random.value * 4;
        NewDestination();
    }

    private void SetNextRandomState()
    {
        npcState = (NPCState_)Random.Range(0, 3);
        timeLeftCurrentState = 10;
        if (npcState.Equals(NPCState_.Patrol))
        {
            // When walking around we don't need to update because the destination will be changed when reached.
            SetRandomPatrolDestinations();
        }
        walkingSpeed = 2 + Random.value * 4;
        UpdateAnimationSpeed();
    }

    private void UpdateAnimationSpeed()
    {
        if (npcState.Equals(NPCState_.StandingStill))
        {
            animator.SetFloat(animIDSpeed, 0);
        }
        else
        {
            animator.SetFloat(animIDSpeed, walkingSpeed);
        }
    }

    public void SetRandomPatrolDestinations()
    {
        patrolRallyPoints.Clear();
        patrolRallyPoints.Add(new Vector2(transform.position.x, transform.position.z));
        patrolRallyPoints.Add(new Vector2(transform.position.x - PATROL_AREA_SIZE/2 + Random.value * PATROL_AREA_SIZE, transform.position.z - PATROL_AREA_SIZE/2 + Random.value * PATROL_AREA_SIZE));
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
        if (initialNPCState.Equals(NPCState_.Random))
        {
            timeLeftCurrentState -= Time.deltaTime;
            if (timeLeftCurrentState < 0)
            {
                SetNextRandomState();
            }
        }

        if (hasDied)
        {
            timeLeftDying -= Time.deltaTime;

            if (timeLeftDying < 0)
            {
                Game.Instance.NPCs.Remove(gameObject);
                Destroy(gameObject);
            }

            return;
        }

        if (timeLeftKnockedOut > 0)
        {
            timeLeftKnockedOut -= Time.deltaTime;
            if (timeLeftKnockedOut < 0)
            {
                float y = Game.Instance.MainTerrain.SampleHeight(hips.position);
                if (timesStuck < 10 && hips.position.y - y > 1)
                {
                    // not on the ground yet
                    timeLeftKnockedOut = 2f;
                    timesStuck++;     // prevent getting stuck when there is something wrong with sampling the terrain height
                }
                else
                {
                    if (timesHit > 2 )
                    {
                        Die();
                    }
                    else
                    {
                        RiseAgain(y);
                    }
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
            if (timeLeftShooting < 0.6 && !shotFired)
            {
                shotFired = true;
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

    private void RiseAgain(float y)
    {
        transform.position = new Vector3(hips.position.x, y + 1, hips.position.z);
        characterController.enabled = true;
        animator.enabled = true;
        rigidbody.isKinematic = false;
    }

    private void FirePistol()
    {
        timeLeftShooting = 0.7f;
        animator.Play("Shoot", LAYER_SHOOT, 0);
        animator.SetLayerWeight(LAYER_SHOOT, 1);
        shotFired = false;
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
            characterController.Move(2 * walkingSpeed * Time.deltaTime * newDirection);
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

    public void BlastImpact(GameObject otherObject, float multiplyFactor = 1)
    {
        Scream();
        characterController.enabled = false;
        animator.enabled = false;
        rigidbody.isKinematic = false;

        Vector3 forceDirection = (otherObject.transform.position - transform.position).normalized;
        forceDirection = new Vector3(forceDirection.x, 12, forceDirection.z) * multiplyFactor;
        rigidbody.AddForce(forceDirection, ForceMode.VelocityChange);
        rigidbody.AddTorque(Random.insideUnitSphere, ForceMode.VelocityChange);

        timesHit++;
        timeLeftKnockedOut = 4;
    }

    private void NewDestination()
    {
        if (npcState == NPCState_.Patrol)
        {
           NextPatrolDestination();
        }
        else
        {
            destination = new Vector2(transform.position.x - MAX_WALK_DISTANCE / 2 + Random.value * MAX_WALK_DISTANCE, 
                transform.position.z - MAX_WALK_DISTANCE / 2 + Random.value * MAX_WALK_DISTANCE);
        }
    }

    private void Scream()
    {
        if (isFemale)
        {
            AudioSource.PlayClipAtPoint(clipsScreamFemale[Random.Range(0, clipsScreamFemale.Count)], transform.position);
        }
        else
        {
            AudioSource.PlayClipAtPoint(clipsScreamMale[Random.Range(0, clipsScreamMale.Count)], transform.position);
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

        Scream();

        if (timesHit > 2)
        {
            Die();
        }
    }

    public void Die()
    {
        hasDied = true;
        Progress.Instance.Kills++;
        timeLeftDying = TIME_BEFORE_DYING_PLAYER_IS_REMOVED;
        characterController.enabled = false;
        animator.enabled = false;

        npcState = NPCState_.Falling;
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
                animator.SetFloat(animIDSpeed, walkingSpeed);
                distanceToDestination.Normalize();
                Vector3 direction = new(distanceToDestination.x, 0, distanceToDestination.y);
                transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
                if (characterController.enabled)
                {
                    characterController.Move(walkingSpeed * Time.deltaTime * direction);
                }
            }
            else
            {
                animator.SetFloat(animIDSpeed, 0);
                NewDestination();
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponent<Car>() != null)
        {
            BlastImpact(other.gameObject, 2.5f + player.GetComponent<CarController>().Speed * 0.1f);
        }
        if (other.gameObject.GetComponent<Motorbike>() != null)
        {
            BlastImpact(other.gameObject, 2.5f + player.GetComponent<MotorbikeController>().Speed * 0.1f);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        /*
        if (collision.collider.gameObject.layer == layerVehicle)
        {
            rigidbody.constraints = RigidbodyConstraints.None;
            Scream();
        }*/
    }

}
