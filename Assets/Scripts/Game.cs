using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class Game : MonoBehaviour
{
    public static Game Instance;
    [SerializeField] Terrain mainTerrain;
    [SerializeField] private GameObject effectsParent;
    [SerializeField] GameObject panelMission;
    [SerializeField] GameObject canvasMain;
    [SerializeField] TextMeshProUGUI textMission;
    [SerializeField] TextMeshProUGUI textMessage;
    [SerializeField] TextMeshProUGUI textDiamonds;
    [SerializeField] List<Mission> missions = new();
    [SerializeField] Transform playerCameraRoot;
    List<GameObject> npcs = new();
    List<GameObject> cars = new();
    List<GameObject> gems = new();
    Mission activeMission = null;
    float timeLeftDisplayMessage;
    float timeLeftDisplayMission;
    float timeLeftDisplayDiamondsLeft;
    float timeLeftDelay;
    float timeLeftPlayerHint;
    string missionText;
    bool showMiniMap = true;
    bool showAchievements = false;
    int viewDistance = 5;
    CinemachineCamera cameraFollow;
    CinemachineCamera cameraFreeView;
    int wantedLevel = 4;
    bool missionExplained = false;
    private bool useFreeCamera;

    public List<GameObject> Cars { get => cars; set => cars = value; }
    public List<GameObject> NPCs { get => npcs; set => npcs = value; }
    public Mission ActiveMission { get => activeMission; set => activeMission = value; }
    public List<Mission> Missions { get => missions; set => missions = value; }
    public List<GameObject> Gems { get => gems; set => gems = value; }
    public GameObject EffectsParent { get => effectsParent; set => effectsParent = value; }
    public bool ShowMiniMap { get => showMiniMap; set => showMiniMap = value; }
    public int ViewDistance { get => viewDistance; set => viewDistance = value; }
    public Terrain MainTerrain { get => mainTerrain; set => mainTerrain = value; }
    public int WantedLevel { get => wantedLevel; set => wantedLevel = value; }
    public bool UseFreeCamera { get => useFreeCamera; set => useFreeCamera = value; }

    public void Awake()
    {
        Instance = this;
        cameraFollow = GameObject.Find("FollowCamera").GetComponent<CinemachineCamera>();
        cameraFreeView = GameObject.Find("FreeCamera").GetComponent<CinemachineCamera>();
    }

    public void Start()
    {
        panelMission.SetActive(false);
        Cursor.visible = false; 
        textDiamonds.enabled = false; 
        textMessage.enabled = false;
    }

    public void SetFollowCamera(Transform followTransform)
    {
        cameraFollow.Follow = followTransform;
    }

    public void SetFollowCameraToPlayer()
    {
        cameraFollow.Follow = playerCameraRoot;
    }

    public void ToggleFreeCamera()
    {
        useFreeCamera = !useFreeCamera;
        if (useFreeCamera)
        {
            ShowMessage("Free camera on");
            canvasMain.SetActive(false);
            cameraFreeView.gameObject.GetComponent<FreeCamera>().CameraPosition = Camera.main.transform.position;
            cameraFreeView.enabled = true; 
            cameraFreeView.gameObject.GetComponent<PlayerInput>().enabled = true;
        }
        else
        {
            ShowMessage("Free camera off");
            canvasMain.SetActive(true);
            cameraFreeView.enabled = false;
            cameraFreeView.gameObject.GetComponent<PlayerInput>().enabled = false;
        }
    }

    public void FixedUpdate()
    {

        /*
        if (!missionExplained && Time.time > 20)
        {
            missionExplained = true;
            GameObject.Find("Scripts/UI").GetComponent<UI>().RingPhone();
        }*/

        if (timeLeftDisplayMessage > 0)
        {
            timeLeftDisplayMessage -= Time.deltaTime;
            if (timeLeftDisplayMessage <= 0)
            {
                textMessage.enabled = false;
            }
        }
        if (timeLeftDisplayMission > 0)
        {
            timeLeftDisplayMission -= Time.deltaTime;
            if (timeLeftDisplayMission <= 0)
            {
                panelMission.SetActive(false);
            }
        }
        if (timeLeftDisplayDiamondsLeft > 0)
        {
            timeLeftDisplayDiamondsLeft -= Time.deltaTime;
            if (timeLeftDisplayDiamondsLeft <= 0)
            {
                textDiamonds.enabled = false;
            }
        }
        if (timeLeftDelay > 0)
        {
            timeLeftDelay -= Time.deltaTime;
            if (timeLeftDelay <= 0)
            {
                ShowMission();
            }
        }
    }

    private void ShowMission()
    {
        textMission.text = missionText;
        panelMission.SetActive(true);
        timeLeftDisplayMission = 8;
    }

    public void DisplayMission(string missionText, float delay = 0.01f)
    {
        this.missionText = missionText;
        timeLeftDelay = delay;
    }

    public void RemoveAllDiamonds()
    {
        foreach(GameObject gem in Gems)
        {
            Destroy(gem);
        }
    }

    public void DecreaseDiamonds()
    {
        Progress.Instance.Diamonds++;
        textDiamonds.text = "Diamonds left: " + (100 - Progress.Instance.Diamonds);
        textDiamonds.enabled = true;
        timeLeftDisplayDiamondsLeft = 3;
        if (Progress.Instance.Diamonds==100)
        {
            // remove the extra diamonds
            RemoveAllDiamonds();
        }
    }

    public void ShowMessage(string message, float duration = 3f)
    {
        textMessage.text = message;
        textMessage.enabled = true;
        timeLeftDisplayMessage = duration;
    }

    public void ShowAchievements(bool showAchievements)
    {
        this.showAchievements = showAchievements;
        if (showAchievements)
        {
            SceneManager.SetActiveScene(SceneManager.GetSceneByName("ProgressScene"));
        }
        else
        {
            SceneManager.SetActiveScene(SceneManager.GetSceneByName("MainScene"));
        }
    }
}
