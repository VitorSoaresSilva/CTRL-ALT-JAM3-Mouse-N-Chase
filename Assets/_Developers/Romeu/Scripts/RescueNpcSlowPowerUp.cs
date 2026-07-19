using _Developers.Vitor;
using UnityEngine;

/// <summary>
/// Small temporary slow applied to the Rescue hostage carrier NPC.
/// </summary>
public class RescueNpcSlowPowerUp : MonoBehaviour
{
    public float speedOffset = -4.5f;
    public float duration = 2.5f;
    [SerializeField] private GameObject visualPrefab;

    void Start()
    {
        if (visualPrefab != null && transform.Find("SlowVfx") == null)
        {
            GameObject vfx = Instantiate(visualPrefab, transform);
            vfx.transform.localPosition = Vector3.zero;
            vfx.transform.localRotation = Quaternion.identity;
            vfx.transform.localScale = Vector3.one * 0.7f;
            vfx.name = "SlowVfx";
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
            Debug.Log($"[Rescue] NPC slow {speedOffset} for {duration}s");
        }

        gameObject.SetActive(false);
    }
}
