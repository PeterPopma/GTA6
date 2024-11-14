using System.Collections.Generic;
using UnityEngine;

public class Mission : MonoBehaviour
{
    [SerializeField] GameObject missionGuy;
    AudioSource soundLevelUp;
    private string description;
    private bool hasFinished;
    private bool isActive;
    private List<Vector2> destinationLocations = new();
    private float startingTime;
    private int currentDestinationIndex;
    float timeLeftDelay;
    protected float maxTimeGold;
    protected float maxTimeSilver;

    public bool HasFinished { get => hasFinished; set => hasFinished = value; }
    public string Description { get => description; set => description = value; }
    public float StartingTime { get => startingTime; set => startingTime = value; }
    public List<Vector2> DestinationLocations { get => destinationLocations; set => destinationLocations = value; }
    public bool IsActive { get => isActive; set => isActive = value; }
    public GameObject MissionGuy { get => missionGuy; set => missionGuy = value; }
    public string Name { get => name; set => name = value; }

    protected virtual void Start()
    {
        soundLevelUp = GameObject.Find("/Sound/LevelUp").GetComponent<AudioSource>();
    }

    protected virtual void Awake()
    {
    }

    protected virtual void Update()
    {
    }

    protected virtual void FixedUpdate()
    {
        if (timeLeftDelay > 0)
        {
            timeLeftDelay -= Time.deltaTime;
            if (timeLeftDelay <= 0)
            {
                soundLevelUp.Play();
                float totalTime = Time.time - startingTime;
                string medalText;
                if (totalTime <= maxTimeGold)
                {
                    Progress.Instance.MedalsGold++;
                    medalText = "gold";
                } else if (totalTime <= maxTimeSilver)
                {
                    Progress.Instance.MedalsSilver++;
                    medalText = "silver";
                }
                else
                {
                    Progress.Instance.MedalsBronze++; 
                    medalText = "bronze";
                }

                UI ui = GameObject.Find("Scripts/UI").GetComponent<UI>();
                ui.DisplayMissionCompleted(medalText, totalTime.ToString("0"));
                Game.Instance.ActiveMission.HasFinished = true;
                Game.Instance.ActiveMission = null;
                missionGuy.SetActive(false);
            }
        }
    }

    virtual public void StartMission()
    {
        if (!IsActive && !HasFinished)
        {
            isActive = true;
            startingTime = Time.time;
            Game.Instance.ActiveMission = this;
            Game.Instance.DisplayMission(Description);
        }
    }

    public Vector2 CurrentDestination()
    {
        return destinationLocations[currentDestinationIndex];
    }

    public void NextDestination()
    {
        if (currentDestinationIndex < destinationLocations.Count - 1)
        {
            currentDestinationIndex++;
        }
    }

    public void CompleteMission(float delay = 0.01f)
    {
        Progress.Instance.MissionsCompleted++;
        timeLeftDelay = delay;
    }

}
