using _Developers.Vitor;
using UnityEngine;

public class EnemyDamage : MonoBehaviour
{
    EnemyCarFollowPath car; // Referência ao script EnemyCarFollowPath do carro
    [Range(0, 100)] public float health = 80f; // Saúde inicial
    public string damageTag = "Player"; // Tag dos objetos que causam dano ao carro
    public bool takeDamage = true;
    public GameObject dieParticle;
    [SerializeField] private GameObject hitImpactVfxPrefab;
    public delegate void OnDamage();
    public OnDamage onDamage;

    public delegate void OnDie();
    public OnDie onDie;

    private int hitCount = 0; // Contador de hits do jogador
    private const int maxHits = 5; // Máximo de hits necessários para prender o inimigo

    private void Start()
    {
        if (car == null) car = GetComponentInParent<EnemyCarFollowPath>();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag(damageTag) && takeDamage)
        {
            // Calcule o dano com base na velocidade do carro
            float damage = car.speed;

            // Diminua a saúde do carro
            health -= damage;

            // Incrementa contador de hits
            hitCount++;

            SpawnHitImpactVfx(collision);

            onDamage?.Invoke();

            // Verifique se atingiu 5 hits (inimigo preso)
            if (hitCount >= maxHits)
            {
                if(dieParticle != null) dieParticle.SetActive(true);
                onDie?.Invoke();
            }
        }
    }

    private void SpawnHitImpactVfx(Collision collision)
    {
        if (hitImpactVfxPrefab == null)
            return;

        Vector3 pos = collision.contactCount > 0
            ? collision.GetContact(0).point
            : transform.position;

        GameObject vfx = Instantiate(hitImpactVfxPrefab, pos, Quaternion.identity);
        Destroy(vfx, 2f);
    }
}
