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
        public float lateralSpeed = 5;
        public float yOffset = 0;
        public float distanceTravelled;
        public float maxDeltaX;
        public float endOffset = 10f;
        public Transform car;
        public float lateralLimit = 7f;
        public bool isAtTheEnd = false;

        [SerializeField] private InputActionProperty steer;
        [SerializeField] private InputActionProperty right;
        [SerializeField] private InputActionProperty left;
        [SerializeField] private InputActionProperty dashRight;
        [SerializeField] private InputActionProperty dashLeft;
        [SerializeField] private InputActionProperty powerup;

        private void OnEnable()
        {
            steer.action.Enable();
            right.action.Enable();
            left.action.Enable();
            dashRight.action.Enable();
            dashLeft.action.Enable();
            powerup.action.Enable();
        }

        public void SetPathCreator(PathCreator creator)
        {
            pathCreator = creator;
        }

        void FixedUpdate()
        {
            //float horizontalInput = Input.GetAxis("Right") - Input.GetAxis("Left");
            float steerValue = steer.action.ReadValue<float>();
            float horizontalInput = steerValue;

            if (horizontalInput != 0)
            {
                // Mover o carro lateralmente
                float deltaX = horizontalInput * lateralSpeed * Time.fixedDeltaTime;
                car.transform.Translate(Vector3.right * deltaX);
                // Limita o movimento lateral
                Vector3 carPosition = car.transform.localPosition;
                carPosition.x = Mathf.Clamp(carPosition.x, -lateralLimit, lateralLimit);
                car.transform.localPosition = carPosition;
            }
            
            if (pathCreator != null)
            {
                if (distanceTravelled >= pathCreator.path.length - endOffset && !isAtTheEnd)
                {
                    isAtTheEnd = true;
                    Invoke(nameof(ResetPath), 2);
                }
                distanceTravelled += speed * Time.fixedDeltaTime;
                transform.position = pathCreator.path.GetPointAtDistance(distanceTravelled, endOfPathInstruction) + new Vector3(0, yOffset, 0);
                transform.rotation = pathCreator.path.GetRotationAtDistance(distanceTravelled, endOfPathInstruction);
            }
        }

        public void ResetPath()
        {
            PathGenerator.instance.ResetPath();
        }

        public void ResetPosition()
        {
            isAtTheEnd = false;
            distanceTravelled = 0;
        }
    }
}