using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private RectTransform jamLogo;
    [SerializeField] private float logoRotationSpeed = 50f;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        jamLogo.transform.Rotate(0, logoRotationSpeed * Time.deltaTime, 0);
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
