using _Developers.Vitor;
using System.Collections;
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
    public GameObject skidEffectPrefab; // A prefab do efeito de derrapagem
    private GameObject currentSkidEffect; // O efeito de derrapagem atualmente ativo

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

            // Se o jogador está virando, inicia o efeito de derrapagem
            if (turnAmount != 0 && currentSkidEffect == null)
            {
                StartSkidEffect();
            }
            // Se o jogador parou de virar, para o efeito de derrapagem
            else if (turnAmount == 0 && currentSkidEffect != null)
            {
                StopSkidEffect();
            }
        }
    }

    void StartSkidEffect()
    {
        // Cria o efeito de derrapagem atrás do pneu
        Vector3 spawnPosition = transform.position - transform.forward;
        currentSkidEffect = Instantiate(skidEffectPrefab, spawnPosition, Quaternion.identity);

        // Ativa o efeito de derrapagem
        currentSkidEffect.SetActive(true);
    }

    void StopSkidEffect()
    {
        // Destroi o efeito de derrapagem
        Destroy(currentSkidEffect);
        currentSkidEffect = null;
    }
}
