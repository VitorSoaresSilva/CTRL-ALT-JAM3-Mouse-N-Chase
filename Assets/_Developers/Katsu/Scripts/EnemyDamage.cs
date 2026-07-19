using _Developers.Vitor;
using System.Collections;
using UnityEngine;

public class EnemyDamage : MonoBehaviour
{
    EnemyCarFollowPath car;
    [Range(0, 100)] public float health = 80f;
    public string damageTag = "Player";
    public bool takeDamage = true;
    /// <summary>Carro com refem: colisao dispara onDamage mas nao conta para captura.</summary>
    public bool isHostageCarrier = false;
    /// <summary>Se true, colisao com o player nao causa hit (ex.: boss so toma dano de projeteis).</summary>
    public bool ignorePlayerRamming = false;
    public GameObject dieParticle;
    [SerializeField] private GameObject hitImpactVfxPrefab;
    [SerializeField] private AudioClip hitSoundEffect;
    [SerializeField] private float hitSoundVolume = 0.7f;
    public int maxHits = 5;
    public delegate void OnDamage();
    public OnDamage onDamage;

    public delegate void OnDie();
    public OnDie onDie;

    private int hitCount = 0;
    public int HitCount => hitCount;

    private AudioSource audioSource;
    private Transform visualRoot;
    private Vector3 originalScale;
    private bool isPlayingHitFx;

    private void Start()
    {
        if (car == null) car = GetComponentInParent<EnemyCarFollowPath>();

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        visualRoot = car != null && car.car != null ? car.car : transform;
        originalScale = visualRoot.localScale;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!takeDamage)
            return;

        if (!collision.gameObject.CompareTag(damageTag))
            return;

        if (ignorePlayerRamming)
            return;

        SpawnHitImpactVfx(collision);
        PlayHitFeedback();

        // Sequestrador com refem: so notifica o hit (missao trata a falha)
        if (isHostageCarrier)
        {
            onDamage?.Invoke();
            return;
        }

        ApplyHitInternal(playFeedback: false);
    }

    /// <summary>Aplica um hit (usado por projeteis/objetos rebatedos).</summary>
    public void RegisterHit()
    {
        RegisterHit(transform.position);
    }

    public void RegisterHit(Vector3 impactPoint)
    {
        if (!takeDamage || isHostageCarrier)
            return;

        SpawnHitImpactAt(impactPoint);
        ApplyHitInternal(playFeedback: true);
    }

    private void ApplyHitInternal(bool playFeedback)
    {
        if (playFeedback)
            PlayHitFeedback();

        float damageAmount = car != null ? car.speed : 1f;
        health -= damageAmount;
        hitCount++;
        onDamage?.Invoke();

        if (hitCount >= maxHits)
        {
            if (dieParticle != null) dieParticle.SetActive(true);
            onDie?.Invoke();
        }
    }

    private void PlayHitFeedback()
    {
        if (hitSoundEffect != null && audioSource != null)
            audioSource.PlayOneShot(hitSoundEffect, hitSoundVolume);

        if (!isPlayingHitFx && visualRoot != null)
            StartCoroutine(SquashPunch());
    }

    private IEnumerator SquashPunch()
    {
        isPlayingHitFx = true;
        float duration = 0.35f;
        float elapsed = 0f;
        Vector3 squashed = new Vector3(
            originalScale.x * 1.25f,
            originalScale.y * 0.7f,
            originalScale.z * 1.25f);

        while (elapsed < duration * 0.35f)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / (duration * 0.35f));
            visualRoot.localScale = Vector3.Lerp(originalScale, squashed, t);
            yield return null;
        }

        elapsed = 0f;
        while (elapsed < duration * 0.65f)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / (duration * 0.65f));
            visualRoot.localScale = Vector3.Lerp(squashed, originalScale, t);
            yield return null;
        }

        visualRoot.localScale = originalScale;
        isPlayingHitFx = false;
    }

    private void SpawnHitImpactVfx(Collision collision)
    {
        Vector3 pos = collision.contactCount > 0
            ? collision.GetContact(0).point
            : transform.position;
        SpawnHitImpactAt(pos);
    }

    private void SpawnHitImpactAt(Vector3 pos)
    {
        if (hitImpactVfxPrefab == null)
            return;

        GameObject vfx = Instantiate(hitImpactVfxPrefab, pos, Quaternion.identity);
        Destroy(vfx, 2f);
    }
}
