using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI : MonoBehaviour
{
    [SerializeField] Texture2D textureMap;
    [SerializeField] Texture2D textureFrame;
    [SerializeField] Texture2D texturePlayerPosition;
    [SerializeField] Texture2D textureMission;
    [SerializeField] Texture2D textureDestination;
    [SerializeField] private Shader blendShader;
    [SerializeField] GameObject canvasMissionCompleted;
    [SerializeField] GameObject canvasAchievements;
    [SerializeField] GameObject canvasHelp;
    [SerializeField] TextMeshProUGUI textMissionTime;
    [SerializeField] TextMeshProUGUI textMissionMedal;
    [SerializeField] TextMeshProUGUI textInfo;
    [SerializeField] TextMeshProUGUI textInfo2;
    [SerializeField] List<GameObject> wantedStars;
    [SerializeField] Image imageMedal;
    [SerializeField] Image imagePhone;

    //    [SerializeField] private RectTransform minimapPlayer;
    private Rect rectFrame;
    private Player player;
    private int mapTextureHeight, mapTextureWidth;
    private int minimapWidth, minimapHeight;
    private int minimapPositionX, minimapPositionY;
    private float ratioTextureToMinimapX, ratioTextureToMinimapY;
    private bool showMap;
    private bool showAchievements;
    private bool showHelp;
    private float timeLeftDisplayMissionCompleted;
    private bool wantedStarsActive;
    private float timeLeftWantedStarsVisible;
    private float timePhoneActive;
    private bool phoneConversationActive;
    private AudioSource soundRingtone;
    private AudioSource soundMission1;
    private bool phoneConversationStarted;

    private float corner;

    public bool ShowMap { get => showMap; set => showMap = value; }

    void Start()
    {
        mapTextureWidth = textureMap.width;
        mapTextureHeight = textureMap.height;
        minimapWidth = (int)(Screen.width * 0.15);
        minimapHeight = (int)(Screen.height * 0.25);
        minimapPositionX = Screen.width - minimapWidth - 10;
        minimapPositionY = Screen.height - minimapHeight - 10;
        ratioTextureToMinimapX = minimapWidth / (float)textureMap.width;
        ratioTextureToMinimapY = minimapHeight / (float)textureMap.height;
        rectFrame = new Rect(minimapPositionX - 8, minimapPositionY - 8, minimapWidth + 16, minimapHeight + 16);
        player = GameObject.Find("Player").GetComponent<Player>();
        canvasMissionCompleted.SetActive(false);
        canvasAchievements.SetActive(false);
        canvasHelp.SetActive(false);
        textInfo.enabled = false;
        textInfo2.enabled = false;
        timeLeftWantedStarsVisible = 50; 
        soundRingtone = GameObject.Find("/Sound/Ringtone").GetComponent<AudioSource>();
        soundMission1 = GameObject.Find("/Sound/Mission1").GetComponent<AudioSource>();
    }

    private Vector2 WorldToMapPosition(float x, float z)
    {
        // 0, 0 .. 1022, 833 <->  395, 1751 .. 2106, 4011
        // note that the map uses world Z as X-axis and world X as Y-axis
        return new Vector2((float)(1022f - (1022f * (z - 1751) / 2260f)), (float)(833f * (1 - ((x - 395) / 1711f))));
    }

    void OnGUI()
    {
        if (!phoneConversationActive && !Game.Instance.UseFreeCamera)
        {
            if (showMap)
            {
                UpdateMap();
            }
            else if (Game.Instance.ShowMiniMap && !showAchievements && !showHelp)
            {
                UpdateMiniMap();
            }
        }
    }

    public void RingPhone()
    {
        imagePhone.sprite = Resources.Load<Sprite>("uphone");
        imagePhone.enabled = true;
        phoneConversationActive = true;
        timePhoneActive = 0;
        phoneConversationStarted = false;
        soundRingtone.Play();
    }

    void Update()
    {
        if (phoneConversationActive)
        {
            timePhoneActive += Time.deltaTime;
            int yPos = (int)(-200 + timePhoneActive * 400);
            if (timePhoneActive > 11.5f)        // move phone down again
            {
                yPos = (int)(-200 + (12-timePhoneActive) * 400);
            }
            if (yPos > 0)
            {
                yPos = 0;
            }
            imagePhone.rectTransform.anchoredPosition = new Vector2(0, yPos);
            if (!phoneConversationStarted && timePhoneActive > 2)
            {
                phoneConversationStarted = true;
                imagePhone.sprite = Resources.Load<Sprite>("uphone_mission1");
                soundRingtone.Stop();
                soundMission1.Play();
            }
            if (timePhoneActive > 12)
            {
                phoneConversationActive = false;
                imagePhone.enabled = false;
            }
        }

        if (timeLeftDisplayMissionCompleted > 0)
        {
            timeLeftDisplayMissionCompleted -= Time.deltaTime;
            if (timeLeftDisplayMissionCompleted <= 0)
            {
                canvasMissionCompleted.SetActive(false);
            }
        }
        if (timeLeftWantedStarsVisible > 0)
        {
            timeLeftWantedStarsVisible--;
            if (timeLeftWantedStarsVisible <= 0)
            {
                timeLeftWantedStarsVisible = 50;
                wantedStarsActive = !wantedStarsActive;
                for (int i = 0; i<wantedStars.Count; i++)
                {
                    if (wantedStarsActive && Game.Instance.WantedLevel>i)
                    {
                        wantedStars[i].SetActive(true);
                    }
                    else
                    {
                        wantedStars[i].SetActive(false);
                    }
                }
            }
        }
    }

    private void UpdateMap()
    {
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), textureMap);
        Vector2 position = WorldToMapPosition(player.transform.position.x, player.transform.position.z);
        corner += Time.deltaTime;
        if(corner > 360)
        {
            corner = 0;
        }
        Matrix4x4 guiRotationMatrix = GUI.matrix; // set up for GUI rotation
