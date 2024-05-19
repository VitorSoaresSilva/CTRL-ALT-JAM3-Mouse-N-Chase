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
        public CarFollowPath player;

        public bool firstApproach = false;
        
        public float initialApproachDistance = 40;
        public float closeDistance = 15;
        public float closestDistance = 5;
        public float higherDistance = 20;
        
        public float velocityToIncreaseDistance = 24f;
        public float velocityToSlightlyDecreaseDistance = 19f;
        public float velocityToHighlyDecreaseDistance = 18f;
        public float playerVelocity = 22f;
        
        
        public float baseSpeed = 5f; // Velocidade base do inimigo
        public float minSpeedDifference = 0.5f; // Diferença mínima de velocidade quando próximo
        public float maxSpeedDifference = 3f; // Diferença máxima de velocidade quando distante
        public float minDistance = 5f; // Distância mínima para considerar a velocidade mínima
        public float maxDistance = 50f; // Distância máxima para considerar a velocidade máxima
        // private float enemySpeed;
        
        
        void Start() {
            if (pathCreator != null)
            {
                pathCreator.pathUpdated += OnPathChanged;
            }
        }

        public void Init(CarFollowPath playerRef, PathCreator pathCreatorRef, float initialDistanceTravelled)
        {
            distanceTravelled = initialDistanceTravelled;
            player = playerRef;
            pathCreator = pathCreatorRef;
            speed = velocityToHighlyDecreaseDistance;
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

            UpdateVelocity();
        }
        
        void OnPathChanged() {
            distanceTravelled = pathCreator.path.GetClosestDistanceAlongPath(transform.position);
        }

        private void UpdateVelocity()
        {
            // float currentDistance = distanceTravelled - player.distanceTravelled;

            float distance = distanceTravelled - player.distanceTravelled;

            // Calcula a diferença de velocidade com base na distância
            float speedDifference = Mathf.Lerp(minSpeedDifference, maxSpeedDifference, Mathf.InverseLerp(minDistance, maxDistance, distance));
            speed = player.speed - speedDifference;

            // Move o inimigo para frente com a velocidade calculada
            // transform.Translate(Vector3.forward * (speed * Time.deltaTime));

        }
    }
}