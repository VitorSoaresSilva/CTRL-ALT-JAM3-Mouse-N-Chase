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

        private float horizontalInput = 0f;
        private float changeDirectionInterval = 2f;
        private float lastDirectionChangeTime;
        public float lateralLimit = 7f;
        public CarFollowPath player;
        public float minSpeedDifference = 0.5f;
        public float maxSpeedDifference = 3f;
        public float minDistance = 5f;
        public float maxDistance = 50f;
        public float damageSpeedBoost = 5f;
        public float boostDuration = 2f;
        public float negativeOffsetAfterBoost = -1;
        private bool isBoosted = false;
        private float boostEndTime = 0f;

        public float randomBoostInterval = 5f;
        public float randomBoostChance = 0.3f;
        private float lastRandomBoostCheckTime = 0f;

        // Soft temporary modifiers (Rescue powerups / panic nudge)
        private float temporarySpeedOffset = 0f;
        private float temporaryOffsetEndTime = 0f;
        [SerializeField] private float panicNudgeOffset = 2.5f;
        [SerializeField] private float panicNudgeDuration = 1.5f;

        private void OnEnable()
        {
            if (damage == null) damage = GetComponentInChildren<EnemyDamage>();
        }

        void Start()
        {
            if (pathCreator != null)
            {
                pathCreator.pathUpdated += OnPathChanged;
            }

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

        /// <summary>
        /// Soft pacing so the hostage carrier stays near the player instead of racing away.
        /// </summary>
        public void ConfigureHostagePacing()
        {
            minSpeedDifference = 0.3f;
            maxSpeedDifference = 1.8f;
            minDistance = 8f;
            maxDistance = 55f;
            randomBoostChance = 0f;
            damageSpeedBoost = 1.5f;
            boostDuration = 1.2f;
            negativeOffsetAfterBoost = -8f;
            panicNudgeOffset = 3f;
            panicNudgeDuration = 1.5f;
        }

        /// <summary>
        /// Soft boost/slow that stacks on top of normal pace matching (does not use hard Boost).
        /// </summary>
        public void ApplyTemporarySpeedOffset(float offset, float duration)
        {
            temporarySpeedOffset = offset;
            temporaryOffsetEndTime = Time.time + duration;
            isBoosted = false;
        }

        /// <summary>
        /// Small pull-away when the player hits traffic during Rescue.
        /// </summary>
        public void ApplyPanicNudge()
        {
            ApplyTemporarySpeedOffset(panicNudgeOffset, panicNudgeDuration);
        }

        void FixedUpdate()
        {
            if (Time.time - lastDirectionChangeTime > changeDirectionInterval)
            {
                horizontalInput = Random.Range(-1f, 1f);
                lastDirectionChangeTime = Time.time;
            }
            if (horizontalInput != 0 && car != null)
            {
                float deltaX = horizontalInput * lateralSpeed * Time.fixedDeltaTime;
                car.transform.Translate(Vector3.right * deltaX);

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
            CheckRandomBoost();
        }

        void OnPathChanged()
        {
            // Path/túnel regenerou: player volta pro início — recoloca inimigo à frente
            if (player != null)
                distanceTravelled = player.distanceTravelled + 40f;
            else
                distanceTravelled = 50f;
        }

        private void OnTakeDamage()
        {
            Boost();
        }

        private void CheckRandomBoost()
        {
            if (randomBoostChance <= 0f)
                return;

            if (Time.time - lastRandomBoostCheckTime > randomBoostInterval && !isBoosted)
            {
                randomBoostInterval = Random.Range(4f, 6f);
                lastRandomBoostCheckTime = Time.time;

                if (Random.value < randomBoostChance)
                {
                    Boost();
                }
            }
        }

        private void UpdateVelocity()
        {
            if (player == null)
                return;

            if (Time.time >= temporaryOffsetEndTime)
                temporarySpeedOffset = 0f;

            float distance = distanceTravelled - player.distanceTravelled;
            if (isBoosted && Time.time >= boostEndTime)
            {
                isBoosted = false;
            }

            if (!isBoosted)
            {
                float speedDifference = Mathf.Lerp(maxSpeedDifference, minSpeedDifference, Mathf.InverseLerp(minDistance, maxDistance, distance));
                speed = player.speed - speedDifference + temporarySpeedOffset;
                speed = Mathf.Max(speed, 1f);

                if (distance < negativeOffsetAfterBoost)
                {
                    Boost();
                }
            }
            else
            {
                speed = player.speed + damageSpeedBoost + temporarySpeedOffset;
                speed = Mathf.Max(speed, 1f);
            }
        }

        [ContextMenu("Take Damage")]
        public void Boost()
        {
            isBoosted = true;
            speed = player != null ? player.speed + damageSpeedBoost : speed + damageSpeedBoost;
            boostEndTime = Time.time + boostDuration;
        }
    }
}