//        GUIUtility.RotateAroundPivot(-player.transform.eulerAngles.y + 90, new Vector2(16 + Screen.width * (position.x / mapTextureWidth), 16 + Screen.height * (position.y / mapTextureHeight)));
        GUIUtility.RotateAroundPivot(player.GetComponent<ThirdPersonController>().CinemachineTargetYaw - 90, new Vector2(16 + Screen.width * (position.x / mapTextureWidth), 16 + Screen.height * (position.y / mapTextureHeight)));
        GUI.DrawTexture(new Rect(Screen.width * (position.x / mapTextureWidth), Screen.height * (position.y / mapTextureHeight), 32, 32), texturePlayerPosition);
        GUI.matrix = guiRotationMatrix; //end GUI rotation
        if (Game.Instance.ActiveMission != null)
        {
            position = WorldToMapPosition(Game.Instance.ActiveMission.CurrentDestination().x, Game.Instance.ActiveMission.CurrentDestination().y);
            GUI.DrawTexture(new Rect(Screen.width * (position.x / mapTextureWidth), Screen.height * (position.y / mapTextureHeight), 32, 32), textureDestination);
        }
        else
        {
            foreach (Mission mission in Game.Instance.Missions)
            {
                if (!mission.HasFinished)
                {
                    position = WorldToMapPosition(mission.MissionGuy.transform.position.x, mission.MissionGuy.transform.position.z);
                    GUI.DrawTexture(new Rect(Screen.width * (position.x / mapTextureWidth), Screen.height * (position.y / mapTextureHeight), 32, 32), textureMission);
                }
            }
        }
    }

    public void DisplayMissionCompleted(string medalText, string missionTime)
    {
        canvasMissionCompleted.SetActive(true);
        textMissionMedal.text = medalText + " medal";
        textMissionTime.text = "mission time: " + missionTime + "s ";
        imageMedal.sprite = Resources.Load<Sprite>(medalText);
        timeLeftDisplayMissionCompleted = 12;
    }

    public void ToggleAchievementsScreen()
    {
        showAchievements = !showAchievements;
        if (showAchievements)
        {
            Progress.Instance.UpdateProgressScreen();
        }
        showHelp = false;
        canvasHelp.SetActive(false);
        canvasAchievements.SetActive(showAchievements);
    }

    public void ToggleHelpScreen()
    {
        showHelp = !showHelp;
        showAchievements = false;
        canvasAchievements.SetActive(false); 
        canvasHelp.SetActive(showHelp);
    }

    private void UpdateMiniMap()
    {
        Vector2 position = WorldToMapPosition(player.transform.position.x, player.transform.position.z);
        float lowerleftCornerMinimapInMapTextureX = position.x - (minimapWidth / 2);
        float lowerleftCornerMinimapInMapTextureY = position.y + (minimapHeight / 2);
        if (lowerleftCornerMinimapInMapTextureX < 0)
            lowerleftCornerMinimapInMapTextureX = 0;
        if (lowerleftCornerMinimapInMapTextureX > mapTextureWidth - minimapWidth)
            lowerleftCornerMinimapInMapTextureX = mapTextureWidth - minimapWidth;
        if (lowerleftCornerMinimapInMapTextureY < minimapHeight)
            lowerleftCornerMinimapInMapTextureY = minimapHeight;
        if (lowerleftCornerMinimapInMapTextureY > mapTextureHeight)
            lowerleftCornerMinimapInMapTextureY = mapTextureHeight;
        float minimapPercentageInTextureMapX = lowerleftCornerMinimapInMapTextureX / mapTextureWidth;
        float minimapPercentageInTextureMapY = 1 - (lowerleftCornerMinimapInMapTextureY / mapTextureHeight);

        GUI.DrawTexture(rectFrame, textureFrame);
        GUI.DrawTextureWithTexCoords(new Rect(minimapPositionX, minimapPositionY, minimapWidth, minimapHeight), textureMap,
            new Rect(minimapPercentageInTextureMapX, minimapPercentageInTextureMapY,
                    ratioTextureToMinimapX, ratioTextureToMinimapY), false);

        if (position.x > 0 && position.y > 0 && position.x < mapTextureWidth && position.y < mapTextureHeight)
        {
//            minimapPlayer.position = new Vector3(minimapPositionX + position.x - lowerleftCornerMinimapInMapTextureX, position.y - lowerleftCornerMinimapInMapTextureY + minimapHeight, 0);
//            minimapPlayer.rotation = Quaternion.Euler(0, 0, -player.transform.rotation.eulerAngles.y);
            Matrix4x4 guiRotationMatrix = GUI.matrix; // set up for GUI rotation
            GUIUtility.RotateAroundPivot(player.GetComponent<ThirdPersonController>().CinemachineTargetYaw - 90, new Vector2(12 + minimapPositionX + position.x - lowerleftCornerMinimapInMapTextureX, 12 + minimapPositionY + position.y - lowerleftCornerMinimapInMapTextureY + minimapHeight));
//            GUIUtility.RotateAroundPivot(-player.transform.eulerAngles.y + 90, new Vector2(12 + minimapPositionX + position.x - lowerleftCornerMinimapInMapTextureX, 12 + minimapPositionY + position.y - lowerleftCornerMinimapInMapTextureY + minimapHeight));
            GUI.DrawTexture(new Rect(minimapPositionX + position.x - lowerleftCornerMinimapInMapTextureX, minimapPositionY + position.y - lowerleftCornerMinimapInMapTextureY + minimapHeight, 24, 24), texturePlayerPosition);
            GUI.matrix = guiRotationMatrix; //end GUI rotation    
        }

        if (Game.Instance.ActiveMission != null)
        {
            position = WorldToMapPosition(Game.Instance.ActiveMission.CurrentDestination().x, Game.Instance.ActiveMission.CurrentDestination().y);
            if (position.x > lowerleftCornerMinimapInMapTextureX && position.y > lowerleftCornerMinimapInMapTextureY - minimapHeight && position.x < lowerleftCornerMinimapInMapTextureX + minimapWidth - 20 && position.y < lowerleftCornerMinimapInMapTextureY - 20)
            {
                GUI.DrawTexture(new Rect(minimapPositionX + position.x - lowerleftCornerMinimapInMapTextureX, minimapPositionY + position.y - lowerleftCornerMinimapInMapTextureY + minimapHeight, 20, 20), textureDestination);
            }
        }
        else
        {
            foreach (Mission mission in Game.Instance.Missions)
            {
                if (!mission.HasFinished)
                {
                    position = WorldToMapPosition(mission.MissionGuy.transform.position.x, mission.MissionGuy.transform.position.z);
                    if (position.x > lowerleftCornerMinimapInMapTextureX && position.y > lowerleftCornerMinimapInMapTextureY - minimapHeight && position.x < lowerleftCornerMinimapInMapTextureX + minimapWidth - 20 && position.y < lowerleftCornerMinimapInMapTextureY - 20)
                    {
                        GUI.DrawTexture(new Rect(minimapPositionX + position.x - lowerleftCornerMinimapInMapTextureX, minimapPositionY + position.y - lowerleftCornerMinimapInMapTextureY + minimapHeight, 20, 20), textureMission);
                    }
                }
            }
        }
    }
}
