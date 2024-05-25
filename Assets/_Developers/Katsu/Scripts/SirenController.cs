using UnityEngine;

public class SirenController : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioHighPassFilter highPassFilter;
    public AudioClip clip1;
    public AudioClip clip2;
    public Material sirenMaterial;
    public Material sirenMaterial2; // novo material
    public Light sirenLight;
    public Light sirenLight2; // nova luz
    public float lightBlinkInterval = 1f;
    public bool activateSiren = false;
    public bool activateAlternateLights = false; // nova variável booleana

    private bool isClip1Playing = true;
    private float emissionIntensity = 0f;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.spatialBlend = 1f;
        sirenMaterial.DisableKeyword("_EMISSION");
        if (sirenMaterial2 != null) // verifica se o segundo material está definido
        {
            sirenMaterial2.DisableKeyword("_EMISSION"); // desativar emissão para o novo material
        }
    }
    void Update()
    {
        if (activateSiren)
        {
            if (!audioSource.isPlaying)
            {
                if (isClip1Playing)
                {
                    audioSource.clip = clip2;
                }
                else
                {
                    audioSource.clip = clip1;
                }
                audioSource.Play();
                isClip1Playing = !isClip1Playing;
            }

            emissionIntensity = Mathf.PingPong(Time.time, lightBlinkInterval);

            if (activateAlternateLights && sirenLight2 != null && sirenMaterial2 != null)
            {
                // Se activateAlternateLights estiver ativado e a segunda luz e o segundo material estiverem definidos, as luzes piscarão alternadamente
                if (emissionIntensity < lightBlinkInterval / 2)
                {
                    sirenMaterial.EnableKeyword("_EMISSION");
                    sirenMaterial.SetColor("_EmissionColor", Color.red * (emissionIntensity + 1));
                    sirenLight.intensity = emissionIntensity * 2;

                    sirenMaterial2.SetColor("_EmissionColor", Color.black);
                    sirenMaterial2.DisableKeyword("_EMISSION");
                    sirenLight2.intensity = 0;
                }
                else
                {
                    sirenMaterial.SetColor("_EmissionColor", Color.black);
                    sirenMaterial.DisableKeyword("_EMISSION");
                    sirenLight.intensity = 0;

                    sirenMaterial2.EnableKeyword("_EMISSION");
                    sirenMaterial2.SetColor("_EmissionColor", Color.blue * (emissionIntensity + 1));
                    sirenLight2.intensity = emissionIntensity * 2;
                }
            }
            else
            {
                // Se activateAlternateLights estiver desativado ou a segunda luz/material não estiver definido, a primeira luz/material piscará
                sirenMaterial.EnableKeyword("_EMISSION");
                sirenMaterial.SetColor("_EmissionColor", Color.red * (emissionIntensity + 1));
                sirenLight.intensity = emissionIntensity * 2;

                // Se o segundo material estiver definido, ele será desativado
                if (sirenMaterial2 != null)
                {
                    sirenMaterial2.SetColor("_EmissionColor", Color.black);
                    sirenMaterial2.DisableKeyword("_EMISSION");
                }
                // Se a segunda luz estiver definida, ela será desativada
                if (sirenLight2 != null)
                {
                    sirenLight2.intensity = 0;
                }
            }
        }
        else
        {
            if (audioSource.isPlaying)
            {
                audioSource.Stop();
            }
            sirenMaterial.SetColor("_EmissionColor", Color.black);
            sirenMaterial.DisableKeyword("_EMISSION");
            sirenLight.intensity = 0;

            // Se o segundo material estiver definido, ele será desativado
            if (sirenMaterial2 != null)
            {
                sirenMaterial2.SetColor("_EmissionColor", Color.black);
                sirenMaterial2.DisableKeyword("_EMISSION");
            }
            // Se a segunda luz estiver definida, ela será desativada
            if (sirenLight2 != null)
            {
                sirenLight2.intensity = 0;
            }
        }
    }
}
