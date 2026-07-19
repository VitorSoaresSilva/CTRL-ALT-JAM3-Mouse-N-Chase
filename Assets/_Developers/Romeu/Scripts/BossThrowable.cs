using UnityEngine;

/// <summary>
/// Barril do boss: flutua acima do asfalto; ao ser atingido pelo player, busca o boss (teleguiado).
/// </summary>
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class BossThrowable : MonoBehaviour
{
    [SerializeField] private float launchSpeed = 42f;
    [SerializeField] private float lifetime = 12f;
    [SerializeField] private float hitRadius = 3.5f;
    [SerializeField] private float turnRate = 12f;
    [SerializeField] private float upwardBias = 1.2f;

    private Rigidbody rb;
    private Collider col;
    private Transform bossTarget;
    private EnemyDamage bossDamage;
    private bool launched;
    private bool consumed;
    private float hoverY;

    public bool IsLaunched => launched;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    public void Init(Transform bossTransform, EnemyDamage damage, float worldHoverY)
    {
        bossTarget = bossTransform;
        bossDamage = damage;
        hoverY = worldHoverY;

        // Flutua parado no ar — sem gravidade / sem afundar no chão
        Vector3 pos = transform.position;
        pos.y = hoverY;
        transform.position = pos;

        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.useGravity = false;
        rb.isKinematic = true;

        Destroy(gameObject, lifetime);
    }

    void FixedUpdate()
    {
        if (consumed)
            return;

        // Mantém altura enquanto espera o player
        if (!launched)
        {
            Vector3 pos = transform.position;
            if (Mathf.Abs(pos.y - hoverY) > 0.01f)
            {
                pos.y = hoverY;
                transform.position = pos;
            }
            return;
        }

        if (bossTarget == null)
            return;

        Vector3 aimPoint = bossTarget.position + Vector3.up * upwardBias;
        Vector3 toBoss = aimPoint - transform.position;
        float dist = toBoss.magnitude;

        if (dist <= hitRadius)
        {
            ConsumeHit(aimPoint);
            return;
        }

        Vector3 desiredVel = toBoss.normalized * launchSpeed;
        rb.velocity = Vector3.Lerp(rb.velocity, desiredVel, 1f - Mathf.Exp(-turnRate * Time.fixedDeltaTime));
        rb.useGravity = false;

        if (rb.velocity.sqrMagnitude > 0.01f)
            transform.rotation = Quaternion.LookRotation(rb.velocity.normalized, Vector3.up);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (consumed)
            return;

        if (!launched && IsPlayerCollider(collision.gameObject))
        {
            LaunchTowardBoss(collision.gameObject);
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
        ConsumeHit(impact);
    }

    private static bool IsPlayerCollider(GameObject go)
    {
        return go.CompareTag("Player") || go.GetComponentInParent<PlayerCar>() != null;
    }

    private void LaunchTowardBoss(GameObject playerObject)
    {
        if (launched || bossTarget == null)
            return;

        launched = true;

        rb.isKinematic = false;
        rb.useGravity = false;

        Collider playerCol = playerObject.GetComponentInParent<Collider>();
        if (playerCol == null)
            playerCol = playerObject.GetComponent<Collider>();
        if (playerCol != null && col != null)
            Physics.IgnoreCollision(col, playerCol, true);

        Vector3 toBoss = bossTarget.position - transform.position;
        toBoss.y += upwardBias;
        if (toBoss.sqrMagnitude < 0.01f)
            toBoss = transform.forward;

        rb.velocity = toBoss.normalized * launchSpeed;
        rb.angularVelocity = Random.insideUnitSphere * 6f;
    }

    private void ConsumeHit(Vector3 impactPoint)
    {
        if (consumed || bossDamage == null)
            return;

        consumed = true;
        bossDamage.RegisterHit(impactPoint);
        Destroy(gameObject);
    }
}
