using System.Collections.Generic;
using UnityEngine;

public class Boat : MonoBehaviour
{
    [SerializeField] private List<Vector2> wayPoints = new();
    [SerializeField] private float Speed = 0.5f;
    [SerializeField] private float WobbleSpeed = 0.1f;
    [SerializeField] private float WobbleAmount = 10f;
    [SerializeField] private float RotationSpeed = 0.01f;
    Quaternion previousRotation;
    Vector2 destination;
    float wobbleX;
    int currentWayPointIndex;
    float timeCount = 0.0f;
    Vector3 direction = new Vector3(0,0,1);

    void Start()
    {
        NextWayPoint();
    }

    void FixedUpdate()
    {
        wobbleX += WobbleSpeed;
        if (wobbleX > WobbleAmount)
        {
            wobbleX = 0;
        }

        Vector2 distanceToDestination = destination - new Vector2(transform.position.x, transform.position.z);
        if (distanceToDestination.sqrMagnitude > 0.25f)
        {
            distanceToDestination.Normalize();
            direction = new Vector3(distanceToDestination.x, 0, distanceToDestination.y);
            transform.rotation = Quaternion.Lerp(previousRotation, Quaternion.LookRotation(direction, Vector3.up), timeCount);
            timeCount += RotationSpeed;
            if (wobbleX < WobbleAmount/2f)
            {
                transform.Rotate(new Vector3(wobbleX - WobbleAmount/4f, 90, 0));
            }
            else
            {
                transform.Rotate(new Vector3(WobbleAmount*3f/4f - wobbleX, 90, 0));
            }
            transform.position += direction * Speed;
        }
        else
        {
            NextWayPoint();
        }
    }

    private void NextWayPoint()
    {
        previousRotation = Quaternion.LookRotation(direction, Vector3.up);
        timeCount = 0;
        currentWayPointIndex++;
        if (currentWayPointIndex >= wayPoints.Count)
        {
            currentWayPointIndex = 0;
        }
        destination = wayPoints[currentWayPointIndex];
    }
}
