using UnityEngine;

public class CarController : MonoBehaviour
{
    public float speed = 10f; // Velocidade do carro
    public float lateralSpeed = 5f; // Velocidade lateral do carro
    public float rotationSpeed = 5f; // Velocidade de rotação do carro
    public float maxRoadDistance = 1.5f; // Distância máxima para considerar a estrada

    void Update()
    {
        // Movendo o carro para frente automaticamente
        transform.Translate(Vector3.forward * (speed * Time.deltaTime));

        // Obter a direção da estrada
        Vector3 roadDirection = GetRoadDirection();

        // Verificar entrada do jogador para movimento lateral
        float horizontalInput = Input.GetAxis("Right") - Input.GetAxis("Left");
        if (horizontalInput != 0)
        {
            // Mover o carro lateralmente
            float deltaX = horizontalInput * lateralSpeed * Time.deltaTime;
            transform.Translate(Vector3.right * deltaX);
        }

        // Rotação do carro para seguir a direção da estrada
        if (roadDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(Vector3.Cross(transform.right, roadDirection), Vector3.up);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    Vector3 GetRoadDirection()
    {
        // Lance um raio para baixo para detectar a direção da estrada
        RaycastHit hit;
        if (Physics.Raycast(transform.position, -transform.up, out hit, Mathf.Infinity))
        {
            if (hit.distance <= maxRoadDistance)
            {
                return hit.normal; // Use a normal da superfície como direção da estrada
            }
        }

        return Vector3.zero;
    }
}