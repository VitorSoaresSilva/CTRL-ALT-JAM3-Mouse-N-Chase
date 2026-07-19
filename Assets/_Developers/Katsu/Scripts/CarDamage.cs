using _Developers.Vitor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    [SerializeField] private float squashStretchDuration = 0.45f;
    [SerializeField] private float squashStretchIntensity = 0.55f;
    [SerializeField] private float shakeDuration = 0.4f;
    [SerializeField] private float shakeIntensity = 0.12f;
    [SerializeField] private float hitFreezeDuration = 0.03f;
    [SerializeField] private float cartoonSpinDegrees = 25f;
    [SerializeField] private GameObject impactEffectPrefab;
    [SerializeField] private GameObject dustCoverVfxPrefab;
    [SerializeField] private AudioClip hitSoundEffect;

    private Vector3 originalScale;
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private bool isPlayingEffect = false;
    private bool hasCachedTransform = false;
    private AudioSource audioSource;

    private void Start()
    {
        if (car == null) car = GetComponentInParent<CarFollowPath>();

        CacheOriginalTransform();

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    private void OnDisable()
    {
        RestoreTransform();
        if (Time.timeScale < 1f)
            Time.timeScale = 1f;
        isPlayingEffect = false;
    }

    private void CacheOriginalTransform()
    {
        originalScale = transform.localScale;
        originalPosition = transform.localPosition;
        originalRotation = transform.localRotation;
        hasCachedTransform = true;
    }

    private void RestoreTransform()
    {
        if (!hasCachedTransform)
            return;

        transform.localScale = originalScale;
        transform.localPosition = originalPosition;
        transform.localRotation = originalRotation;
    }

    void OnCollisionEnter(Collision collision)
    {
        // Hit de inimigo: sem dano e sem squash — VFX fica no EnemyDamage
        if (collision.gameObject.GetComponentInParent<EnemyDamage>() != null
            || collision.gameObject.GetComponentInParent<EnemyCarFollowPath>() != null)
        {
            return;
        }

        Debug.Log("Entered damage coll");
        if (collision.gameObject.CompareTag(damageTag) && takeDamage)
        {
            if (Time.time - lastDamageTime < damageCooldown)
            {
                return;
            }
            lastDamageTime = Time.time;

            float damage = car.speed / 1.25f;

            if (CareerPoints.instance != null && CareerPoints.instance.BumperUnlockedPermanent)
            {
                float damageReduction = CareerPoints.instance.BumperDamageReduction / 100f;
                damage *= (1f - damageReduction);
            }

            health -= damage;

            if (CareerPoints.instance != null)
                CareerPoints.instance.RemovePoints((int)damage);

            if (enableCartoonEffects && !isPlayingEffect)
            {
                StartCoroutine(PlayCartoonEffects(car.speed, collision.relativeVelocity));
            }

            if (onDamage != null)
            {
                onDamage.Invoke();
            }
        }
    }

    private IEnumerator PlayCartoonEffects(float speed, Vector3 impactDirection)
    {
        isPlayingEffect = true;
        float previousTimeScale = Time.timeScale;

        // Hit freeze curto
        Time.timeScale = 0.25f;
        yield return new WaitForSecondsRealtime(hitFreezeDuration);
        Time.timeScale = previousTimeScale > 0f ? previousTimeScale : 1f;

        float intensityMultiplier = Mathf.Clamp01(Mathf.Max(0.4f, speed / 20f));

        // Cartoon completo: squash bounce + spin + wheels + shake
        StartCoroutine(SquashAndStretchEffect(intensityMultiplier));
        StartCoroutine(CartoonSpinEffect(intensityMultiplier));
        StartCoroutine(WheelWobbleEffect(intensityMultiplier));
        StartCoroutine(ShakeEffect(intensityMultiplier * 0.7f));

        if (impactEffectPrefab != null)
            Instantiate(impactEffectPrefab, transform.position, Quaternion.identity);

        // Dust extra world-space (não substitui o cartoon)
        if (dustCoverVfxPrefab != null)
        {
            GameObject dust = Instantiate(dustCoverVfxPrefab, transform.position, Quaternion.identity);
            dust.transform.localScale = Vector3.one * 0.45f;
            Destroy(dust, 0.35f);
        }

        if (hitSoundEffect != null && audioSource != null)
            audioSource.PlayOneShot(hitSoundEffect, 0.7f);

        yield return new WaitForSeconds(Mathf.Max(squashStretchDuration, shakeDuration));

        if (Time.timeScale < 1f)
            Time.timeScale = 1f;
        RestoreTransform();
        isPlayingEffect = false;
    }

    private IEnumerator SquashAndStretchEffect(float intensity)
    {
        float intensityAmount = squashStretchIntensity * intensity;
        float half = squashStretchDuration * 0.35f;
        float bounce = squashStretchDuration * 0.65f;
        float elapsed = 0f;

        // Fase 1: esmaga (Y baixo, XZ largo)
        while (elapsed < half)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / half);
            float squash = Mathf.SmoothStep(0f, intensityAmount, t);
            transform.localScale = new Vector3(
                originalScale.x * (1f + squash * 0.7f),
                originalScale.y * (1f - squash),
                originalScale.z * (1f + squash * 0.7f));
            yield return null;
        }

        // Fase 2: estica além (overshoot) e volta com spring
        elapsed = 0f;
        Vector3 squashed = transform.localScale;
        Vector3 overshoot = new Vector3(
            originalScale.x * (1f - intensityAmount * 0.35f),
            originalScale.y * (1f + intensityAmount * 0.55f),
            originalScale.z * (1f - intensityAmount * 0.35f));

        while (elapsed < bounce)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / bounce);
            // EaseOutElastic-ish
            float spring = 1f - Mathf.Pow(1f - t, 3f);
            float wobble = Mathf.Sin(t * Mathf.PI * 2.5f) * (1f - t) * 0.15f * intensityAmount;

            Vector3 target = Vector3.Lerp(squashed, overshoot, Mathf.Sin(t * Mathf.PI * 0.5f));
            target = Vector3.Lerp(target, originalScale, spring);
            target.y += wobble;
            transform.localScale = target;
            yield return null;
        }

        transform.localScale = originalScale;
    }

    private IEnumerator CartoonSpinEffect(float intensity)
    {
        float duration = squashStretchDuration * 0.8f;
        float elapsed = 0f;
        float spinAmount = cartoonSpinDegrees * intensity * (Random.value > 0.5f ? 1f : -1f);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            // Vai e volta
            float angle = Mathf.Sin(t * Mathf.PI) * spinAmount;
            transform.localRotation = originalRotation * Quaternion.Euler(0f, 0f, angle);
            yield return null;
        }

        transform.localRotation = originalRotation;
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
        var wheelList = new List<Transform>();
        foreach (Transform t in GetComponentsInChildren<Transform>())
        {
            if (t != transform && t.name.IndexOf("Wheel", System.StringComparison.OrdinalIgnoreCase) >= 0)
                wheelList.Add(t);
        }

        if (wheelList.Count == 0)
            yield break;

        float elapsed = 0f;
        Quaternion[] originalWheelRotations = new Quaternion[wheelList.Count];

        for (int i = 0; i < wheelList.Count; i++)
            originalWheelRotations[i] = wheelList[i].localRotation;

        while (elapsed < shakeDuration)
        {
            elapsed += Time.deltaTime;

            for (int i = 0; i < wheelList.Count; i++)
            {
                if (wheelList[i] == null) continue;
                float wobble = Mathf.Sin(elapsed * 45f + i) * 28f * intensity;
                wheelList[i].localRotation = originalWheelRotations[i] * Quaternion.AngleAxis(wobble, Vector3.right);
            }

            yield return null;
        }

        for (int i = 0; i < wheelList.Count; i++)
        {
            if (wheelList[i] == null) continue;
            wheelList[i].localRotation = originalWheelRotations[i];
        }
    }
}
