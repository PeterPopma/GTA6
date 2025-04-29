using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Splines.ExtrusionShapes;

public class CarSpawner : MonoBehaviour
{
    [SerializeField] Transform spawnedCarsRoot;
    [SerializeField] Transform waypointListsRoot;
    [SerializeField] GameObject[] pfCars;
    [SerializeField] float minimumDistanceOtherCars = 2;
    [SerializeField] int[] weightFactors;
    [SerializeField] float secondsBetweenSpawns = 1;
    [SerializeField] int initialItems = 0;
    [SerializeField] int maxItems = 10;
    List<List<GameObject>> roads = new List<List<GameObject>>();
    int startFromWaypointIndex;
    float timeLastSpawn;

    // Start is called before the first frame update
    void Start()
    {
        timeLastSpawn = Time.time;

        foreach (Transform waypointList in waypointListsRoot)
        {
            List<GameObject> waypoints = new List<GameObject>();

            foreach (Transform waypoint in waypointList)
            {
                waypoints.Add(waypoint.gameObject);
            }

            roads.Add(waypoints);
        }

        for (int i = 0; i < initialItems; i++)
        {
            SpawnNewCar();
        }
    }

    private Vector3 SelectSpawnPosition(int selectedRoadIndex)
    {
        startFromWaypointIndex = Random.Range(0, roads[selectedRoadIndex].Count);
        int headingToWaypointIndex = startFromWaypointIndex + 1;
        if (headingToWaypointIndex >= roads[selectedRoadIndex].Count)
        {
            headingToWaypointIndex = 0;
        }
        float firstWaypointShare = Random.value;

        return Vector3.Lerp(roads[selectedRoadIndex][startFromWaypointIndex].transform.position, roads[selectedRoadIndex][headingToWaypointIndex].transform.position, firstWaypointShare);
    }

    private void SpawnNewCar()
    {
        int objectIndex = Random.Range(0, pfCars.Length);
        int triesLeft = 10;
        int selectedRoadIndex = Random.Range(0, roads.Count);

        Vector3 spawnPosition;
        do
        {
            triesLeft--;
            spawnPosition = SelectSpawnPosition(selectedRoadIndex);
        } while (!NoOtherCarsNearby(spawnPosition) && triesLeft > 0);
        GameObject newObject = Instantiate(pfCars[objectIndex], spawnPosition,
                                                Quaternion.identity);

        newObject.transform.parent = spawnedCarsRoot;
        newObject.GetComponent<CarAI>().SetWayPoints(roads[selectedRoadIndex], startFromWaypointIndex);
        newObject.GetComponent<CarAI>().SetAIActive(true);
        Game.Instance.Cars.Add(newObject);
    }

    void FixedUpdate()
    {
        if (secondsBetweenSpawns > 0 && 
            Time.time - timeLastSpawn > secondsBetweenSpawns && 
            Game.Instance.Cars.Count < maxItems)
        {
            SpawnNewCar();
            timeLastSpawn = Time.time;
        }
    }

    private bool NoOtherCarsNearby(Vector3 position)
    {
        foreach (GameObject car in Game.Instance.Cars)
        {
            if ((car.transform.position - position).magnitude < minimumDistanceOtherCars)
            {
                return false;
            }
        }

        return true;
    }
}
