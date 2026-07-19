using System.Collections;
using UnityEngine;

/// <summary>
/// Temporary speed boost for Pursuit — only the player moves faster.
/// Uses powerupSpeedOverride so NPC AI keeps reading base Follow.speed.
/// Visual: Area_fire_red
/// </summary>
public class PursuitSpeedPowerUp : MonoBehaviour
{
    public float speedBoost = 45f;
    public float duration = 4f;
    [SerializeField] private GameObject visualPrefab;

    private PlayerCar playerCar;
    public static Coroutine ActiveCoroutine { get; set; }

    void Start()
    {
        if (playerCar == null)
            playerCar = FindObjectOfType<PlayerCar>();

        for (int i = 0; i < transform.childCount; i++)
            transform.GetChild(i).gameObject.SetActive(false);

        var rootPs = GetComponent<ParticleSystem>();
        if (rootPs != null) rootPs.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        if (visualPrefab != null)
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

        if (playerCar == null)
            playerCar = FindObjectOfType<PlayerCar>();

        if (playerCar != null && playerCar.Follow != null)
        {
            if (ActiveCoroutine != null)
                playerCar.StopCoroutine(ActiveCoroutine);
            if (PursuitSlowPowerUp.ActiveCoroutine != null)
            {
                playerCar.StopCoroutine(PursuitSlowPowerUp.ActiveCoroutine);
                PursuitSlowPowerUp.ActiveCoroutine = null;
            }

            ActiveCoroutine = playerCar.StartCoroutine(SpeedBoost(playerCar, speedBoost, duration));
            Debug.Log($"[Pursuit] SpeedPowerUp collected! Player-only boost: {speedBoost} for {duration}s");
        }

        gameObject.SetActive(false);
    }

    private static IEnumerator SpeedBoost(PlayerCar target, float boostAmount, float boostDuration)
    {
        target.Follow.SetPowerupSpeed(boostAmount);
        yield return new WaitForSeconds(boostDuration);
        target.Follow.ClearPowerupSpeed();
        ActiveCoroutine = null;
    }
}
