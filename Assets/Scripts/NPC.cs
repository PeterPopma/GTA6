using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Unity.VisualScripting.Member;

public enum NPCState_
{
    WalkingAround,
    StandingStill,
    Patrol,
    FollowingPlayer,
    RandomAction,
    Farting,
    Shooting,
    KnockedOut,
    Dying,
    Talking
}

public class NPC : MonoBehaviour
{
    const int LAYER_SHOOT = 4;
    const int LAYER_FART = 6;
    const int TIME_BEFORE_DYING_PLAYER_IS_REMOVED = 300;
    const int PATROL_AREA_SIZE = 20;
    const int MAX_WALK_DISTANCE = 50;
    const float FART_LIKELINESS = 0.002f;
    float TALK_LIKELINESS = 0.2f;

    [SerializeField] bool isFemale;
    [SerializeField] NPCState_ initialState = NPCState_.WalkingAround;
    [SerializeField] private Transform hips;
    [SerializeField] private new Rigidbody rigidbody;
    [SerializeField] private GameObject handPosition;
    [SerializeField] private GameObject gunFirePistol;
    [SerializeField] private Transform vfxFireGun;
    [SerializeField] private GameObject pistol;

    private Transform followingPerson;
    private LayerMask layerMaskNPC;
    private CharacterController characterController;
    private Animator animator;
    private NPCState_ npcState, previousNPCState;
    private Vector3 storedPosition;
    private Player player;
    private int animIDSpeed;
    private int timesStuck;
    private float timeLastDistanceMeasurement;
    private float currentSpeed;
    private float walkingSpeed;
    private float timeLeftCurrentState;
    private Vector2 destination;
    private readonly List<Vector2> patrolRallyPoints = new();
    private int currentRallyPointIndex;
    private int timesHit;
    private bool pistolActive;
    private bool shotFired;
    private const int MAXIMUM_DISTANCE_TALKING = 100;
    private FaceAnimation faceAnimation;

    public NPCState_ NpcState { get => npcState; set => npcState = value; }

    void Awake()
    {
        layerMaskNPC = LayerMask.NameToLayer("NPC");
        animIDSpeed = Animator.StringToHash("Speed");
        animator = GetComponent<Animator>();
        characterController = GetComponent<CharacterController>();
        faceAnimation = GetComponent<FaceAnimation>();

        SetNPCState(initialState);

        player = GameObject.Find("Player").GetComponent<Player>();

        if (pistol != null)
        {
            pistolActive = true;
        }

        walkingSpeed = 2 + Random.value * 4;
        NewDestination();
    }

