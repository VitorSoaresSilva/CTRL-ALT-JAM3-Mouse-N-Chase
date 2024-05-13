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

    private RectTransform rt;
    private float currentOffset = 0f;
    private bool movingRight = true;
    private float initialXPosition;

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

        initialXPosition = rt.localPosition.x;
    }

    void Update()
    {
        // Movendo o RectTransform
        if (movingRight)
        {
            currentOffset += moveSpeed * Time.deltaTime;
            if (currentOffset >= moveOffset)
            {
                movingRight = false;
            }
        }
        else
        {
            currentOffset -= moveSpeed * Time.deltaTime;
            if (currentOffset <= -moveOffset)
            {
                movingRight = true;
            }
        }

        // Calculando a nova posição com base na posição inicial e no offset atual
        float newXPosition = initialXPosition + currentOffset;

        // Aplicando a nova posição ao RectTransform
        Vector3 newPosition = rt.localPosition;
        newPosition.x = newXPosition;
        rt.localPosition = newPosition;
    }

    public void MoveTo(Vector3 position, Quaternion rotation)
    {
        rt.anchoredPosition = position;
        rt.localRotation = rotation;
        initialXPosition = position.x;
    }
}
