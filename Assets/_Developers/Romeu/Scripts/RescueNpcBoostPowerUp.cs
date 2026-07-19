using _Developers.Vitor;
using UnityEngine;

/// <summary>
/// Small temporary speed boost applied to the Rescue hostage carrier NPC.
/// </summary>
public class RescueNpcBoostPowerUp : MonoBehaviour
{
    public float speedOffset = 4.5f;
    public float duration = 2.5f;
    [SerializeField] private GameObject visualPrefab;

    void Start()
    {
        for (int i = 0; i < transform.childCount; i++)
            transform.GetChild(i).gameObject.SetActive(false);

        var rootPs = GetComponent<ParticleSystem>();
        if (rootPs != null) rootPs.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        if (visualPrefab != null && transform.Find("SpeedVfx") == null)
        {
            GameObject vfx = Instantiate(visualPrefab, transform);
            vfx.transform.localPosition = Vector3.zero;
            vfx.transform.localRotation = Quaternion.identity;
            vfx.transform.localScale = Vector3.one * 0.7f;
            vfx.name = "SpeedVfx";
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.gameObject.CompareTag("Player"))
            return;

        EnemyCarFollowPath hostage = RescueMission.ActiveHostageCarrier;
        if (hostage != null)
        {
            hostage.ApplyTemporarySpeedOffset(speedOffset, duration);
            Debug.Log($"[Rescue] NPC boost +{speedOffset} for {duration}s");
        }

        gameObject.SetActive(false);
    }
}
