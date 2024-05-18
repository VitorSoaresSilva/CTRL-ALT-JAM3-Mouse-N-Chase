using PathCreation;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameplayManager : MonoBehaviour
{
    [SerializeField] private GameObject playerCar;
    [SerializeField] private Camera CameraControl;
    [SerializeField] private GameObject roadMesh;
    [SerializeField] private PathCreator pathCreator;

    [SerializeField] private GameObject[] enemyCars;
    [SerializeField] private GameObject[] powerUps;

    void OnEnable()
    {
        if(SceneControl.instance != null)
            SceneControl.instance.AddGameplayManager(this);
    }

    public void StartGameplay() // wip
    {
        Debug.Log("Starting gameplay");
        playerCar.SetActive(true);
        roadMesh.SetActive(true);

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
