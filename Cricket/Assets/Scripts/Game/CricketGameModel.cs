using System;
using UnityEngine;

public class CricketGameModel : Singleton<CricketGameModel>
{
    [SerializeField] private BowlerType bowlerType;
    [SerializeField] private BowlerBowlingArm bowlingArm;
    [SerializeField] private CricketDataController cricketDataController;

    public float GetBowlerMaxSpeed()
    {
        return cricketDataController.GetBowlerConfig(bowlerType, bowlingArm).bowlerConfigSO.maxSpeed;
    }

    public float GetBounceFactor()
    {
        return cricketDataController.GetBounceFactor();
    }

    public float GetGravity()
    {
        return cricketDataController.GetGameConstants().gravity;
    }
}
