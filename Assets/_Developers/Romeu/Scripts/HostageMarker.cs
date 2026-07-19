using _Developers.Vitor;
using UnityEngine;

/// <summary>
/// Marca o carro como sequestrador com refém (sem visual placeholder).
/// </summary>
public class HostageMarker : MonoBehaviour
{
    void Awake()
    {
        EnemyDamage damage = GetComponentInChildren<EnemyDamage>();
        if (damage != null)
            damage.isHostageCarrier = true;
    }
}
