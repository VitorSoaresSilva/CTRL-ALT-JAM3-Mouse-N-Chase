using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuDialog : MonoBehaviour
{
    [SerializeField] Button openBtn;
    [SerializeField] Button closeBtn;

    public void Open()
    {
        gameObject.SetActive(true);
        if(closeBtn)
            closeBtn.Select();
    }

    public void Close()
    {
        gameObject.SetActive(false);
        if (openBtn)
            openBtn.Select();
    }

    public void Toggle()
    {
        if(gameObject.activeSelf)
            Close();
        else
            Open();
    }
}
