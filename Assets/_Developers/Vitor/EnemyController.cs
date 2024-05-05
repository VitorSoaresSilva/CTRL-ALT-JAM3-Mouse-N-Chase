using System;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

namespace _Developers.Vitor
{
    public class EnemyController : MonoBehaviour
    {
        [SerializeField] private NavMeshAgent navMeshAgent;
        [SerializeField] private Vector3 point;
        [SerializeField] private bool isMoving;
        [SerializeField] private GameObject[] pointsToMove;

        private void Awake()
        {
            navMeshAgent = GetComponent<NavMeshAgent>();
        }

        private void Start()
        {
        }

        private void Update()
        {
            if (isMoving)
            {
                if (navMeshAgent.remainingDistance < 10)
                {
                    isMoving = false;
                }
            }
            else
            {
                if (!MyRandomPoint(pointsToMove[Random.Range(0, pointsToMove.Length)].transform.position, 10, out point)) return;
                isMoving = true;
                navMeshAgent.SetDestination(point);
                navMeshAgent.enabled = true;
            }
            
        }
        public static bool MyRandomPoint(Vector3 center, float range, out Vector3 result)
        {
            for (int i = 0; i < 100; i++)
            {
                Vector3 randomPoint = center + Random.insideUnitSphere * range;
                NavMeshHit hit;
                if (NavMesh.SamplePosition(randomPoint, out hit, 1.0f, NavMesh.AllAreas))
                {
                    result = hit.position;
                    return true;
                }
            }
            result = Vector3.zero;
            return false;
        }
    }
}