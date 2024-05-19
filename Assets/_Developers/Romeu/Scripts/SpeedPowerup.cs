using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SpeedPowerUp : MonoBehaviour
{
    public float speedBoost = 45f;
    public float duration = 5f;
    private PlayerCar playerCar;
    float originalSpeed;

    void Start()
    {
        if (playerCar == null) playerCar = FindObjectOfType<PlayerCar>();
    }

    void OnTriggerEnter(Collider other)
    {
        //Debug.Log("SpeedPowerUp OnTriggerEnter");
        // Verifique se o objeto que entrou na colisão tem a tag "Player"
        if (other.gameObject.CompareTag("Player"))
        {
            //Debug.Log("Speeding");
            // Acesse o script PlayerCar no objeto do jogador
            if (playerCar == null) playerCar = FindObjectOfType<PlayerCar>();

            // Se o PlayerCar script existir no objeto do jogador
            if (playerCar != null)
            {
                // Inicie a corrotina SpeedBoost
                originalSpeed = playerCar.Follow.speed;
                if (originalSpeed == speedBoost) return;
                StartCoroutine(SpeedBoost(playerCar));
            }

            // Desative o objeto do power-up
            gameObject.SetActive(false);
        }
    }

    IEnumerator SpeedBoost(PlayerCar playerCar)
    {
        // Aumente a velocidade do jogador
        playerCar.Follow.speed = speedBoost;

        // Espere pela duração do power-up
        yield return new WaitForSeconds(duration);

        // Restaure a velocidade original do jogador
        playerCar.Follow.speed = originalSpeed;
    }
}
