using System.Collections;
using UnityEngine;

/// <summary>
/// Temporary car slowdown for Pursuit — only the player decelerates.
/// Uses powerupSpeedOverride so NPC AI keeps reading base Follow.speed.
/// Visual: Area_circles_blue
/// </summary>
public class PursuitSlowPowerUp : MonoBehaviour
{
    public float slowSpeed = 12f;
    public float duration = 3f;
    [SerializeField] private GameObject visualPrefab;

    private PlayerCar playerCar;
    public static Coroutine ActiveCoroutine { get; set; }

    void Start()
    {
        if (playerCar == null)
            playerCar = FindObjectOfType<PlayerCar>();

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

        if (playerCar == null)
            playerCar = FindObjectOfType<PlayerCar>();

        if (playerCar != null && playerCar.Follow != null)
        {
            if (ActiveCoroutine != null)
                playerCar.StopCoroutine(ActiveCoroutine);

            // Cancela boost do player se estiver ativo
            if (PursuitSpeedPowerUp.ActiveCoroutine != null)
            {
                playerCar.StopCoroutine(PursuitSpeedPowerUp.ActiveCoroutine);
                PursuitSpeedPowerUp.ActiveCoroutine = null;
            }

            ActiveCoroutine = playerCar.StartCoroutine(ApplySlow(playerCar, slowSpeed, duration));
            Debug.Log($"[Pursuit] SlowPowerUp collected! Player-only slow: {slowSpeed} for {duration}s");
        }

        gameObject.SetActive(false);
    }

    private static IEnumerator ApplySlow(PlayerCar target, float slowAmount, float slowDuration)
    {
        target.Follow.SetPowerupSpeed(slowAmount);
        yield return new WaitForSeconds(slowDuration);
        target.Follow.ClearPowerupSpeed();
        ActiveCoroutine = null;
    }
}
