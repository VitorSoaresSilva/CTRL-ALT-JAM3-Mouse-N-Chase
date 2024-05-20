using System;
using PathCreation;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _Developers.Vitor
{
    public class TrafficSpawner : MonoBehaviour
    {
        public TrafficCarFollowPath[] trafficCarPrefabs;
        // public float[] spawnPoints;
        public float minXOffset = -5f;
        public float maxXOffset = 5f;
        private PathGenerator _pathGenerator;
        public int amountSpawn = 20;

        private void Awake()
        {
            _pathGenerator = GetComponent<PathGenerator>();
            
        }

        private void Start()
        {
            for (int i = 0; i < amountSpawn; i++)
            {
                SpawnTrafficCar(amountSpawn);
            }
        }

        private void SpawnTrafficCar(float spawnDistance)
        {
            Vector3 spawnPosition = _pathGenerator.pathCreatorInstance.path.GetPointAtDistance(spawnDistance);
            TrafficCarFollowPath selectedTrafficCarPrefab = trafficCarPrefabs[Random.Range(0, trafficCarPrefabs.Length)];
            TrafficCarFollowPath trafficCarInstance = Instantiate(selectedTrafficCarPrefab, spawnPosition, Quaternion.identity);
            trafficCarInstance.Init(_pathGenerator.pathCreatorInstance,
                Random.Range(0f, _pathGenerator.pathCreatorInstance.path.length),
                Random.Range(0, 2) < 0.5f ? -1 : 1);

        }
    }
}
