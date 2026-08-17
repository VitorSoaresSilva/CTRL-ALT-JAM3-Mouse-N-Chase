using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Corner tip balloon on Police Station. Uses scene UI — only swaps TextMeshPro text.
/// </summary>
public class MissionTipBalloon : MonoBehaviour
{
    public static MissionTipBalloon instance { get; private set; }

    [SerializeField] private GameObject panel;
    [SerializeField] private TextMeshProUGUI tipText;
    [SerializeField] private float displayDuration = 5f;

    Coroutine hideRoutine;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        ResolveRefs();
        Hide();
        SetupUpgradeTips();
    }

    void OnDestroy()
    {
        if (instance == this)
            instance = null;
    }

    public void ShowForMission(MissionType type, bool isRandomDuty)
    {
        ShowTip(isRandomDuty ? GetDutyTip() : GetTipForMission(type));
    }

    public void ShowTip(string text)
    {
        ResolveRefs();
        if (tipText == null || string.IsNullOrEmpty(text))
            return;

        tipText.text = text;
        SetVisible(true);

        if (hideRoutine != null)
            StopCoroutine(hideRoutine);
        hideRoutine = StartCoroutine(HideAfterDelay());
    }

    public void Hide()
    {
        if (hideRoutine != null)
        {
            StopCoroutine(hideRoutine);
            hideRoutine = null;
        }

        SetVisible(false);
    }

    void SetVisible(bool visible)
    {
        // Never SetActive(false) on this GameObject — the script lives here and must stay awake.
        if (panel != null && panel != gameObject)
        {
            panel.SetActive(visible);
            return;
        }

        var bg = GetComponent<Image>();
        if (bg != null)
            bg.enabled = visible;

        if (tipText != null)
            tipText.enabled = visible;
    }

    IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(displayDuration);
        hideRoutine = null;
        Hide();
    }

    void ResolveRefs()
    {
        if (tipText == null)
            tipText = GetComponentInChildren<TextMeshProUGUI>(true);

        if (panel == null && tipText != null)
            panel = tipText.transform.parent != null ? tipText.transform.parent.gameObject : tipText.gameObject;
    }

    static string GetTipForMission(MissionType type)
    {
        switch (type)
        {
            case MissionType.FastResponse:
                return "Complete the laps. Avoid too many crashes and grab speed power-ups.";
            case MissionType.Pursuit:
                return "Take down all the racers before time runs out.";
            case MissionType.Rescue:
                return "Stay close to the hostage car — don't hit it. Escort them to the tunnel.";
            case MissionType.Boss:
                return "Ram Bad Pete to land hits. Survive until you take him down.";
            default:
                return "Stay sharp out there.";
        }
    }

    static string GetDutyTip() => "A random unlocked assignment. Stay sharp!";

    void SetupUpgradeTips()
    {
        Canvas canvas = FindMenuCanvas();
        if (canvas == null)
            return;

        AttachUpgradeTip(
            FindChildRecursive(canvas.transform, "BumperUpgrade"),
            "Bumper — take less crash damage.",
            "Bumper — unlocks at 7,000 career points.",
            () => CareerPoints.instance != null && CareerPoints.instance.BumperUnlocked);

        AttachUpgradeTip(
            FindChildRecursive(canvas.transform, "ShieldUpgrade"),
            "Shield — extra protection on the car.",
            "Shield — unlocks at 15,000 career points.",
            () => CareerPoints.instance != null && CareerPoints.instance.ShieldUnlocked);

        AttachUpgradeTip(
            FindChildRecursive(canvas.transform, "SecretCar"),
            "Secret car — click to toggle (blue = on, red = off).",
            null,
            () => CareerPoints.instance != null && CareerPoints.instance.SecretCarUnlocked);

        AttachTipToAllNamed(canvas.transform, "QuitButton",
            "Exit — return to the main menu.",
            "Exit — return to the main menu.",
            null);

        AttachTipToAllNamed(canvas.transform, "DutyButton",
            "Go to duty — start a random unlocked mission.",
            "Go to duty — start a random unlocked mission.",
            null);
    }

    static void AttachTipToAllNamed(Transform root, string objectName, string unlockedTip, string lockedTip, System.Func<bool> isUnlocked)
    {
        AttachTipRecursive(root, objectName, unlockedTip, lockedTip, isUnlocked);
    }

    static void AttachTipRecursive(Transform parent, string objectName, string unlockedTip, string lockedTip, System.Func<bool> isUnlocked)
    {
        if (parent == null)
            return;

        if (parent.name == objectName)
            AttachUpgradeTip(parent, unlockedTip, lockedTip, isUnlocked);

        for (int i = 0; i < parent.childCount; i++)
            AttachTipRecursive(parent.GetChild(i), objectName, unlockedTip, lockedTip, isUnlocked);
    }

    static void AttachUpgradeTip(Transform target, string unlockedTip, string lockedTip, System.Func<bool> isUnlocked)
    {
        if (target == null)
            return;

        var tipTrigger = target.GetComponent<HubTipOnSelect>();
        if (tipTrigger == null)
            tipTrigger = target.gameObject.AddComponent<HubTipOnSelect>();
        tipTrigger.Configure(unlockedTip, lockedTip, isUnlocked);

        var image = target.GetComponent<Image>();
        if (image != null)
            image.raycastTarget = true;

        var graphics = target.GetComponentsInChildren<Graphic>(true);
        for (int i = 0; i < graphics.Length; i++)
        {
            if (graphics[i] == null)
                continue;
            graphics[i].raycastTarget = true;

            var forward = graphics[i].GetComponent<HubTipPointerForward>();
            if (forward == null)
                forward = graphics[i].gameObject.AddComponent<HubTipPointerForward>();
            forward.owner = tipTrigger;
        }
    }

    static Canvas FindMenuCanvas()
    {
        var canvases = Object.FindObjectsOfType<Canvas>();
        for (int i = 0; i < canvases.Length; i++)
        {
            if (canvases[i] != null && canvases[i].name == "Menu Canvas")
                return canvases[i];
        }

        return canvases.Length > 0 ? canvases[0] : null;
    }

    static Transform FindChildRecursive(Transform parent, string name)
    {
        if (parent == null)
            return null;
        if (parent.name == name)
            return parent;

        for (int i = 0; i < parent.childCount; i++)
        {
            Transform found = FindChildRecursive(parent.GetChild(i), name);
            if (found != null)
                return found;
        }

        return null;
    }
}
