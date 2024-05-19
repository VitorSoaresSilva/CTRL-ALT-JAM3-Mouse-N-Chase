using UnityEngine;

public class SirenController : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioHighPassFilter highPassFilter;
    public AudioClip clip1;
    public AudioClip clip2;
    public Material sirenMaterial;
    public GameObject sirenModel;
    public Light sirenLight;
    public float lightBlinkInterval = 1f;
    public bool activateSiren = false;

    private bool isClip1Playing = true;
    private float emissionIntensity = 0f;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.spatialBlend = 1f;
        highPassFilter = GetComponent<AudioHighPassFilter>();
        highPassFilter.cutoffFrequency = 1000;
        sirenMaterial.DisableKeyword("_EMISSION");
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

            sirenMaterial.EnableKeyword("_EMISSION");
            emissionIntensity = Mathf.PingPong(Time.time, lightBlinkInterval);
            sirenMaterial.SetColor("_EmissionColor", Color.red * (emissionIntensity + 1));
            sirenLight.intensity = emissionIntensity * 2;
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
        }
    }
}
