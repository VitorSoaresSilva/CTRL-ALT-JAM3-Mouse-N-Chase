using PathCreation;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static MainMenu;

public class GameplayManager : MonoBehaviour
{
    [SerializeField, Header("Player")] private PlayerCar playerCar;
    [SerializeField] private CameraControl cameraControl;

    [SerializeField, Header("Start")] private float SceneCameraSpeed = 2f;
    [SerializeField] private AudioSource RadioAudio;
    [SerializeField] private AudioSource PlayerResponse;

    [SerializeField, Header("Canvas")] private Canvas GameCanvas;
    [SerializeField] private Slider HealthSlider;

    [SerializeField, Header("Upgrades")] private Image shieldSlot;
    [SerializeField] private Image slotSlot;
    [SerializeField] private Image bumperSlot;

    [SerializeField, Header("Path")] private PathCreator pathCreator;
    [SerializeField] private GameObject TunnelObject;
    [SerializeField] private float TunnelDistance; // dist do inicio e fim do path

    //[SerializeField] private GameObject roadMesh;
    //[SerializeField] private GameObject[] enemyCars;
    //[SerializeField] private GameObject[] powerUps;
    
    private float StartSceneTime = 5;

    void OnEnable()
    {
        if(SceneControl.instance != null) SceneControl.instance.AddGameplayManager(this);
        if(GameCanvas == null) GameCanvas = GetComponent<Canvas>();
        if(HealthSlider == null) HealthSlider = GameCanvas.GetComponentInChildren<Slider>(true);
        if(cameraControl == null) cameraControl = FindObjectOfType<CameraControl>();
        if(playerCar == null) playerCar = FindObjectOfType<PlayerCar>();

        if(CareerPoints.instance != null)
        {
            if (shieldSlot != null)
            {
                Debug.Log(CareerPoints.instance.ShieldUnlocked);
                shieldSlot.gameObject.SetActive(CareerPoints.instance.ShieldUnlocked);
            }

            if (slotSlot != null)
            {
                Debug.Log(CareerPoints.instance.SlotUnlocked);
                slotSlot.gameObject.SetActive(CareerPoints.instance.SlotUnlocked);
            }

            if (bumperSlot != null)
            {
                Debug.Log(CareerPoints.instance.BumperUnlocked);
                bumperSlot.gameObject.SetActive(CareerPoints.instance.BumperUnlocked);
            }
        }
    }

    private void Start()
    {
        SpawnTunnel();

    }

    public void StartGameplay() // wip
    {
        Debug.Log("Starting gameplay");
        playerCar.gameObject.SetActive(true);

        if(SceneControl.instance != null)
        {
            AudioClip[] clips = new AudioClip[0];
            switch (SceneControl.instance.currentMission)
            {
                case MissionType.FastResponse:
                    clips = SceneControl.instance.FastResponseClips;
                    break;
                case MissionType.Pursuit:
                    clips = SceneControl.instance.PursuitClips;
                    break;
                case MissionType.Rescue:
                    clips = SceneControl.instance.RescueClips;
                    break;
                case MissionType.Boss:
                    clips = SceneControl.instance.BossClips;
                    break;
            }

            if(clips.Length > 0)
            {
                RadioAudio.clip = clips[Random.Range(0, clips.Length)];
                RadioAudio.Play();
                StartSceneTime = RadioAudio.clip.length;
            }
        }

        float followDist = cameraControl.FollowDistance;
        cameraControl.FollowDistance = -followDist;
        if (RadioAudio != null)
            RadioAudio.Play();

        // Play start cinematic
        StartCoroutine(PlayStartScene());
        IEnumerator PlayStartScene()
        {

            yield return new WaitForSeconds(StartSceneTime);
            // volta a camera a posição original

            while(Mathf.Abs(cameraControl.FollowDistance - followDist) > 0.2f)
            {
                cameraControl.FollowDistance = Mathf.Lerp(cameraControl.FollowDistance, followDist, SceneCameraSpeed * Time.deltaTime);
                yield return null;
            }

            cameraControl.FollowDistance = followDist;
        }
    }

    // Update is called once per frame
    void Update()
    {
        HealthSlider.value = playerCar.carDamage.health / 100;
    }

    private void SpawnTunnel()
    {
        if(pathCreator != null && TunnelObject != null)
        {
            VertexPath path = pathCreator.path;
            Vector3 endPoint = path.GetPointAtDistance(path.length);
            Debug.Log(endPoint);
            Instantiate(TunnelObject, endPoint, path.GetRotationAtDistance(path.length));

            //Quaternion startRot = Quaternion.LookRotation(path.GetRotationAtDistance(TunnelDistance).eulerAngles, Vector3.up);
            //Quaternion endRot = Quaternion.LookRotation(path.GetRotationAtDistance(path.length - TunnelDistance).eulerAngles, Vector3.up);
            //GameObject startTunnel = Instantiate(TunnelObject, startPos, startRot);
            //startTunnel.transform.LookAt(path.GetRotationAtDistance(TunnelDistance).eulerAngles);
            //Instantiate(TunnelObject, endPos, endRot);
        }
    }
}
