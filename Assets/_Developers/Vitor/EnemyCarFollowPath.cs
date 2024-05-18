using PathCreation;
using UnityEngine;

namespace _Developers.Vitor
{
    public class EnemyCarFollowPath : MonoBehaviour
    {
        public PathCreator pathCreator;
        public EndOfPathInstruction endOfPathInstruction;
        public float speed = 5;
        public float lateralSpeed = 5;
        public float yOffset = 0;
        public float distanceTravelled;
        public Transform car;
        
        // Variáveis para controle de movimentação aleatória
        private float horizontalInput = 0f;
        private float changeDirectionInterval = 2f; // Intervalo de tempo para mudar a direção
        private float lastDirectionChangeTime;
        public float lateralLimit = 7f;
        void Start() {
            if (pathCreator != null)
            {
                pathCreator.pathUpdated += OnPathChanged;
            }
        }
        
        void FixedUpdate()
        {
            if (Time.time - lastDirectionChangeTime > changeDirectionInterval)
            {
                horizontalInput = Random.Range(-1f, 1f);
                lastDirectionChangeTime = Time.time;
            }
            if (horizontalInput != 0)
            {
                float deltaX = horizontalInput * lateralSpeed * Time.fixedDeltaTime;
                car.transform.Translate(Vector3.right * deltaX);
                
                // Limita o movimento lateral
                Vector3 carPosition = car.transform.localPosition;
                carPosition.x = Mathf.Clamp(carPosition.x, -lateralLimit, lateralLimit);
                car.transform.localPosition = carPosition;
            }
            if (pathCreator != null)
            {
                distanceTravelled += speed * Time.fixedDeltaTime;
                transform.position = pathCreator.path.GetPointAtDistance(distanceTravelled, endOfPathInstruction) + new Vector3(0, yOffset, 0);
                transform.rotation = pathCreator.path.GetRotationAtDistance(distanceTravelled, endOfPathInstruction);
            }
        }
        
        void OnPathChanged() {
            distanceTravelled = pathCreator.path.GetClosestDistanceAlongPath(transform.position);
        }
    }
}