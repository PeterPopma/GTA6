using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Progress : MonoBehaviour
{
    public static Progress Instance;
    float distanceTravelled;
    int kills;
    int missionsCompleted;
    int diamonds = 0;
    [SerializeField] TextMeshProUGUI textDistance;
    [SerializeField] TextMeshProUGUI textMissions;
    [SerializeField] TextMeshProUGUI textDiamonds;
    [SerializeField] TextMeshProUGUI textKills;
    [SerializeField] TextMeshProUGUI textCompleted;
    [SerializeField] GameObject panelMedals;
    int medalsBronze;
    int medalsSilver;
    int medalsGold;

    public float DistanceTravelled { get => distanceTravelled; set => distanceTravelled = value; }
    public int Kills { get => kills; set => kills = value; }
    public int MissionsCompleted { get => missionsCompleted; set => missionsCompleted = value; }
    public int Diamonds { get => diamonds; set => diamonds = value; }
    public int MedalsGold { get => medalsGold; set => medalsGold = value; }
    public int MedalsSilver { get => medalsSilver; set => medalsSilver = value; }
    public int MedalsBronze { get => medalsBronze; set => medalsBronze = value; }

    public void Awake()
    {
        Instance = this;
    }

    public void UpdateProgressScreen()
    {
        textDistance.text = "Distance travelled: " + (distanceTravelled * .0005).ToString("0.0") + " km";
        textKills.text = "Kills: " + kills;
        textMissions.text = "Missions completed: " + missionsCompleted + "/" + Game.Instance.Missions.Count;
        textDiamonds.text = "Diamonds: " + diamonds + "/100";
        float percentageCompleted = diamonds/100f * 20 + 80 * missionsCompleted / (float)Game.Instance.Missions.Count;
        textCompleted.text = "Game completed: " + percentageCompleted.ToString("0.0") + " %";
        int imageNumber = 1;

        for (int i = 0; i < MedalsGold; i++)
        {
            Image currentImage = panelMedals.transform.Find("medal" + imageNumber).GetComponent<Image>();
            currentImage.sprite = Resources.Load<Sprite>("gold");                
            currentImage.enabled = true;
            imageNumber++;
        }
        for (int i = 0; i < MedalsSilver; i++)
        {
            Image currentImage = panelMedals.transform.Find("medal" + imageNumber).GetComponent<Image>();
            currentImage.sprite = Resources.Load<Sprite>("silver");
            currentImage.enabled = true;
            imageNumber++;
        }
        for (int i = 0; i < MedalsBronze; i++)
        {
            Image currentImage = panelMedals.transform.Find("medal" + imageNumber).GetComponent<Image>();
            currentImage.sprite = Resources.Load<Sprite>("bronze");
            currentImage.enabled = true;
            imageNumber++;
        }
    }
}
