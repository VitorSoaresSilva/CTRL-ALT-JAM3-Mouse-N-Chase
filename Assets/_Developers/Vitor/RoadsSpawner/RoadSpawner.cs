using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _Developers.Vitor.RoadsSpawner
{
    public class RoadSpawner : MonoBehaviour
    {
        [SerializeField] private Road[] roads;
        private List<Road> _roadsSpawned = new List<Road>();
        private void Start()
        {
            SpawnRoads();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                UnSpawnRoads();
                SpawnRoads();
            }
        }

        private void UnSpawnRoads()
        {
            foreach (var road in _roadsSpawned)
            {
                Destroy(road.gameObject);
            }
            _roadsSpawned.Clear();
        }

        private void SpawnRoads()
        {
            Road firstRoad = SpawnRoad(Vector3.zero, quaternion.identity);
            _roadsSpawned.Add(firstRoad);
            Road lastRoad = firstRoad;
            for (int i = 0; i < 10; i++)
            {
                Road curRoad = SpawnRoad(lastRoad.endPosition.position, lastRoad.endPosition.localRotation);
                _roadsSpawned.Add(curRoad);
                lastRoad = curRoad;
            }
        }

        private Road SpawnRoad(Vector3 spawnPosition,Quaternion spawnRotation)
        {
            Road roadToSpawn = roads[Random.Range(0, roads.Length)];
            Road road = Instantiate(roadToSpawn, spawnPosition, spawnRotation);
            return road;
        }
    }
}