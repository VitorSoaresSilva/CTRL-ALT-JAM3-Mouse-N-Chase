using UnityEngine;

public class SteeringWheelController : MonoBehaviour
{
    public WheelController frontLeftWheel; // Roda dianteira esquerda
    public WheelController frontRightWheel; // Roda dianteira direita
    public float maxSteeringAngle = 45f; // Ângulo máximo de virada do volante

    void Update()
    {
        // Obtenha a média da rotação das rodas frontais
        float averageWheelRotation = (frontLeftWheel.transform.localEulerAngles.y + frontRightWheel.transform.localEulerAngles.y) / 2f;

        // Calcule o novo ângulo de rotação para o volante
        float steeringAngle = Mathf.Clamp(averageWheelRotation, -maxSteeringAngle, maxSteeringAngle);

        // Aplique a rotação ao volante
        transform.localRotation = Quaternion.Euler(0, 0, -steeringAngle);
    }
}
