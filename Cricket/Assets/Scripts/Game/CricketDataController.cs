using UnityEngine;

public class CricketDataController : MonoBehaviour
{
    [SerializeField] private CricketGameConstants gameConstants;
    [SerializeField] private BowlerConfigData bowlerConfigData;
    // Control the bounce intensity
    [SerializeField] private float bounceFactor = 0.7f;

    public float GetBounceFactor()
    {
        return bounceFactor;
    }

    public CricketGameConstants GetGameConstants()
    {
        return gameConstants;
    }

    public BowlerConfig GetBowlerConfig(BowlerType bowlerType, BowlerBowlingArm bowlerBowlingArm)
    {
        foreach (var bowlerConfig in bowlerConfigData.bowlerConfigs)
        {
            if (bowlerConfig.bowlerType == bowlerType && bowlerConfig.bowlerBowlingArm == bowlerBowlingArm)
            {
                return bowlerConfig;
            }
        }

        return null;
    }
}
