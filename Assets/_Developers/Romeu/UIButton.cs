using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIButton : MonoBehaviour, ISelectHandler
{
    [SerializeField] private RectTransform GlobalPointer;
    [SerializeField] private RectTransform DummyPointer;
    [field: SerializeField] public Vector3 PointerPosition { get; private set; }
    [field: SerializeField] public Vector3 PointerRotation { get; private set; }
    [SerializeField] private float transitionDuration = 1f;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OnSelect(BaseEventData eventData)
    {
        if(GlobalPointer != null)
        {
            GlobalPointer.SetParent(this.transform);
            StartCoroutine(MovePointer());

            //GlobalPointer.anchoredPosition = PointerPosition;
            //GlobalPointer.localRotation = Quaternion.Euler(PointerRotation);
            //GlobalPointer.position = PointerPosition;
            //GlobalPointer.rotation = Quaternion.Euler(PointerRotation);
        }

    }

    IEnumerator MovePointer()
    {
        float elapsedTime = 0;
        float progress = 0;
        while(progress <= 1)
        {
            GlobalPointer.anchoredPosition = Vector3.Slerp(GlobalPointer.anchoredPosition, PointerPosition, progress);
            GlobalPointer.localRotation = Quaternion.Slerp(GlobalPointer.localRotation, Quaternion.Euler(PointerRotation), progress);
                
            elapsedTime += Time.deltaTime;
            progress = elapsedTime / transitionDuration;
                
            yield return null;
        }

        GlobalPointer.anchoredPosition = PointerPosition;
        GlobalPointer.localRotation = Quaternion.Euler(PointerRotation);
    }
}
