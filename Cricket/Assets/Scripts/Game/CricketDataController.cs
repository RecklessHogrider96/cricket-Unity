using UnityEngine;

/// <summary>
/// Single read point for all ScriptableObject data.
/// MonoBehaviours never reference SOs directly — they go through here.
/// </summary>
public class CricketDataController : MonoBehaviour
{
    [SerializeField] private CricketGameConstants gameConstants;
    [SerializeField] private BowlerConfigData bowlerConfigData;

    [Header("Surface Configs")]
    [Tooltip("Physical properties of the pitch strip (harder, less friction).")]
    [SerializeField] private SurfaceConfigSO pitchSurfaceConfig;
    [Tooltip("Physical properties of the outfield (softer, more friction).")]
    [SerializeField] private SurfaceConfigSO outfieldSurfaceConfig;

    public CricketGameConstants GetGameConstants() => gameConstants;

    public SurfaceConfigSO GetPitchSurfaceConfig() => pitchSurfaceConfig;

    public SurfaceConfigSO GetOutfieldSurfaceConfig() => outfieldSurfaceConfig;

    /// <summary>
    /// Returns the surface config appropriate for the given world-space position.
    /// On pitch → pitch config, everywhere else → outfield config.
    /// </summary>
    public SurfaceConfigSO GetSurfaceConfig(Vector3 worldPosition)
    {
        return gameConstants.IsOnPitch(worldPosition) ? pitchSurfaceConfig : outfieldSurfaceConfig;
    }

    /// <summary>
    /// Finds the BowlerConfig matching the given type and arm.
    /// Logs a descriptive error and returns null if no match exists —
    /// callers must handle the null case.
    /// </summary>
    public BowlerConfig GetBowlerConfig(BowlerType bowlerType, BowlerBowlingArm bowlerBowlingArm)
    {
        foreach (var config in bowlerConfigData.bowlerConfigs)
        {
            if (config.bowlerType == bowlerType && config.bowlerBowlingArm == bowlerBowlingArm)
                return config;
        }

        Debug.LogError($"[CricketDataController] No BowlerConfig found for {bowlerType} / {bowlerBowlingArm}. " +
                       "Check the BowlerConfigData asset.");
        return null;
    }
}
