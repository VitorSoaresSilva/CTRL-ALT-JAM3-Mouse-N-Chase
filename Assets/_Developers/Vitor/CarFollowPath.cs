using PathCreation;
using UnityEngine;

namespace _Developers.Vitor
{
    public class CarFollowPath : MonoBehaviour
    {
        public PathCreator pathCreator;
        public EndOfPathInstruction endOfPathInstruction;
        public float speed = 5;
        public float lateralSpeed = 5;
        public float yOffset = 0;
        float distanceTravelled;
        public Transform car;
        void Start() {
            if (pathCreator != null)
            {
                // Subscribed to the pathUpdated event so that we're notified if the path changes during the game
                pathCreator.pathUpdated += OnPathChanged;
            }
        }
        void Update()
        {
            float horizontalInput = Input.GetAxis("Right") - Input.GetAxis("Left");
            if (horizontalInput != 0)
            {
                // Mover o carro lateralmente
                float deltaX = horizontalInput * lateralSpeed * Time.deltaTime;
                car.transform.Translate(Vector3.right * deltaX);
            }
            
            if (pathCreator != null)
            {
                distanceTravelled += speed * Time.deltaTime;
                transform.position = pathCreator.path.GetPointAtDistance(distanceTravelled, endOfPathInstruction) + new Vector3(0, yOffset, 0);
                transform.rotation = pathCreator.path.GetRotationAtDistance(distanceTravelled, endOfPathInstruction);
            }
        }
        
        void OnPathChanged() {
            distanceTravelled = pathCreator.path.GetClosestDistanceAlongPath(transform.position);
        }
    }
}