    private void SetNextRandomAction()
    {
        SetNPCState((NPCState_)Random.Range(0, 3), 10);
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

    private void FixedUpdate()
    {
        if (new[] { NPCState_.WalkingAround, NPCState_.Patrol, NPCState_.StandingStill }.Contains(npcState))
        {
            if (Random.value < TALK_LIKELINESS)
            {
                Collider[] colliders = Physics.OverlapSphere(transform.position, MAXIMUM_DISTANCE_TALKING, 1<<layerMaskNPC);
                foreach (var collider in colliders)
                {
                    NPC otherPlayerNPCScript = collider.gameObject.GetComponent<NPC>();
                    if (otherPlayerNPCScript != null && otherPlayerNPCScript != this)
                    {
                        if (new[] { NPCState_.WalkingAround, NPCState_.Patrol, NPCState_.StandingStill }.Contains(otherPlayerNPCScript.NpcState))
                        {
                            followingPerson = collider.transform;
                            SetNPCState(NPCState_.FollowingPlayer);
                            followingPerson.GetComponent<NPC>().SetNPCState(NPCState_.StandingStill);
                            break;
                        }
                    }
                }
            }
        }
    }

    public void StartConversationSecondPerson(bool otherIsFemale, int conversationNumber)
    {
        animator.SetFloat(animIDSpeed, 0);
        SetNPCState(NPCState_.Talking);
        string soundName = "";

        if (otherIsFemale && isFemale)
        {
            soundName = "ConversationFF1_2";
        }
        else if (otherIsFemale && !isFemale)
        {
            soundName = "ConversationFM" + conversationNumber + "_2";
        }
        else if (!otherIsFemale && isFemale)
        {
            soundName = "ConversationFM" + conversationNumber + "_1";
        }
        else if (!otherIsFemale && !isFemale)
        {
            soundName = "ConversationMM1_2";
        }
        GameObject soundObject = SoundManager.Instance.PlaySoundAt(soundName, transform.position);
        if (faceAnimation != null)
        {
            faceAnimation.SetAudioSource(soundObject.GetComponent<AudioSource>());
        }
        StartCoroutine(EndConversation(soundObject.GetComponent<AudioSource>().clip.length));
    }

    public void StartConversation()
    {
        animator.SetFloat(animIDSpeed, 0);
        // make other NPC look towards this NPC
        Vector3 directionToPerson = (transform.position - followingPerson.position).normalized;
        directionToPerson = new(directionToPerson.x, 0, directionToPerson.z);
        followingPerson.rotation = Quaternion.LookRotation(directionToPerson, Vector3.up);

        SetNPCState(NPCState_.Talking);
        string soundName = "";
        int conversationNumber = 1;

        if (isFemale && followingPerson.GetComponent<NPC>().isFemale)
        {
            soundName = "ConversationFF1_1";
        } 
        else if (isFemale && !followingPerson.GetComponent<NPC>().isFemale)
        {
            conversationNumber = Random.Range(1, 3);
            soundName = "ConversationFM" + conversationNumber + "_1";
        }
        else if (!isFemale && followingPerson.GetComponent<NPC>().isFemale)
        {
            conversationNumber = Random.Range(1, 3);
            soundName = "ConversationFM" + conversationNumber + "_2";
        }
        else if (!isFemale && !followingPerson.GetComponent<NPC>().isFemale)
        {
            soundName = "ConversationMM1_1";
        }

        GameObject soundObject = SoundManager.Instance.PlaySoundAt(soundName, transform.position);
        if (faceAnimation != null)
        {
            faceAnimation.SetAudioSource(soundObject.GetComponent<AudioSource>());
        }
        StartCoroutine(EndConversation(soundObject.GetComponent<AudioSource>().clip.length));

        followingPerson.GetComponent<NPC>().StartConversationSecondPerson(isFemale, conversationNumber);
    }

    private System.Collections.IEnumerator EndConversation(float delay)
    {
        yield return new WaitForSeconds(delay);
        TALK_LIKELINESS = 0.0002f;
        SetNPCState(initialState);
    }

    private void SetNPCState(NPCState_ newState, float stateDuration = 0)
    {
        previousNPCState = npcState;
        npcState = newState;
        if (previousNPCState.Equals(NPCState_.Farting))
        {
            SoundManager.Instance.PlaySoundAt("Fart", transform.position);
        }
        if (previousNPCState.Equals(NPCState_.FollowingPlayer))
        {
            if (followingPerson.GetComponent<NPC>() != null)
            {
                // NPC was following other NPC to talk to him
                StartConversation();
            }
        }
        if (npcState.Equals(NPCState_.Patrol))
        {
            SetRandomPatrolDestinations();
        }
        if (npcState.Equals(NPCState_.RandomAction))
        {
            SetNextRandomAction();
        }
        timeLeftCurrentState = stateDuration;
    }

    private void KnockedOutOVer()
    {
        float y = Game.Instance.MainTerrain.SampleHeight(hips.position);
        if (timesStuck < 10 && hips.position.y - y > 1)
        {
            // not on the ground yet
            SetNPCState(NPCState_.KnockedOut, 2f);
            timesStuck++;     // prevent getting stuck when there is something wrong with sampling the terrain height
        }
        else
        {
            if (timesHit > 2)
            {
                Die();
            }
            else
            {
                RiseAgain(y);
            }
        }
    }

    void Update()
    {
        if (timeLeftCurrentState > 0)
        {
            timeLeftCurrentState -= Time.deltaTime;
            if (timeLeftCurrentState < 0)
            {
                if (npcState.Equals(NPCState_.Dying))
                {
                    Game.Instance.NPCs.Remove(gameObject);
                    Destroy(gameObject);
                    return;
                }
                else if(npcState.Equals(NPCState_.KnockedOut))
                {
                    KnockedOutOVer();
                }
                else if (npcState.Equals(NPCState_.Shooting))
                {
                    animator.SetLayerWeight(LAYER_SHOOT, 0);
                }
                else
                {
                    SetNPCState(previousNPCState);
                }
            }
        }

        if (npcState.Equals(NPCState_.Shooting))
        {
            if (timeLeftCurrentState < 0.6 && !shotFired)
            {
                shotFired = true;
                Instantiate(vfxFireGun, handPosition.transform.position, Quaternion.identity);
                gunFirePistol.SetActive(true);
                if (Random.value > 0.5)
                {
                    player.Hit();
                }
            }
            if (timeLeftCurrentState < 0.5)
            {
                gunFirePistol.SetActive(false);
            }
        }
        else if (pistolActive)
        {
            Vector3 directionPlayer = player.transform.position - transform.position;
            if (directionPlayer.sqrMagnitude < 400)
            {
                SetNPCState(NPCState_.StandingStill);
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
                npcState = previousNPCState;
            }
        }

        if (!characterController.isGrounded && characterController.enabled /*&& npcState!=NPCState_.Falling*/)
        {
            // make sure characters stay on the ground
            characterController.Move(new Vector3(0.0f, -2f * Time.deltaTime, 0.0f));
        }

        if (npcState.Equals(NPCState_.Farting))
        {
            animator.SetLayerWeight(LAYER_FART, Mathf.Lerp(animator.GetLayerWeight(LAYER_FART), 1f, Time.deltaTime * 5f));
        }
        else 
        {
            animator.SetLayerWeight(LAYER_FART, Mathf.Lerp(animator.GetLayerWeight(LAYER_FART), 0f, Time.deltaTime * 5f));
            if (!new[] { NPCState_.Talking, NPCState_.FollowingPlayer }.Contains(npcState))
            {
                if (Random.value < FART_LIKELINESS)
                {
                    SetNPCState(NPCState_.Farting, 0.5f);
                }
            }
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
                FollowPerson();
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
        animator.Play("Shoot", LAYER_SHOOT, 0);
        animator.SetLayerWeight(LAYER_SHOOT, 1);
        shotFired = false;
        SoundManager.Instance.PlaySoundAt("Gunshot", transform.position);
        SetNPCState(NPCState_.Shooting, 0.7f);
    }

    private void FollowPerson()
    {
        Vector3 distanceToPerson = followingPerson.position - transform.position;
        if (distanceToPerson.sqrMagnitude > 1.21f)
        {
            if (currentSpeed < 4)
            {
                currentSpeed += Time.deltaTime * 8f;   
            }
            distanceToPerson.Normalize();
            Vector3 direction = new(distanceToPerson.x, 0, distanceToPerson.z);
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
                SetNPCState(previousNPCState);
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
        SetNPCState(NPCState_.KnockedOut, 4);
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
            SoundManager.Instance.PlaySoundAt("FemaleScream" + Random.Range(1, 3), transform.position);
        }
        else
        {
            SoundManager.Instance.PlaySoundAt("MaleScream" + Random.Range(1, 11), transform.position);
        }
    }

    public void Hit(Vector3 hitPosition)
    {
        if (npcState.Equals(NPCState_.Dying))
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
        Progress.Instance.Kills++;
        SetNPCState(NPCState_.Dying, TIME_BEFORE_DYING_PLAYER_IS_REMOVED);
        characterController.enabled = false;
        animator.enabled = false;
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

}
