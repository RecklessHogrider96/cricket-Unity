using UnityEngine;

/// <summary>
/// Central runtime state for the cricket game.
/// Holds the active bowler selection and delegates all data lookups
/// to CricketDataController — never touches ScriptableObjects directly.
/// </summary>
public class CricketGameModel : Singleton<CricketGameModel>
{
    [SerializeField] private BowlerType bowlerType;
    [SerializeField] private BowlerBowlingArm bowlingArm;
    [SerializeField] private CricketDataController cricketDataController;

    // ── Bowler selection ────────────────────────────────────────────────────

    public void SetBowlerType(BowlerType type) => bowlerType = type;
    public BowlerType GetBowlerType() => bowlerType;

    public void SetBowlingArm(BowlerBowlingArm arm) => bowlingArm = arm;
    public BowlerBowlingArm GetBowlingArm() => bowlingArm;

    // ── Data access ─────────────────────────────────────────────────────────

    public CricketDataController GetDataController() => cricketDataController;

    // ── Throw parameter assembly ─────────────────────────────────────────────

    /// <summary>
    /// Builds a BallThrowData for the current bowler type and arm by sampling
    /// all ranges from the matching BowlerConfigSO.
    /// Returns safe fallback data and logs an error if no config is found.
    /// </summary>
    public BallThrowData GetThrowParameters(Vector3 bounceTarget)
    {
        BowlerConfig config = cricketDataController.GetBowlerConfig(bowlerType, bowlingArm);

        if (config == null)
        {
            Debug.LogError("[CricketGameModel] GetThrowParameters: bowler config not found. " +
                           "Returning fallback throw data.");
            return new BallThrowData
            {
                bounceTarget = bounceTarget,
                speed        = 30f,
                spin         = 0f,
                swingAmount  = 0f,
                bowlingArm   = bowlingArm
            };
        }

        BowlerConfigSO so = config.bowlerConfigSO;

        float speed  = Random.Range(so.minSpeed, so.maxSpeed);
        float spin   = Random.Range(so.minSpin,  so.maxSpin);
        float swing  = Random.Range(so.minSwing, so.maxSwing) * so.swingDirection;

        return new BallThrowData
        {
            bounceTarget = bounceTarget,
            speed        = speed,
            spin         = spin,
            swingAmount  = swing,
            bowlingArm   = bowlingArm
        };
    }
}
