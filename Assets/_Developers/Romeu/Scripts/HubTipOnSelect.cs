using System;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Tip balloon on select/hover. Hand cursor is owned by UIButton (same as missions / Secret Car).
/// </summary>
public class HubTipOnSelect : MonoBehaviour, ISelectHandler, IPointerEnterHandler
{
    [SerializeField, TextArea] string unlockedTip;
    [SerializeField, TextArea] string lockedTip;

    Func<bool> isUnlocked;

    public void Configure(string whenUnlocked, string whenLocked, Func<bool> unlockedCheck)
    {
        unlockedTip = whenUnlocked;
        lockedTip = whenLocked;
        isUnlocked = unlockedCheck;
    }

    public void SetTip(string text)
    {
        unlockedTip = text;
        lockedTip = text;
        isUnlocked = null;
    }

    public void OnSelect(BaseEventData eventData) => Show();

    public void OnPointerEnter(PointerEventData eventData) => Show();

    public void Show()
    {
        if (MissionTipBalloon.instance == null)
            return;

        bool unlocked = isUnlocked == null || isUnlocked();
        string text = unlocked ? unlockedTip : lockedTip;
        if (string.IsNullOrEmpty(text))
            return;

        MissionTipBalloon.instance.ShowTip(text);
    }
}
