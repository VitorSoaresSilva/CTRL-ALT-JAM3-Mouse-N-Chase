using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Audio;

public class GameOverManager : MonoBehaviour
{
    public string carTag = "Player"; // Tag do carro
    private CarDamage car; // Referência ao script CarDamage do carro
    public Image gameOverImage; // Imagem de Game Over
    public AudioClip gameOverMusic; // Música de Game Over
    public AudioMixerGroup audioMixer; // AudioMixer para controlar o volume da música
    public float delayBeforeMenu = 10f; // Atraso antes de voltar ao menu

    private AudioSource audioSource;

    void Start()
    {
        // Encontre o carro usando a tag
        GameObject carObject = GameObject.FindGameObjectWithTag(carTag);
        if (carObject != null)
        {
            car = carObject.GetComponent<CarDamage>();
        }

        // Crie um novo AudioSource para tocar a música de Game Over
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = gameOverMusic;
        audioSource.outputAudioMixerGroup = audioMixer;
    }

    void Update()
    {
        // Verifique se a saúde do carro é 0 ou menos, ou se os pontos são 0 ou menos
        if (car != null && (car.health <= 0 || CareerPoints.Instance.Points <= 0))
        {
            // O jogo acabou, faça algo aqui (por exemplo, exibir uma tela de Game Over)
            gameOverImage.enabled = true; // Mostre a imagem de Game Over
            audioSource.Play(); // Toque a música de Game Over

            // Reset os pontos
            CareerPoints.Instance.SetPoints(0);

            // Aguarde alguns segundos e depois volte para o menu
            Invoke("GoToMenu", delayBeforeMenu);
        }
    }

    void GoToMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
