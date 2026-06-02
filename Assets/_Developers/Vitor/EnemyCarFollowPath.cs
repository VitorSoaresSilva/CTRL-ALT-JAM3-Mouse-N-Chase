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
        public EnemyDamage damage;
        
        // Variáveis para controle de movimentação aleatória
        private float horizontalInput = 0f;
        private float changeDirectionInterval = 2f; // Intervalo de tempo para mudar a direção
        private float lastDirectionChangeTime;
        public float lateralLimit = 7f;
        public CarFollowPath player;
        public float minSpeedDifference = 0.5f; // Diferença mínima de velocidade quando próximo
        public float maxSpeedDifference = 3f; // Diferença máxima de velocidade quando distante
        public float minDistance = 5f; // Distância mínima para considerar a velocidade mínima
        public float maxDistance = 50f; // Distância máxima para considerar a velocidade máxima
        public float damageSpeedBoost = 5f; // Quantidade de aceleração ao sofrer dano
        public float boostDuration = 2f; // Duração do boost em segundos
        public float negativeOffsetAfterBoost = -1;
        private bool isBoosted = false;
        private float boostEndTime = 0f;

        // Boost aleatório
        public float randomBoostInterval = 5f; // Intervalo entre 4-6 segundos
        public float randomBoostChance = 0.3f; // 30% de chance de boost aleatório
        private float lastRandomBoostCheckTime = 0f;

        private void OnEnable()
        {
            if(damage == null) damage = GetComponentInChildren<EnemyDamage>();
        }

        void Start() {
            if (pathCreator != null)
            {
                pathCreator.pathUpdated += OnPathChanged;
            }

            // Conectar callback de dano para disparar boost ao sofrer colisão
            if (damage != null)
            {
                damage.onDamage += OnTakeDamage;
            }

            lastRandomBoostCheckTime = Time.time;
        }

        public void Init(CarFollowPath playerRef, PathCreator pathCreatorRef, float initialDistanceTravelled)
        {
            distanceTravelled = initialDistanceTravelled;
            player = playerRef;
            pathCreator = pathCreatorRef;
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

            // Verificar boost aleatório
            CheckRandomBoost();
        }

        void OnPathChanged() {
            distanceTravelled = 0;
            Debug.Log("Path Changed");
        }

        // Callback para dano - acelera o inimigo
        private void OnTakeDamage()
        {
            Boost();
        }

        // Verificar e aplicar boost aleatório periodicamente
        private void CheckRandomBoost()
        {
            if (Time.time - lastRandomBoostCheckTime > randomBoostInterval && !isBoosted)
            {
                // Sorteia intervalo aleatório (4-6 segundos)
                randomBoostInterval = Random.Range(4f, 6f);
                lastRandomBoostCheckTime = Time.time;

                // 30% de chance de fazer boost
                if (Random.value < randomBoostChance)
                {
                    Boost();
                }
            }
        }

        private void UpdateVelocity()
        {
            // float currentDistance = distanceTravelled - player.distanceTravelled;

            float distance = distanceTravelled - player.distanceTravelled;
            if (isBoosted && Time.time >= boostEndTime)
            {
                isBoosted = false;
            }
            
            if (!isBoosted)
            {
                // Calcula a diferença de velocidade com base na distância
                float speedDifference = Mathf.Lerp(maxSpeedDifference, minSpeedDifference, Mathf.InverseLerp(minDistance, maxDistance, distance));
                speed = player.speed - speedDifference;
                if (distance < negativeOffsetAfterBoost)
                {
                    Boost();
                }
            }
        }
        [ContextMenu("Take Damage")]
        public void Boost()
        {
            // Aumenta a velocidade temporariamente
            isBoosted = true;
            speed = player.speed + damageSpeedBoost;
            boostEndTime = Time.time + boostDuration;
        }
    }
}