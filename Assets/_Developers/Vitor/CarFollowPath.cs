using System;
using PathCreation;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _Developers.Vitor
{
    public class CarFollowPath : MonoBehaviour
    {
        public PathCreator pathCreator;
        public EndOfPathInstruction endOfPathInstruction;
        public float speed = 5;
        /// <summary>
        /// Temporary movement override for player powerups only.
        /// When &gt;= 0, player moves at this speed; AI enemies still read <see cref="speed"/>.
        /// </summary>
        public float powerupSpeedOverride = -1f;

        public float MoveSpeed => powerupSpeedOverride >= 0f ? powerupSpeedOverride : speed;

        public void SetPowerupSpeed(float overrideSpeed)
        {
            powerupSpeedOverride = overrideSpeed;
        }

        public void ClearPowerupSpeed()
        {
            powerupSpeedOverride = -1f;
        }
        public float lateralSpeed = 5;
        public float yOffset = 0;
        public float xOffset = 0;
        public float distanceTravelled;
        public float maxDeltaX;
        public float endOffset = 10f;
        public float startOffset = 5f;
        public Transform car;
        public float lateralLimit = 7f;
        public bool isAtTheEnd = false;
        public GameplayManager gameplayManager;

        [SerializeField] private InputActionProperty steer;
        [SerializeField] private InputActionProperty right;
        [SerializeField] private InputActionProperty left;
        [SerializeField] private InputActionProperty dashRight;
        [SerializeField] private InputActionProperty dashLeft;
        [SerializeField] private InputActionProperty powerup;

        private void OnEnable()
        {
            if(steer != null) steer.action.Enable();
            if(right != null) right.action.Enable();
            if(left != null) left.action.Enable();
            if(dashRight != null) dashRight.action.Enable();
            if(dashLeft != null) dashLeft.action.Enable();
            if(powerup != null) powerup.action.Enable();
            if(gameplayManager == null) gameplayManager = FindObjectOfType<GameplayManager>();
        }

        public void SetPathCreator(PathCreator creator)
        {
            pathCreator = creator;
        }

        void FixedUpdate()
        {
            //float horizontalInput = Input.GetAxis("Right") - Input.GetAxis("Left");
            float horizontalInput = 0;
            if (steer != null)
            {
                float steerValue = steer.action.ReadValue<float>();
                horizontalInput = steerValue;
            }

            if (horizontalInput != 0)
            {
                // Mover o carro lateralmente
                float deltaX = horizontalInput * lateralSpeed * Time.fixedDeltaTime;
                car.transform.Translate(Vector3.right * deltaX);
                // Limita o movimento lateral
                Vector3 carPosition = car.transform.localPosition;
                carPosition.x = Mathf.Clamp(carPosition.x, -lateralLimit, lateralLimit);
                car.transform.localPosition = carPosition;
            } else if(xOffset > 0)
            {
                Vector3 carPosition = car.transform.localPosition;
                carPosition.x = xOffset;
                car.transform.localPosition = carPosition;
            }
            
            if (pathCreator != null)
            {
                if (distanceTravelled >= pathCreator.path.length - endOffset && !isAtTheEnd)
                {
                    isAtTheEnd = true;
                    Invoke(nameof(ResetPath), 2);
                    gameplayManager.currentLap++;
                }

                distanceTravelled += MoveSpeed * Time.fixedDeltaTime;

                // Obter a nova posição do path
                Vector3 newPosition = pathCreator.path.GetPointAtDistance(distanceTravelled, endOfPathInstruction) + new Vector3(0, yOffset, 0);

                // Validar posição antes de aplicar
                if (ValidationUtility.IsValidVector3(newPosition))
                {
                    transform.position = newPosition;
                }
                else
                {
                    Debug.LogError($"[CarFollowPath] Posição inválida do path para carro player: {newPosition}. Resetando.");
                    ResetPosition();
                }

                // Obter nova rotação do path
                Quaternion newRotation = pathCreator.path.GetRotationAtDistance(distanceTravelled, endOfPathInstruction);

                // Validar rotação antes de aplicar
                if (ValidationUtility.IsValidQuaternion(newRotation))
                {
                    transform.rotation = newRotation;
                }
                else
                {
                    Debug.LogError($"[CarFollowPath] Rotação inválida do path: {newRotation}");
                }
            }
        }

        public void ResetPath()
        {
            PathGenerator.instance.ResetPath();
        }

        public void ResetPosition()
        {
            isAtTheEnd = false;
            //distanceTravelled = 0;
            distanceTravelled = startOffset;
        }
    }
}