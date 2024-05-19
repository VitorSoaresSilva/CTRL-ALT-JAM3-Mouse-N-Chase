using _Developers.Vitor;
using UnityEngine;
using UnityEngine.SearchService;

public class CarDamage : MonoBehaviour
{
    public CarFollowPath car; // Refer�ncia ao script CarFollowPath do carro
    [Range(0, 100)] public float health = 100f; // Sa�de inicial do carro
    public string damageTag = "Obstacle"; // Tag dos objetos que causam dano ao carro

    private void Start()
    {
        if(car == null) car = GetComponentInParent<CarFollowPath>();
    }

    void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Entered damage coll");
        // Verifique se o carro colidiu com um objeto que causa dano
        if (collision.gameObject.CompareTag(damageTag))
        {
            // Calcule o dano com base na velocidade do carro
            float damage = car.speed;

            // Diminua a sa�de do carro
            health -= damage;

            // Atualize os pontos perdidos no CareerPoints
            CareerPoints.instance.RemovePoints((int)damage);

            // Verifique se o carro ainda tem sa�de
            if (health <= 0)
            {
                // O carro foi destru�do, fa�a algo aqui (por exemplo, terminar o jogo ou destruir o carro)
            }
        }
    }
}
