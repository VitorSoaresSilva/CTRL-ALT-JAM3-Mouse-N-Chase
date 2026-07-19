using System.Collections;
using UnityEngine;

/// <summary>
/// Solta barris rebatedos (faixa esquerda/centro/direita) e pianos hazard periodicamente.
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
    [SerializeField] private float barrelBehindDistance = 8f;
    [SerializeField] private float laneOffset = 4f;
    [SerializeField] private float barrelHoverAboveRoad = 1.4f;
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

        // -1 esquerda, 0 centro, +1 direita
        int lane = Random.Range(-1, 2);
        Vector3 spawnPos = transform.position - transform.forward * barrelBehindDistance;
        spawnPos += transform.right * (lane * laneOffset);
        spawnPos.y = GetRoadHoverY();

        Transform parent = dropHolder != null ? dropHolder : null;
        GameObject barrel = Instantiate(barrelPrefab, spawnPos, Quaternion.identity, parent);

        if (dropEffectPrefab != null)
        {
            GameObject fx = Instantiate(dropEffectPrefab, spawnPos, Quaternion.identity, parent);
            Destroy(fx, 3f);
        }

        BossThrowable throwable = barrel.GetComponent<BossThrowable>();
        if (throwable != null)
            throwable.Init(transform, bossDamage, spawnPos.y);
    }

    private float GetRoadHoverY()
    {
        // Boss está em pathY + yOffset; sobe o barril um pouco acima do asfalto
        float roadY = transform.position.y;
        var follow = GetComponent<_Developers.Vitor.EnemyCarFollowPath>();
        if (follow != null)
            roadY = transform.position.y - follow.yOffset;

        return roadY + barrelHoverAboveRoad;
    }

    private void SpawnPiano()
    {
        if (pianoPrefab == null)
            return;

        Vector3 target = playerTransform != null
            ? playerTransform.position
            : transform.position - transform.forward * 8f;

        int lane = Random.Range(-1, 2);
        target += transform.forward * Random.Range(2f, 6f);
        target += transform.right * (lane * laneOffset);
        Vector3 spawnPos = target + Vector3.up * pianoHeight;

        Transform parent = dropHolder != null ? dropHolder : null;
        GameObject piano = Instantiate(pianoPrefab, spawnPos, Quaternion.identity, parent);

        Rigidbody rb = piano.GetComponent<Rigidbody>();
        if (rb != null)
            rb.velocity = Vector3.down * pianoFallSpeed;

        Destroy(piano, pianoLifetime);
    }
}
