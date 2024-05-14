using UnityEngine;

public class ObjectSpawner : MonoBehaviour
{
    public GameObject[] objectPrefabs;
    public Transform[] spawnPoints;
    public float spawnDistance = 50f;

    private GameObject[] spawnedObjects;
    private Vector3[] lastSpawnedPositions;
    private Transform cameraTransform;

    void Start()
    {
        cameraTransform = Camera.main.transform;
        spawnedObjects = new GameObject[spawnPoints.Length];
        lastSpawnedPositions = new Vector3[spawnPoints.Length];
        for (int i = 0; i < spawnPoints.Length; i++)
        {
            SpawnRandomObject(i);
        }
    }

    void Update()
    {
        for (int i = 0; i < spawnPoints.Length; i++)
        {
            if (Vector3.Distance(cameraTransform.position, lastSpawnedPositions[i]) > spawnDistance)
            {
                if (spawnedObjects[i] != null)
                {
                    Destroy(spawnedObjects[i]);
                }
                SpawnRandomObject(i);
            }
        }
    }

    void SpawnRandomObject(int index)
    {
        int prefabIndex = Random.Range(0, objectPrefabs.Length);
        GameObject prefab = objectPrefabs[prefabIndex];
        spawnedObjects[index] = Instantiate(prefab, spawnPoints[index].position, Quaternion.identity);
        spawnedObjects[index].transform.position = new Vector3(spawnedObjects[index].transform.position.x, Terrain.activeTerrain.SampleHeight(spawnedObjects[index].transform.position), spawnedObjects[index].transform.position.z);
        spawnedObjects[index].transform.rotation = Quaternion.Euler(-90, 0, 0);
        lastSpawnedPositions[index] = cameraTransform.position;
    }
}
