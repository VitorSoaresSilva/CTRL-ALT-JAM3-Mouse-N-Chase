using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RescueMission : MonoBehaviour
{
    public GameplayManager gameplayManager;

    void OnEnable()
    {
        if (SceneControl.instance != null)
        {
            if (SceneControl.instance.currentMission != MissionType.Rescue)
            {
                Destroy(this.gameObject);
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        if (gameplayManager == null)
        {
            gameplayManager = FindObjectOfType<GameplayManager>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
