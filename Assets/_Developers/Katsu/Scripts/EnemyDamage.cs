using _Developers.Vitor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDamage : MonoBehaviour
{
    EnemyCarFollowPath car; // Referência ao script EnemyCarFollowPath do carro
    [Range(0, 100)] public float health = 80f; // Saúde inicial
    public string damageTag = "Player"; // Tag dos objetos que causam dano ao carro
    public bool takeDamage = true;
    public GameObject dieParticle;
    public delegate void OnDamage();
    public OnDamage onDamage;

    public delegate void OnDie();
    public OnDie onDie;

    private int hitCount = 0; // Contador de hits do jogador
    private const int maxHits = 5; // Máximo de hits necessários para prender o inimigo

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

            // Diminua a saúde do carro
            health -= damage;

            // Incrementa contador de hits
            hitCount++;

            onDamage?.Invoke();

            // Verifique se atingiu 5 hits (inimigo preso)
            if (hitCount >= maxHits)
            {
                if(dieParticle != null) dieParticle.SetActive(true);
                onDie?.Invoke();
                // O carro foi preso, faça algo aqui (por exemplo, terminar o jogo ou destruir o carro)
            }
        }
    }
}
