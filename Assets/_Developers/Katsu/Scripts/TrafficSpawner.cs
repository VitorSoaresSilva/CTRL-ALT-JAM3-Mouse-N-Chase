using System;
using PathCreation;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _Developers.Vitor
{
    public class TrafficSpawner : MonoBehaviour
    {
        public CarFollowPath[] trafficCarPrefabs;
        public float[] spawnPoints;
        public float minXOffset = -5f;
        public float maxXOffset = 5f;
        private PathGenerator _pathGenerator;

        private void Awake()
        {
            _pathGenerator = GetComponent<PathGenerator>();
        }

        private void Start()
        {
            for (int i = 0; i < spawnPoints.Length; i++)
            {
                SpawnTrafficCar(spawnPoints[i]);
            }
        }

        private void SpawnTrafficCar(float spawnDistance)
        {
            Vector3 spawnPosition = _pathGenerator.pathCreatorInstance.path.GetPointAtDistance(spawnDistance);
            CarFollowPath selectedTrafficCarPrefab = trafficCarPrefabs[Random.Range(0, trafficCarPrefabs.Length)];
            CarFollowPath trafficCarInstance = Instantiate(selectedTrafficCarPrefab, spawnPosition, Quaternion.identity);
            trafficCarInstance.pathCreator = _pathGenerator.pathCreatorInstance;

            // Define aleatoriamente o deslocamento x do carro de tráfego
            float xOffset = Random.Range(minXOffset, maxXOffset);
            trafficCarInstance.transform.position += new Vector3(xOffset, 0, 0);
        }
    }
}
