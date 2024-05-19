using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SpeedPowerUp : MonoBehaviour
{
    public float speedBoost = 45f;
    public float duration = 5f;

    void OnTriggerEnter(Collider other)
    {
        // Verifique se o objeto que entrou na colisão tem a tag "Player"
        if (other.gameObject.CompareTag("Player"))
        {
            // Acesse o script PlayerCar no objeto do jogador
            PlayerCar playerCar = other.gameObject.GetComponent<PlayerCar>();

            // Se o PlayerCar script existir no objeto do jogador
            if (playerCar != null)
            {
                // Inicie a corrotina SpeedBoost
                StartCoroutine(SpeedBoost(playerCar));
            }

            // Desative o objeto do power-up
            gameObject.SetActive(false);
        }
    }

    IEnumerator SpeedBoost(PlayerCar playerCar)
    {
        // Salve a velocidade original do jogador
        float originalSpeed = playerCar.Follow.speed;

        // Aumente a velocidade do jogador
        playerCar.Follow.speed = speedBoost;

        // Espere pela duração do power-up
        yield return new WaitForSeconds(duration);

        // Restaure a velocidade original do jogador
        playerCar.Follow.speed = originalSpeed;
    }
}
