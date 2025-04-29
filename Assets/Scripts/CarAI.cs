using System.Collections.Generic;
using UnityEngine;

public class CarAI : MonoBehaviour
{
    [SerializeField] GameObject pfDriver;
    [SerializeField] Transform[] frontWheels;
    [SerializeField] Transform[] backWheels;
    [SerializeField] float wheelSpeed = 300f;
    [SerializeField] float maxSteerAngle = 30f; // Max steering angle
    [SerializeField] float rotationSpeed = 5f; // Adjust for smoothness
    [SerializeField] float maxSpeed = 20f;
    [SerializeField] float minSpeed = 1f;
    [SerializeField] float slowDownDistance = 12f; // Distance at which the car starts slowing down
    [SerializeField] int wheelRotationZ;     // The van's wheels are rotated :(
    [SerializeField] private List<GameObject> waypoints;
    private UnityEngine.AI.NavMeshAgent agent;
    private int currentWaypointIndex = 0;
    private Rigidbody rigidbody;
    [SerializeField] private bool aiActive = false;
    private float currentWheelRotation;
    private Transform player; 

    private void Awake()
    {
        rigidbody = GetComponent<Rigidbody>();
        agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        player = GameObject.Find("Player").transform;
    }

    public void SetWayPoints(List<GameObject> waypoints, int headingToWaypointIndex)
    {
        this.waypoints = waypoints;
        currentWaypointIndex = headingToWaypointIndex;
        NextWaypoint();
    }

    public void SetAIActive(bool aiActive)
    {
        this.aiActive = aiActive;
        if (aiActive)
        {
            agent.enabled = true;
            rigidbody.isKinematic = true;
        }
        else
        {
            agent.enabled = false;
            rigidbody.isKinematic = false;
        }
    }

    void Update()
    {
        if (!aiActive)
        {
            return;
        }

        // If close to the current destination, move to the next one
        if (!agent.pathPending && agent.remainingDistance < 1f)
        {
            NextWaypoint();
        }

        if (agent.velocity.sqrMagnitude > 0.01f) // Car is moving
        {
            currentWheelRotation += wheelSpeed * Time.deltaTime;

            // Get the steering angle based on movement direction
            Vector3 desiredDirection = agent.steeringTarget - transform.position;
            float steerAngle = Vector3.SignedAngle(transform.forward, desiredDirection, Vector3.up);
            steerAngle = Mathf.Clamp(steerAngle, -maxSteerAngle, maxSteerAngle);
            
            foreach (Transform backWheel in backWheels)
            {
                backWheel.localRotation = Quaternion.Euler(currentWheelRotation, 0, wheelRotationZ);
            }
            foreach (Transform frontWheel in frontWheels)
            {
                frontWheel.localRotation = Quaternion.Euler(currentWheelRotation, steerAngle, wheelRotationZ);
            }
        }

        AdjustSpeedBasedOnPlayer();
    }

    void AdjustSpeedBasedOnPlayer()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // Lerp speed between maxSpeed and minSpeed based on distance
        float newSpeed = Mathf.Lerp(minSpeed, maxSpeed, distanceToPlayer / slowDownDistance);
        agent.speed = Mathf.Clamp(newSpeed, minSpeed, maxSpeed);
    }

    void NextWaypoint()
    {
        if (waypoints.Count == 0) return;

        currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Count; // Loop back to first
        agent.SetDestination(waypoints[currentWaypointIndex].transform.position);
        if (agent.pathStatus == UnityEngine.AI.NavMeshPathStatus.PathInvalid)
        {
            Debug.Log("⚠ Path is INVALID!");
        }
    }
}
