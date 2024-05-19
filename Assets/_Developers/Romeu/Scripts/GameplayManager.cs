using PathCreation;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameplayManager : MonoBehaviour
{
    [SerializeField] private GameObject playerCar;
    [SerializeField] private CameraControl cameraControl;

    [SerializeField] private GameObject roadMesh;
    [SerializeField] private PathCreator pathCreator;

    [SerializeField] private GameObject[] enemyCars;
    [SerializeField] private GameObject[] powerUps;

    [SerializeField] private float StartSceneTime = 5;

    private AudioSource RadioAudio;
    private AudioSource PlayerResponse;

    void OnEnable()
    {
        if(SceneControl.instance != null)
            SceneControl.instance.AddGameplayManager(this);
    }

    private void Start()
    {
        if(cameraControl == null) cameraControl = FindObjectOfType<CameraControl>();
        

    }

    public void StartGameplay() // wip
    {
        Debug.Log("Starting gameplay");
        playerCar.SetActive(true);
        roadMesh.SetActive(true);

        float followDist = cameraControl.FollowDistance;
        cameraControl.FollowDistance = -followDist;
        
        // Play start cinematic

        IEnumerator PlayStartScene()
        {
            yield return new WaitForSeconds(StartSceneTime);
            // volta a camera a posição original

            while(Mathf.Abs(followDist - cameraControl.FollowDistance) > 0.1f)
            {
                cameraControl.FollowDistance = Mathf.Lerp(cameraControl.FollowDistance, followDist, 1f * Time.deltaTime);
                yield return null;
            }

            cameraControl.FollowDistance = followDist;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
