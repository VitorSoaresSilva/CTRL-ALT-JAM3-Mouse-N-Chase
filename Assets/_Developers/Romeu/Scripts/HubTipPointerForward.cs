using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Forwards pointer enter from child graphics up to HubTipOnSelect.
/// </summary>
public class HubTipPointerForward : MonoBehaviour, IPointerEnterHandler
{
    public HubTipOnSelect owner;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (owner != null)
            owner.Show();
    }
}
