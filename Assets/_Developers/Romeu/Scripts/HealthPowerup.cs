using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthPowerup : MonoBehaviour
{
    [Range(0, 100) ]public float heathBoost = 25f;


    void Start()
    {
        
    }

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
                playerCar.carDamage.health += Mathf.Clamp(playerCar.carDamage.health + heathBoost, 0, 100);
            }

            // Desative o objeto do power-up
            gameObject.SetActive(false);
        }
    }
}
