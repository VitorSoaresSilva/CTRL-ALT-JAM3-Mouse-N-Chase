using _Developers.Vitor;
using UnityEngine;
using UnityEngine.SearchService;

public class CarDamage : MonoBehaviour
{
    public CarFollowPath car; // Referência ao script CarFollowPath do carro
    [Range(0, 100)] public float health = 80f; // Saúde inicial do carro
    public string damageTag = "Obstacle"; // Tag dos objetos que causam dano ao carro
    public bool takeDamage = true;

    public delegate void OnDamage();
    public OnDamage onDamage;

    private void Start()
    {
        if(car == null) car = GetComponentInParent<CarFollowPath>();
    }

    void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Entered damage coll");
        // Verifique se o carro colidiu com um objeto que causa dano
        if (collision.gameObject.CompareTag(damageTag) && takeDamage)
        {
            // Calcule o dano com base na velocidade do carro
            float damage = car.speed / 1.25f;

            // Diminua a saúde do carro
            health -= damage;

            // Atualize os pontos perdidos no CareerPoints
            CareerPoints.instance.RemovePoints((int)damage);

            onDamage?.Invoke();
        }
    }
}
