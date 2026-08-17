using TMPro;
using UnityEngine;

/// <summary>
/// Shows "Ver. X" from Application.version on a scene TextMeshProUGUI (no runtime Instantiate).
/// </summary>
public class MenuVersionLabel : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI versionText;

    void Awake()
    {
        if (versionText == null)
            versionText = GetComponent<TextMeshProUGUI>();
        if (versionText == null)
            versionText = GetComponentInChildren<TextMeshProUGUI>(true);

        if (versionText != null)
            versionText.text = $"Ver. {Application.version}";
    }
}
