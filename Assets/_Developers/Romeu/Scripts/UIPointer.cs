using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIPointer : MonoBehaviour
{
    [SerializeField] UIButton FirstButtonSelected;
    [SerializeField] float moveOffset = 20f;
    [SerializeField] private float moveSpeed = 1f;
    [SerializeField] private MoveAxis moveAxis = MoveAxis.X;

    private RectTransform rt;
    private float currentOffset = 0f;
    private bool moving = true;
    private float initialXPosition;
    public enum MoveAxis { X, Y }

    private void OnEnable()
    {
        rt = GetComponent<RectTransform>();
    }

    void Start()
    {
        if(FirstButtonSelected != null)
        {
            if(FirstButtonSelected.TryGetComponent<Button>(out Button btn))
                btn.Select();
        }

        if(moveAxis == MoveAxis.X)
            initialXPosition = rt.localPosition.x;
        else if(moveAxis == MoveAxis.Y)
            initialXPosition = rt.localPosition.y;
    }

    void Update()
    {
        // Movendo o RectTransform
        if (moving)
        {
            currentOffset += moveSpeed * Time.deltaTime;
            if (currentOffset >= moveOffset)
            {
                moving = false;
            }
        }
        else
        {
            currentOffset -= moveSpeed * Time.deltaTime;
            if (currentOffset <= -moveOffset)
            {
                moving = true;
            }
        }

        // Calculando a nova posição com base na posição inicial e no offset atual
        float newXPosition = initialXPosition + currentOffset;

        // Aplicando a nova posição ao RectTransform
        Vector3 newPosition = rt.localPosition;
        if(moveAxis == MoveAxis.X)
            newPosition.x = newXPosition;
        else if(moveAxis == MoveAxis.Y)
            newPosition.y = newXPosition;

        rt.localPosition = newPosition;
    }

    public void MoveTo(Vector3 position, Quaternion rotation)
    {
        rt.anchoredPosition = position;
        rt.localRotation = rotation;
        initialXPosition = position.x;
    }
}
