using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthPowerup : MonoBehaviour
{
    [Range(0, 100) ]public float heathBoost = 25f;
    private PlayerCar playerCar;

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
            //Debug.Log("Healing");
            // Acesse o script PlayerCar no objeto do jogador
            if (playerCar == null) playerCar = FindObjectOfType<PlayerCar>();

            if (playerCar != null)
            {
                playerCar.carDamage.health += Mathf.Clamp(playerCar.carDamage.health + heathBoost, 0, 100);
            }

            // Desative o objeto do power-up
            gameObject.SetActive(false);
        }
    }
}
