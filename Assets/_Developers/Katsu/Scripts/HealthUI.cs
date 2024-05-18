using UnityEngine;
using UnityEngine.UI;

public class HealthUI : MonoBehaviour
{
    public string carTag = "Player"; // Tag do carro
    private CarDamage car; // Referência ao script CarDamage do carro
    public Slider healthSlider; // Slider da UI para exibir a saúde

    void Start()
    {
        // Encontre o carro usando a tag
        GameObject carObject = GameObject.FindGameObjectWithTag(carTag);
        if (carObject != null)
        {
            car = carObject.GetComponent<CarDamage>();
        }
    }

    void Update()
    {
        // Atualize o valor do slider para corresponder à saúde do carro
        if (car != null)
        {
            healthSlider.value = car.health;
        }
    }
}
