using UnityEngine;

/// <summary>
/// Single read point for all ScriptableObject data.
/// MonoBehaviours never reference SOs directly — they go through here.
/// </summary>
public class CricketDataController : MonoBehaviour
{
    [SerializeField] private CricketGameConstants gameConstants;

    [Header("Bowler Roster")]
    [Tooltip("The full squad — one BowlerConfigSO per named bowler.")]
    [SerializeField] private BowlerRosterSO bowlerRoster;

    [Header("Surface Configs")]
    [Tooltip("Physical properties of the pitch strip.")]
    [SerializeField] private SurfaceConfigSO pitchSurfaceConfig;
    [Tooltip("Physical properties of the outfield.")]
    [SerializeField] private SurfaceConfigSO outfieldSurfaceConfig;

    // ── Accessors ─────────────────────────────────────────────────────────────

    public CricketGameConstants GetGameConstants()    => gameConstants;
    public BowlerRosterSO       GetBowlerRoster()     => bowlerRoster;
    public SurfaceConfigSO      GetPitchSurfaceConfig()    => pitchSurfaceConfig;
    public SurfaceConfigSO      GetOutfieldSurfaceConfig() => outfieldSurfaceConfig;

    /// <summary>
    /// Returns the surface config for the given world-space position.
    /// Inside pitch XZ bounds → pitch config; everywhere else → outfield config.
    /// </summary>
    public SurfaceConfigSO GetSurfaceConfig(Vector3 worldPosition)
    {
        return gameConstants.IsOnPitch(worldPosition) ? pitchSurfaceConfig : outfieldSurfaceConfig;
    }
}
