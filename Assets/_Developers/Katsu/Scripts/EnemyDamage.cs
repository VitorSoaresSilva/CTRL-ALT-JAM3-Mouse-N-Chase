using _Developers.Vitor;
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
    public int maxHits = 5;
    public delegate void OnDamage();
    public OnDamage onDamage;

    public delegate void OnDie();
    public OnDie onDie;

    private int hitCount = 0;
    public int HitCount => hitCount;

    private void Start()
    {
        if (car == null) car = GetComponentInParent<EnemyCarFollowPath>();
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

        // Sequestrador com refem: so notifica o hit (missao trata a falha)
        if (isHostageCarrier)
        {
            onDamage?.Invoke();
            return;
        }

        ApplyHitInternal();
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
        ApplyHitInternal();
    }

    private void ApplyHitInternal()
    {
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
