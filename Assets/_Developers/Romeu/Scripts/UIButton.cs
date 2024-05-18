using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIButton : MonoBehaviour, ISelectHandler, ISubmitHandler
{
    [SerializeField] private UIPointer GlobalPointer;
    [field: SerializeField] public Vector3 PointerPosition { get; private set; }
    [field: SerializeField] public Vector3 PointerRotation { get; private set; }
    [SerializeField] private float transitionDuration = 1f;
    public MissionType missionType;

    public Button Button { get; private set; }

    void OnEnable()
    {
        Button = GetComponent<Button>();

        if(GlobalPointer == null) GlobalPointer = FindObjectOfType<UIPointer>();
    }

    public void OnSelect(BaseEventData eventData)
    {
        if(GlobalPointer != null)
        {
            GlobalPointer.MoveTo(PointerPosition, Quaternion.Euler(PointerRotation));
        }

    }

    public void OnSubmit(BaseEventData eventData)
    {

        Debug.Log($"Submited {name}");
    }
}
