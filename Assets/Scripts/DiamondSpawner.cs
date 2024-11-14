using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiamondSpawner : MonoBehaviour
{
    [SerializeField] GameObject[] pfSpawnObjects;
    [SerializeField] Transform rootTransform;
    [SerializeField] int initialItems = 0;
    [SerializeField] float minX = -1500;
    [SerializeField] float maxX = 1500;
    [SerializeField] float minZ = -1500;
    [SerializeField] float maxZ = 1500;

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < initialItems; i++)
        {
            SpawnNewObject();
        }
    }

    private void SpawnNewObject()
    {
        int objectIndex = Random.Range(0, pfSpawnObjects.Length);
        int triesLeft = 10;

        Vector3 spawnPosition;
        do
        {
            Vector3 spawnLocation;
            float x, y = 0, z;
            triesLeft--;
            x = Random.value * (maxX - minX) + minX;
            z = Random.value * (maxZ - minZ) + minZ;
            spawnLocation = new Vector3(x, 0, z);
            float terrainHeight;
            foreach (Terrain terrain in Terrain.activeTerrains)
            {
                terrainHeight = terrain.SampleHeight(spawnLocation);
                if (y < terrainHeight)
                {
                    y = terrainHeight;
                }
            }
            spawnPosition = new Vector3(x, y + 1, z);
        } while (!NoOtherObjectsNearby(spawnPosition) && triesLeft > 0);
        GameObject newObject = Instantiate(pfSpawnObjects[objectIndex], spawnPosition,
                                                Quaternion.Euler(new Vector3(-90, 0, 0)));

        newObject.transform.parent = rootTransform;
        Game.Instance.Gems.Add(newObject);
    }

    private bool NoOtherObjectsNearby(Vector3 position)
    {
        Collider[] colliders = Physics.OverlapSphere(position, 10);
        foreach (var collider in colliders)
        {
            if (!collider.gameObject.name.StartsWith("Ground"))
            {
                return false;
            }
        }

        return true;
    }
}
