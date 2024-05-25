using _Developers.Vitor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDamage : MonoBehaviour
{
    EnemyCarFollowPath car; // Refer�ncia ao script EnemyCarFollowPath do carro
    [Range(0, 100)] public float health = 80f; // Sa�de inicial
    public string damageTag = "Player"; // Tag dos objetos que causam dano ao carro
    public bool takeDamage = true;

    public delegate void OnDamage();
    public OnDamage onDamage;

    public delegate void OnDie();
    public OnDie onDie;

    private void Start()
    {
        if (car == null) car = GetComponentInParent<EnemyCarFollowPath>();
    }

    void OnCollisionEnter(Collision collision)
    {
        //Debug.Log("Entered enemy damage coll");

        if (collision.gameObject.CompareTag(damageTag) && takeDamage)
        {
            // Calcule o dano com base na velocidade do carro
            float damage = car.speed;

            // Diminua a sa�de do carro
            health -= damage;

            onDamage?.Invoke();

            // Verifique se o carro ainda tem sa�de
            if (health <= 0)
            {
                onDie?.Invoke();
                // O carro foi destru�do, fa�a algo aqui (por exemplo, terminar o jogo ou destruir o carro)
            }
        }
    }
}
