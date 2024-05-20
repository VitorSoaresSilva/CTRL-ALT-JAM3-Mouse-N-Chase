using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Audio;

public class GameOverManager : MonoBehaviour
{
    public string carTag = "Player"; // Tag do carro
    public CarDamage car; // Refer�ncia ao script CarDamage do carro
    public Image gameOverImage; // Imagem de Game Over
    public AudioClip gameOverMusic; // M�sica de Game Over
    public AudioMixerGroup audioMixer; // AudioMixer para controlar o volume da m�sica
    public float delayBeforeMenu = 10f; // Atraso antes de voltar ao menu

    private AudioSource audioSource;

    void Start()
    {
        // Crie um novo AudioSource para tocar a m�sica de Game Over
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = gameOverMusic;
        audioSource.outputAudioMixerGroup = audioMixer;
        audioSource.Play();

        if(CareerPoints.instance != null)
        {
            CareerPoints.instance.ResetProgress();
        }

        Invoke(nameof(GoToMenu), delayBeforeMenu);
    }

    void GoToMenu()
    {
        Debug.Log("GoToMenu");
        SceneControl.instance.ChangeScene("MainMenu");
    }
}
