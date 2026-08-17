using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Simple selectable for Shield/Bumper tip slots. Movement is handled by HubUpgradeNav.
/// </summary>
public class HubUpgradeButton : Button
{
    public void ConfigureAsUpgradeSlot()
    {
        transition = Transition.ColorTint;
        colors = new ColorBlock
        {
            normalColor = Color.white,
            highlightedColor = new Color(0.95f, 0.95f, 0.95f, 1f),
            pressedColor = new Color(0.85f, 0.85f, 0.85f, 1f),
            selectedColor = new Color(0.9f, 0.95f, 1f, 1f),
            disabledColor = new Color(0.7f, 0.7f, 0.7f, 0.5f),
            colorMultiplier = 1f,
            fadeDuration = 0.1f
        };
        navigation = new Navigation { mode = Navigation.Mode.None };
        interactable = true;

        var image = GetComponent<Image>();
        if (image != null)
        {
            image.raycastTarget = true;
            targetGraphic = image;
        }
    }

    public override void OnMove(AxisEventData eventData)
    {
        // HubUpgradeNav handles movement.
    }
}
