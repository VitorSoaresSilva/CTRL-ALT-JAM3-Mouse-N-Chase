using _Developers.Vitor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SpeedPowerUp : MonoBehaviour
{
    public float speedBoost = 70f;
    public float duration = 5f;
    private PlayerCar playerCar;
    float originalSpeed;

    // Sistema de acúmulo com bônus progressivos
    private static int speedPowerUpCollectCount = 0; // Contador estático de coletas
    private static float maxSpeedLimit = 120f; // Limite máximo de velocidade
    private static float bonusIncrement = 5f; // Aumento de velocidade por coleta adicional

    // Referência para rastrear boosts ativos
    private static Coroutine activeBoostCoroutine = null;

    void Start()
    {
        if (playerCar == null) playerCar = FindObjectOfType<PlayerCar>();
        //enemies.Clear();
        //enemies.AddRange(FindObjectsByType<EnemyCarFollowPath>(FindObjectsSortMode.None));
    }

    void OnTriggerEnter(Collider other)
    {
        // Verifique se o objeto que entrou na colisão tem a tag "Player"
        if (other.gameObject.CompareTag("Player"))
        {
            // Acesse o script PlayerCar no objeto do jogador
            if (playerCar == null) playerCar = FindObjectOfType<PlayerCar>();

            // Se o PlayerCar script existir no objeto do jogador
            if (playerCar != null)
            {
                // Calcular velocidade do boost com bônus progressivo
                float currentBoost = speedBoost;
                if (speedPowerUpCollectCount > 0)
                {
                    // Cada coleta adicional adiciona um bônus pequeno
                    float bonusAmount = bonusIncrement * speedPowerUpCollectCount;
                    currentBoost = Mathf.Min(speedBoost + bonusAmount, maxSpeedLimit);
                }

                // Inicie a corrotina SpeedBoost
                originalSpeed = playerCar.Follow.speed;

                // Parar boost anterior se houver
                if (activeBoostCoroutine != null)
                {
                    StopCoroutine(activeBoostCoroutine);
                }

                activeBoostCoroutine = StartCoroutine(SpeedBoost(playerCar, currentBoost));
                speedPowerUpCollectCount++;

                // Adicionar pontos por pegar o powerup
                if (CareerPoints.instance != null)
                {
                    CareerPoints.instance.AddPoints(100);
                    Debug.Log($"Speed PowerUp collected! +100 points. Total collected: {speedPowerUpCollectCount}. Boost: {currentBoost}");
                }
            }

            // Desative o objeto do power-up
            gameObject.SetActive(false);
        }
    }

    IEnumerator SpeedBoost(PlayerCar playerCar, float boostAmount)
    {
        // Aumente a velocidade do jogador
        playerCar.Follow.speed = boostAmount;

        // Espere pela duração do power-up
        yield return new WaitForSeconds(duration);

        // Restaure a velocidade original do jogador
        playerCar.Follow.speed = originalSpeed;

        activeBoostCoroutine = null;
    }

    // Método estático para resetar contador entre fases
    public static void ResetSpeedPowerUpCounter()
    {
        speedPowerUpCollectCount = 0;
    }

    // Método estático para obter o contador de coletas
    public static int GetSpeedPowerUpCollectCount()
    {
        return speedPowerUpCollectCount;
    }
}
