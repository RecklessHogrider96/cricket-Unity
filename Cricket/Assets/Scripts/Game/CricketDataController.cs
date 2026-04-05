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

    [Header("Weather Roster")]
    [Tooltip("All available weather conditions shown in the HUD dropdown.\n" +
             "Run Cricket → Generate Weather Presets to create the WeatherRoster asset\n" +
             "and all presets automatically. Then drag WeatherRoster.asset here.")]
    [SerializeField] private WeatherRosterSO weatherRoster;

    // ── Accessors ─────────────────────────────────────────────────────────────

    public CricketGameConstants    GetGameConstants()          => gameConstants;
    public BowlerRosterSO          GetBowlerRoster()           => bowlerRoster;
    public SurfaceConfigSO         GetPitchSurfaceConfig()     => pitchSurfaceConfig;
    public SurfaceConfigSO         GetOutfieldSurfaceConfig()  => outfieldSurfaceConfig;
    public WeatherRosterSO         GetWeatherRoster()          => weatherRoster;

    /// <summary>
    /// Returns the surface config for the given world-space position.
    /// Inside pitch XZ bounds → pitch config; everywhere else → outfield config.
    /// </summary>
    public SurfaceConfigSO GetSurfaceConfig(Vector3 worldPosition)
    {
        return gameConstants.IsOnPitch(worldPosition) ? pitchSurfaceConfig : outfieldSurfaceConfig;
    }

    // ── Weather summary ───────────────────────────────────────────────────────

    /// <summary>
    /// Generates a human-readable summary of every effect a WeatherConfigSO applies.
    /// Called by the HUD whenever the weather dropdown changes, so the player can
    /// immediately understand the conditions without opening the asset inspector.
    ///
    /// Format (four lines):
    ///   Wind:        lateral crosswind · headwind/tailwind [· vertical if significant]
    ///   Atmosphere:  swing multiplier effect
    ///   Pitch:       bounce delta · friction delta · spin grip multiplier
    ///   Outfield:    rolling friction delta
    /// </summary>
    public string GenerateWeatherSummary(WeatherConfigSO weather)
    {
        if (weather == null)
            return "No weather selected.";

        var sb = new System.Text.StringBuilder();

        // ── Wind ─────────────────────────────────────────────────────────────

        bool hasWindX = Mathf.Abs(weather.windX) >= 0.5f;
        bool hasWindZ = Mathf.Abs(weather.windZ) >= 0.5f;
        bool hasWindY = Mathf.Abs(weather.windY) >= 0.2f;

        string windX = hasWindX
            ? (weather.windX < 0
                ? $"Crosswind pushing ball LEFT ({weather.windX:F1} m/s²)"
                : $"Crosswind pushing ball RIGHT (+{weather.windX:F1} m/s²)")
            : "No crosswind";

        string windZ = hasWindZ
            ? (weather.windZ < 0
                ? $"headwind −{Mathf.Abs(weather.windZ):F1} m/s (pitches shorter)"
                : $"tailwind +{weather.windZ:F1} m/s (pitches fuller)")
            : "no headwind";

        string windY = hasWindY
            ? (weather.windY > 0
                ? "updraft (ball floats, pitches fuller)"
                : "downdraft (ball dips, pitches shorter)")
            : "";

        sb.Append("Wind: ");
        sb.Append(windX);
        sb.Append(" · ");
        sb.Append(windZ);
        if (hasWindY) { sb.Append(" · "); sb.Append(windY); }
        sb.AppendLine(".");

        // ── Atmosphere ────────────────────────────────────────────────────────

        float sm = weather.swingMultiplier;
        string atmos = (sm >= 0.95f && sm <= 1.05f)
            ? "Neutral — no swing change."
            : sm > 1.05f
                ? $"Swing BOOSTED ×{sm:F2} — overcast / humid conditions help the ball move."
                : $"Swing REDUCED ×{sm:F2} — dry heat or wet ball limits movement.";

        sb.Append("Atmosphere: ");
        sb.AppendLine(atmos);

        // ── Pitch ─────────────────────────────────────────────────────────────

        float bd = weather.pitchBounceDelta;
        string bounce = Mathf.Abs(bd) < 0.02f
            ? "Normal bounce"
            : bd > 0
                ? $"Harder surface — ball JUMPS higher (+{bd:F2} bounce)"
                : $"Soft surface — ball STAYS LOW ({bd:F2} bounce)";

        float fd = weather.pitchFrictionDelta;
        string friction = Mathf.Abs(fd) < 0.01f
            ? "normal surface grip"
            : fd > 0
                ? $"extra grip on impact (+{fd:F2} friction)"
                : $"ball SKIDS through on impact ({fd:F2} friction)";

        float sg = weather.spinGripMultiplier;
        string spinGrip = Mathf.Abs(sg - 1f) < 0.05f
            ? "normal spin grip."
            : sg > 1f
                ? $"dry / rough — spin GRIPS harder (×{sg:F2})."
                : $"wet / damp — spin REDUCED (×{sg:F2}), ball skids past bat edge.";

        sb.Append("Pitch: ");
        sb.Append(bounce);
        sb.Append(" · ");
        sb.Append(friction);
        sb.Append(" · ");
        sb.AppendLine(spinGrip);

        // ── Outfield ──────────────────────────────────────────────────────────

        float rd = weather.outfieldRollingDelta;
        string outfield = Mathf.Abs(rd) < 0.002f
            ? "Normal rolling speed."
            : rd > 0
                ? $"Fast / dry — ball races away (+{rd:F3} rolling)."
                : $"Damp — ball slows quickly ({rd:F3} rolling).";

        sb.Append("Outfield: ");
        sb.Append(outfield);

        return sb.ToString();
    }
}
