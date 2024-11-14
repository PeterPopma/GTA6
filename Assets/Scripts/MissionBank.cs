using UnityEngine;

public class MissionBank : Mission
{
    protected override void Awake()
    {
        base.Awake();
        Name = "bank";
        Description = "TODO";
        DestinationLocations.Add(new Vector2(756.1f, 3053.1f));
        DestinationLocations.Add(new Vector2(1087.83f, 3373.9f));

        maxTimeGold = 120;
        maxTimeSilver = 240;
    }

    protected override void Start()
    {
        base.Start();
    }

    protected override void Update()
    {
        base.Update();
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
    }

}
