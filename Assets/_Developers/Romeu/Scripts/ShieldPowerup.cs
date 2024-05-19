using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEngine;

public class ShieldPowerup : MonoBehaviour
{
    private PlayerCar playerCar;
    public float duration = 5f;

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
                DoShield();
            }

            // Desative o objeto do power-up
            gameObject.SetActive(false);
        }
    }

    IEnumerator DoShield()
    {
        playerCar.carDamage.takeDamage = false;
        yield return new WaitForSeconds(duration);
        playerCar.carDamage.takeDamage = true;
    }
}