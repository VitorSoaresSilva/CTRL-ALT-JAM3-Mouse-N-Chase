using System.Collections;
using UnityEngine;

/// <summary>
/// Solta barris rebatedos e pianos hazard periodicamente durante a missao do boss.
/// </summary>
public class BossDropController : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject barrelPrefab;
    [SerializeField] private GameObject pianoPrefab;
    [SerializeField] private GameObject dropEffectPrefab;

    [Header("Timing")]
    [SerializeField] private float barrelMinInterval = 4f;
    [SerializeField] private float barrelMaxInterval = 7f;
    [SerializeField] private float pianoMinInterval = 12f;
    [SerializeField] private float pianoMaxInterval = 20f;
    [SerializeField] private float pianoWarningSeconds = 1.2f;

    [Header("Spawn")]
    [SerializeField] private float barrelBehindDistance = 6f;
    [SerializeField] private float barrelLateralJitter = 1.5f;
    [SerializeField] private float pianoHeight = 12f;
    [SerializeField] private float pianoFallSpeed = 18f;
    [SerializeField] private float pianoLifetime = 12f;

    [Header("Optional warning UI")]
    [SerializeField] private GameObject help;

    private Transform playerTransform;
    private EnemyDamage bossDamage;
    private bool active;
    private Transform dropHolder;

    public void Configure(
        GameObject barrel,
        GameObject piano,
        GameObject effect,
        Transform player,
        EnemyDamage damage,
        Transform holder = null)
    {
        barrelPrefab = barrel;
        pianoPrefab = piano;
        dropEffectPrefab = effect;
        playerTransform = player;
        bossDamage = damage;
        dropHolder = holder;
    }

    public void BeginDrops()
    {
        if (active)
            return;

        active = true;
        StopAllCoroutines();
        StartCoroutine(BarrelLoop());
        StartCoroutine(PianoLoop());
    }

    public void StopDrops()
    {
        active = false;
        StopAllCoroutines();
        if (help != null)
            help.SetActive(false);
    }

    void OnDisable()
    {
        StopDrops();
    }

    private IEnumerator BarrelLoop()
    {
        // Primeiro drop um pouco depois do inicio
        yield return new WaitForSeconds(Random.Range(2f, 4f));

        while (active)
        {
            SpawnBarrel();
            yield return new WaitForSeconds(Random.Range(barrelMinInterval, barrelMaxInterval));
        }
    }

    private IEnumerator PianoLoop()
    {
        yield return new WaitForSeconds(Random.Range(6f, 10f));

        while (active)
        {
            if (help != null)
            {
                help.SetActive(true);
                yield return new WaitForSeconds(pianoWarningSeconds);
                help.SetActive(false);
            }

            SpawnPiano();
            yield return new WaitForSeconds(Random.Range(pianoMinInterval, pianoMaxInterval));
        }
    }

    private void SpawnBarrel()
    {
        if (barrelPrefab == null)
            return;

        Vector3 spawnPos = transform.position - transform.forward * barrelBehindDistance;
        spawnPos += transform.right * Random.Range(-barrelLateralJitter, barrelLateralJitter);
        spawnPos.y = Mathf.Max(spawnPos.y, 0.5f);

        Transform parent = dropHolder != null ? dropHolder : null;
        GameObject barrel = Instantiate(barrelPrefab, spawnPos, Quaternion.identity, parent);

        if (dropEffectPrefab != null)
        {
            GameObject fx = Instantiate(dropEffectPrefab, spawnPos, Quaternion.identity, parent);
            Destroy(fx, 3f);
        }

        BossThrowable throwable = barrel.GetComponent<BossThrowable>();
        if (throwable != null)
            throwable.Init(transform, bossDamage);
    }

    private void SpawnPiano()
    {
        if (pianoPrefab == null)
            return;

        Vector3 target = playerTransform != null
            ? playerTransform.position
            : transform.position - transform.forward * 8f;

        // Ligeiramente a frente do player na direcao do movimento do boss
        target += transform.forward * Random.Range(2f, 6f);
        target += transform.right * Random.Range(-2f, 2f);
        Vector3 spawnPos = target + Vector3.up * pianoHeight;

        Transform parent = dropHolder != null ? dropHolder : null;
        GameObject piano = Instantiate(pianoPrefab, spawnPos, Quaternion.identity, parent);

        Rigidbody rb = piano.GetComponent<Rigidbody>();
        if (rb != null)
            rb.velocity = Vector3.down * pianoFallSpeed;

        Destroy(piano, pianoLifetime);
    }
}
