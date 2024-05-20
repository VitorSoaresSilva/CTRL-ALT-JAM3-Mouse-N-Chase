using PathCreation;
using UnityEngine;

namespace _Developers.Vitor
{
    public class TrafficCarFollowPath : MonoBehaviour
    {
        public PathCreator pathCreator;
        public EndOfPathInstruction endOfPathInstruction;
        public float speed = 5;
        public float lateralSpeed = 5;
        public float yOffset = 0;
        public float distanceTravelled;
        public Transform movingPart;
        public Transform visual;
        
        // Variáveis para controle de movimentação aleatória
        private float horizontalInput = 0f;
        private float changeDirectionInterval = 8f; // Intervalo de tempo para mudar a direção
        private float lastDirectionChangeTime;
        public float lateralLimit = 7f;
        private bool isBoosted = false;
        private float boostEndTime = 0f;
        private float _direction = 1;
        public bool changeDirection = false;
        public float minXOffset = -5f;
        public float maxXOffset = 5f;
        void Start() {
            if (pathCreator != null)
            {
                pathCreator.pathUpdated += OnPathChanged;
            }
        }

        public void Init(PathCreator pathCreatorRef, float initialDistanceTravelled, int direction)
        {
            distanceTravelled = initialDistanceTravelled;
            pathCreator = pathCreatorRef;
            _direction = direction;
            float xOffset = Random.Range(minXOffset, maxXOffset);
            movingPart.transform.localPosition += new Vector3(xOffset, 0, 0);

            if (direction == -1)
            {
                visual.Rotate(Vector3.up, 180f);
                movingPart.Rotate(Vector3.up, 180f);
            }
        }
        
        void FixedUpdate()
        {
            if (changeDirection)
            {
                if (Time.time - lastDirectionChangeTime > changeDirectionInterval)
                {
                    horizontalInput = Random.Range(-1f, 1f);
                    lastDirectionChangeTime = Time.time;
                }
            }
            if (horizontalInput != 0)
            {
                float deltaX = horizontalInput * lateralSpeed * Time.fixedDeltaTime;
                movingPart.transform.Translate(Vector3.right * deltaX);
                
                // Limita o movimento lateral
                Vector3 carPosition = movingPart.transform.localPosition;
                carPosition.x = Mathf.Clamp(carPosition.x, -lateralLimit, lateralLimit);
                movingPart.transform.localPosition = carPosition;
            }
            if (pathCreator != null)
            {
                distanceTravelled += speed * _direction * Time.fixedDeltaTime;
                transform.position = pathCreator.path.GetPointAtDistance(distanceTravelled, endOfPathInstruction) + new Vector3(0, yOffset, 0);
                transform.rotation = pathCreator.path.GetRotationAtDistance(distanceTravelled, endOfPathInstruction);
            }
        }
        
        void OnPathChanged() {
            // distanceTravelled = pathCreator.path.GetClosestDistanceAlongPath(transform.position);
        }
    }
}