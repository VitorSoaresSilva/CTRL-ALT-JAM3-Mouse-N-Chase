using UnityEngine;

public class SteeringWheelController : MonoBehaviour
{
    public WheelController frontLeftWheel; // Roda dianteira esquerda
    public WheelController frontRightWheel; // Roda dianteira direita
    public float maxSteeringAngle = 45f; // �ngulo m�ximo de virada do volante

    void Update()
    {
        // Obtenha a m�dia da rota��o das rodas frontais
        float averageWheelRotation = (frontLeftWheel.transform.localEulerAngles.y + frontRightWheel.transform.localEulerAngles.y) / 2f;

        // Calcule o novo �ngulo de rota��o para o volante
        float steeringAngle = Mathf.Clamp(averageWheelRotation, -maxSteeringAngle, maxSteeringAngle);

        // Aplique a rota��o ao volante
        transform.localRotation = Quaternion.Euler(0, 0, -steeringAngle);
    }
}
