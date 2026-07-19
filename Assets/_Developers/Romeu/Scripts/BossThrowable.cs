using UnityEngine;

/// <summary>
/// Objeto solto pelo boss: o player bate e lanca na direcao do boss; so conta hit se ja estiver lancado.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class BossThrowable : MonoBehaviour
{
    [SerializeField] private float launchSpeed = 45f;
    [SerializeField] private float lifetime = 10f;
    [SerializeField] private float upwardBias = 2f;

    private Rigidbody rb;
    private Transform bossTarget;
    private EnemyDamage bossDamage;
    private bool launched;
    private bool consumed;

    public bool IsLaunched => launched;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    public void Init(Transform bossTransform, EnemyDamage damage)
    {
        bossTarget = bossTransform;
        bossDamage = damage;
        Destroy(gameObject, lifetime);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (consumed)
            return;

        if (!launched && collision.gameObject.CompareTag("Player"))
        {
            LaunchTowardBoss();
            return;
        }

        if (!launched)
            return;

        EnemyDamage damage = collision.gameObject.GetComponentInParent<EnemyDamage>();
        if (damage == null || damage != bossDamage)
            return;

        Vector3 impact = collision.contactCount > 0
            ? collision.GetContact(0).point
            : transform.position;

        consumed = true;
        damage.RegisterHit(impact);
        Destroy(gameObject);
    }

    private void LaunchTowardBoss()
    {
        if (launched || bossTarget == null)
            return;

        launched = true;

        Vector3 toBoss = bossTarget.position - transform.position;
        toBoss.y += upwardBias;
        if (toBoss.sqrMagnitude < 0.01f)
            toBoss = transform.forward;

        rb.velocity = toBoss.normalized * launchSpeed;
        rb.angularVelocity = Random.insideUnitSphere * 8f;
    }
}
