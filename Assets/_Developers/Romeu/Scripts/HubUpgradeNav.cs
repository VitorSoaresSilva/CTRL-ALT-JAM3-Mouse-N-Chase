using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Manual move between upgrade tips. Keeps Navigation.Mode.None so uGUI does not
/// jump to Duty, while still allowing Down/Up/Left via Select().
/// </summary>
public class HubUpgradeNav : MonoBehaviour, IMoveHandler
{
    Selectable prev;
    Selectable next;
    Selectable left;
    Selectable right;

    public void SetNeighbors(Selectable leftNeighbor, Selectable rightNeighbor, Selectable upNeighbor, Selectable downNeighbor)
    {
        left = leftNeighbor;
        right = rightNeighbor;
        prev = upNeighbor;
        next = downNeighbor;

        var selectable = GetComponent<Selectable>();
        if (selectable != null)
        {
            // Mode.None stops Automatic/Explicit from skipping to Duty;
            // this component owns Up/Down/Left instead.
            selectable.navigation = new Navigation { mode = Navigation.Mode.None };
            selectable.interactable = true;
        }
    }

    public void OnMove(AxisEventData eventData)
    {
        Selectable target = null;
        switch (eventData.moveDir)
        {
            case MoveDirection.Up:
                target = prev;
                break;
            case MoveDirection.Down:
                target = next;
                break;
            case MoveDirection.Left:
                target = left;
                break;
            case MoveDirection.Right:
                target = right;
                break;
        }

        if (target == null)
            return;
        if (!target.gameObject.activeInHierarchy || !target.IsActive())
            return;

        target.Select();
        eventData.Use();
    }
}
