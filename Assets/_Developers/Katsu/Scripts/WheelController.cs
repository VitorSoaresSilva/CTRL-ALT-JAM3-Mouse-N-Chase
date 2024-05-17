using _Developers.Vitor;
using UnityEngine;
using UnityEngine.InputSystem;

public class WheelController : MonoBehaviour
{
    public CarFollowPath car;
    public bool isFrontWheel;
    public bool rotateClockwise;
    public float baseRotationSpeedMultiplier = 1f;
    public InputActionProperty turnLeftAction; 
    public InputActionProperty turnRightAction; 

    void OnEnable()
    {
        turnLeftAction.action.Enable();
        turnRightAction.action.Enable();
    }

    void OnDisable()
    {
        turnLeftAction.action.Disable();
        turnRightAction.action.Disable();
    }

    void Update()
    {
        // Faz a roda girar com base na velocidade do carro
        float rotationSpeedMultiplier = baseRotationSpeedMultiplier * car.speed;
        float rotationSpeed = rotationSpeedMultiplier * (rotateClockwise ? 1 : -1);
        transform.Rotate(rotationSpeed * Time.deltaTime, 0, 0);

        // Se esta é uma roda da frente, permite que ela vire
        if (isFrontWheel)
        {
            float turnAmount = turnRightAction.action.ReadValue<float>() - turnLeftAction.action.ReadValue<float>();
            Vector3 newRotation = transform.localEulerAngles;
            newRotation.y = turnAmount * 30f; // 30 graus é o ângulo máximo de virada, ajuste conforme necessário
            transform.localEulerAngles = newRotation;
        }
    }
}
