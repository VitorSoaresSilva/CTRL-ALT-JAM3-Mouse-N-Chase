using _Developers.Vitor;
using System.Collections;
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

    private float damageCooldown = 0.3f; // Cooldown entre danos para evitar contagem dupla
    private float lastDamageTime = -999f;

    // Efeitos Cartoon
    [Header("Cartoon Effects")]
    [SerializeField] private bool enableCartoonEffects = true;
    [SerializeField] private float squashStretchDuration = 0.4f;
    [SerializeField] private float squashStretchIntensity = 0.3f;
    [SerializeField] private float shakeDuration = 0.5f;
    [SerializeField] private float shakeIntensity = 0.15f;
    [SerializeField] private float hitFreezeDuration = 0.05f;
    [SerializeField] private GameObject impactEffectPrefab;
    [SerializeField] private AudioClip hitSoundEffect;

    private Vector3 originalScale;
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private bool isPlayingEffect = false;
    private AudioSource audioSource;

    private void Start()
    {
        if(car == null) car = GetComponentInParent<CarFollowPath>();

        originalScale = transform.localScale;
        originalPosition = transform.localPosition;
        originalRotation = transform.localRotation;

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Entered damage coll");
        if (collision.gameObject.CompareTag(damageTag) && takeDamage)
        {
            if (Time.time - lastDamageTime < damageCooldown)
            {
                return;
            }
            lastDamageTime = Time.time;

            float damage = car.speed / 1.25f;
            health -= damage;

            CareerPoints.instance.RemovePoints((int)damage);

            // Reproduzir efeitos cartoon
            if (enableCartoonEffects && !isPlayingEffect)
            {
                StartCoroutine(PlayCartoonEffects(car.speed, collision.relativeVelocity));
            }

            // Invocar callback apenas se ele está registrado (proteção contra nulls)
            if (onDamage != null)
            {
                onDamage.Invoke();
            }
        }
    }

    private IEnumerator PlayCartoonEffects(float speed, Vector3 impactDirection)
    {
        isPlayingEffect = true;

        // Hit Freeze: pequena pausa no impacto
        float originalTimeScale = Time.timeScale;
        Time.timeScale = 0.1f;
        yield return new WaitForSecondsRealtime(hitFreezeDuration);
        Time.timeScale = originalTimeScale;

        // Calcular intensidade baseada na velocidade
        float intensityMultiplier = Mathf.Clamp01(speed / 20f);

        // Iniciar múltiplos efeitos em paralelo
        StartCoroutine(SquashAndStretchEffect(intensityMultiplier));
        StartCoroutine(ShakeEffect(intensityMultiplier));
        StartCoroutine(WheelWobbleEffect(intensityMultiplier));

        // Efeito de partículas
        if (impactEffectPrefab != null)
        {
            Instantiate(impactEffectPrefab, transform.position, Quaternion.identity);
        }

        // Som do impacto
        if (hitSoundEffect != null && audioSource != null)
        {
            audioSource.PlayOneShot(hitSoundEffect);
        }

        // Esperar pelos efeitos terminarem
        yield return new WaitForSeconds(Mathf.Max(squashStretchDuration, shakeDuration));

        isPlayingEffect = false;
    }

    private IEnumerator SquashAndStretchEffect(float intensity)
    {
        float elapsed = 0f;
        Vector3 squashScale = originalScale;

        // Fase 1: Compress (amassa)
        while (elapsed < squashStretchDuration * 0.5f)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / (squashStretchDuration * 0.5f);

            // Amassa na vertical, estica na horizontal
            float squashAmount = Mathf.Lerp(0, squashStretchIntensity * intensity, progress);
            squashScale.y = originalScale.y * (1f - squashAmount);
            squashScale.x = originalScale.x * (1f + squashAmount * 0.5f);
            squashScale.z = originalScale.z * (1f + squashAmount * 0.5f);

            transform.localScale = squashScale;
            yield return null;
        }

        // Fase 2: Volta ao normal com overshoot suave
        elapsed = squashStretchDuration * 0.5f;
        while (elapsed < squashStretchDuration)
        {
            elapsed += Time.deltaTime;
            float progress = (elapsed - squashStretchDuration * 0.5f) / (squashStretchDuration * 0.5f);

            squashScale = Vector3.Lerp(
                new Vector3(originalScale.x * (1f + squashStretchIntensity * intensity * 0.5f),
                           originalScale.y * (1f - squashStretchIntensity * intensity),
                           originalScale.z * (1f + squashStretchIntensity * intensity * 0.5f)),
                originalScale,
                progress
            );

            transform.localScale = squashScale;
            yield return null;
        }

        transform.localScale = originalScale;
    }

    private IEnumerator ShakeEffect(float intensity)
    {
        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            elapsed += Time.deltaTime;

            float shakeAmount = Mathf.Sin(elapsed * 50f) * shakeIntensity * intensity;
            Vector3 shakeOffset = new Vector3(
                Random.Range(-shakeAmount, shakeAmount),
                Random.Range(-shakeAmount, shakeAmount),
                Random.Range(-shakeAmount, shakeAmount)
            );

            transform.localPosition = originalPosition + shakeOffset;
            yield return null;
        }

        transform.localPosition = originalPosition;
    }

    private IEnumerator WheelWobbleEffect(float intensity)
    {
        // Procurar rodas do carro (assume que existem transforms filhas chamadas "Wheel")
        Transform[] wheels = GetComponentsInChildren<Transform>();

        float elapsed = 0f;
        Quaternion[] originalWheelRotations = new Quaternion[wheels.Length];

        for (int i = 0; i < wheels.Length; i++)
        {
            originalWheelRotations[i] = wheels[i].localRotation;
        }

        while (elapsed < shakeDuration)
        {
            elapsed += Time.deltaTime;

            for (int i = 0; i < wheels.Length; i++)
            {
                float wobble = Mathf.Sin(elapsed * 40f + i) * 15f * intensity;
                wheels[i].localRotation = originalWheelRotations[i] * Quaternion.AngleAxis(wobble, Vector3.right);
            }

            yield return null;
        }

        for (int i = 0; i < wheels.Length; i++)
        {
            wheels[i].localRotation = originalWheelRotations[i];
        }
    }
}
