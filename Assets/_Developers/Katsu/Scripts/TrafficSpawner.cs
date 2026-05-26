using System;
using System.Collections;
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
        public int minSpawnDist = 15;
        public int maxSpawnDist = 45;
        public bool goSameDirection = false;
        private int direction = 1;

        // Distâncias de segurança para evitar spawn perto do início e fim
        public float startSafetyDistance = 30f;
        public float endSafetyDistance = 30f;
        // Distância de bloqueio próximo ao tunnel (evitar spawn antes do player ter visão)
        public float tunnelBlockDistance = 50f;
        private GameplayManager _gameplayManager;

        private void Awake()
        {
            _pathGenerator = GetComponent<PathGenerator>();
            _gameplayManager = FindObjectOfType<GameplayManager>();
        }

        private void Start()
        {
            goSameDirection = Random.Range(0, 2) < 0.5f;
            direction = Random.Range(0, 2) < 0.5f ? -1 : 1;

            // Aguardar o fim da introdução antes de spawnar traffic
            StartCoroutine(SpawnTrafficAfterIntro());
        }

        private IEnumerator SpawnTrafficAfterIntro()
        {
            Debug.Log("TrafficSpawner: Aguardando término da introdução...");

            // Aguardar a introdução terminar
            if (_gameplayManager != null)
            {
                while (_gameplayManager.isIntroPlaying)
                {
                    yield return null;
                }
                Debug.Log("TrafficSpawner: Introdução terminada! Iniciando spawn de traffic...");
            }
            else
            {
                Debug.LogWarning("TrafficSpawner: GameplayManager não encontrado! Iniciando spawn imediatamente...");
            }

            // Agora spawnar os carros de tráfego
            for (int i = 0; i < amountSpawn; i++)
            {
                SpawnTrafficCar(GetSafeSpawnDistance());
            }

            Debug.Log($"TrafficSpawner: {amountSpawn} carros spawned com sucesso!");
        }

        private float GetSafeSpawnDistance()
        {
            // Gerar distância de spawn com zonas de segurança
            float pathLength = _pathGenerator.pathCreatorInstance.path.length;
            float safeMinDist = startSafetyDistance;
            float safeMaxDist = pathLength - endSafetyDistance;

            // Garantir que há espaço suficiente para spawn
            if (safeMaxDist <= safeMinDist)
            {
                safeMaxDist = pathLength;
            }

            // Gerar distância aleatória dentro da zona segura
            float spawnDistance = Random.Range(safeMinDist, safeMaxDist);

            // Evitar spawn muito perto do tunnel (final do caminho)
            // Se o spawn ficar muito perto do final, afastar mais
            if (spawnDistance > pathLength - tunnelBlockDistance)
            {
                spawnDistance = pathLength - tunnelBlockDistance - 5f;

                // Se ainda não conseguir lugar seguro, tentar no início
                if (spawnDistance < safeMinDist)
                {
                    spawnDistance = safeMinDist + Random.Range(5f, 20f);
                }
            }

            return spawnDistance;
        }

        private void SpawnTrafficCar(float spawnDistance)
        {
            Vector3 spawnPosition = _pathGenerator.pathCreatorInstance.path.GetPointAtDistance(spawnDistance, EndOfPathInstruction.Reverse);
            TrafficCarFollowPath selectedTrafficCarPrefab = trafficCarPrefabs[Random.Range(0, trafficCarPrefabs.Length)];
            TrafficCarFollowPath trafficCarInstance = Instantiate(selectedTrafficCarPrefab, spawnPosition, Quaternion.identity);
            trafficCarInstance.Init(_pathGenerator.pathCreatorInstance,
                Random.Range(0f, _pathGenerator.pathCreatorInstance.path.length),
                (goSameDirection) ? direction : Random.Range(0, 2) < 0.5f ? -1 : 1);

        }
    }
}
