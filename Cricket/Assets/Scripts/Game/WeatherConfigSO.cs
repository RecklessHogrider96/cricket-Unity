using UnityEngine;

/// <summary>
/// Defines a named weather condition and the runtime offsets it applies to the ball simulation.
///
/// ── How offsets are applied ───────────────────────────────────────────────────
///   None of these values permanently change the SurfaceConfigSO or BowlerConfigSO assets.
///   They are folded into the trajectory calculation at throw time only.
///
///   Phase 1 (flight):
///     effectiveSwing = (bowlerSwing × swingMultiplier) + windX
///     effectiveSpeed = bowlerSpeed + windZ                    (changes flight time T)
///     effectiveG     = gravity − windY                        (updraft reduces gravity)
///
///   Phase 2a (bounce):
///     bounceFactor   = surface.bounceFactor   + pitchBounceDelta    (clamped 0–1)
///     frictionFactor = surface.frictionFactor + pitchFrictionDelta  (clamped 0–1)
///     spinApplied    = bowlerSpin × spinGripMultiplier
///
///   Phase 2b (rolling):
///     rollingFriction = outfield.rollingFriction + outfieldRollingDelta (clamped 0.8–1)
/// </summary>
[CreateAssetMenu(fileName = "WeatherConfig", menuName = "Scriptable Objects/WeatherConfig")]
public class WeatherConfigSO : ScriptableObject
{
    // ── Identity ─────────────────────────────────────────────────────────────

    [Header("Weather Identity")]

    [Tooltip("Display name shown in the weather selection dropdown in the HUD.\n" +
             "Example: \"Overcast\", \"Perth Baked\", \"Gusty Storm\"")]
    public string weatherName = "Clear";

    // ── Wind ─────────────────────────────────────────────────────────────────

    [Header("Wind")]

    [Tooltip("Lateral wind force along world X-axis, in m/s².\n\n" +
             "Added directly to the ball's swing acceleration during flight.\n\n" +
             "Positive (+X) → pushes ball right — boosts in-swing, fights out-swing.\n" +
             "Negative (−X) → pushes ball left  — boosts out-swing, fights in-swing.\n\n" +
             "Suggested range: −10 to +10.\n" +
             "Around ±5 is a noticeable crosswind; ±10 is a strong gale.")]
    public float windX = 0f;

    [Tooltip("Headwind or tailwind along world Z-axis, in m/s.\n\n" +
             "Added to the bowler's delivery speed when computing flight time T.\n\n" +
             "Positive → tailwind  (ball arrives faster, pitches fuller — like bowling downhill).\n" +
             "Negative → headwind  (ball arrives slower, pitches shorter — delivery feels shorter).\n\n" +
             "Suggested range: −6 to +6.\n" +
             "A strong headwind at −4 m/s is equivalent to knocking ~2–3 m/s off effective speed.")]
    public float windZ = 0f;

    [Tooltip("Vertical air movement along world Y-axis, in m/s².\n\n" +
             "Subtracted from effective gravity during Phase 1 flight only.\n\n" +
             "Positive → updraft   (ball floats slightly — pitches fuller, hangs in air).\n" +
             "Negative → downdraft (ball dips earlier — pitches shorter than intended).\n\n" +
             "Keep small — the effect is subtle.\n" +
             "Suggested range: −3 to +3.")]
    public float windY = 0f;

    // ── Atmosphere ────────────────────────────────────────────────────────────

    [Header("Atmosphere — affects swing during flight")]

    [Tooltip("Multiplier applied to the bowler's swing amount before the trajectory is built.\n\n" +
             "Models how atmospheric conditions alter conventional swing:\n\n" +
             "• > 1.0  Overcast / humid — dense air helps the ball swing more.\n" +
             "         Famous in English conditions. Lord's under cloud = chaos.\n" +
             "• = 1.0  Neutral (clear, moderate conditions).\n" +
             "• < 1.0  Dry heat or wet ball — swing reduced. The ball dries out\n" +
             "         in hot sun or a rain-wet ball loses its polished side.\n\n" +
             "Typical presets:\n" +
             "Clear            : 0.9\n" +
             "Overcast         : 1.4\n" +
             "Heavy overcast   : 1.8\n" +
             "Drizzle          : 0.85\n" +
             "Wet ball / rain  : 0.4 – 0.65")]
    [Range(0f, 3f)]
    public float swingMultiplier = 1f;

    // ── Pitch Conditions ──────────────────────────────────────────────────────

    [Header("Pitch Conditions — affect Phase 2a bounce")]

    [Tooltip("Delta added to the pitch SurfaceConfigSO's bounceFactor at runtime.\n" +
             "The base asset is never modified — this is applied per-delivery only.\n\n" +
             "Positive → harder / drier pitch — ball jumps more aggressively.\n" +
             "Negative → soft / wet pitch     — ball keeps low, batsman can't get on top.\n\n" +
             "Typical values:\n" +
             "Baked / scorching day  : +0.10 to +0.20\n" +
             "Normal conditions      :  0.00\n" +
             "Light rain             : −0.05 to −0.10\n" +
             "Heavy rain (just ended): −0.15 to −0.25\n\n" +
             "Clamped to [0, 1] at runtime.")]
    [Range(-0.5f, 0.5f)]
    public float pitchBounceDelta = 0f;

    [Tooltip("Delta added to the pitch SurfaceConfigSO's frictionFactor at runtime.\n" +
             "The base asset is never modified — this is applied per-delivery only.\n\n" +
             "Positive → more grip at the seam — ball holds up, spinner gets purchase.\n" +
             "Negative → ball skids through    — wet / slick surface, less grip on impact.\n" +
             "           Dangerous for batsmen: ball arrives faster and lower than expected.\n\n" +
             "Typical values:\n" +
             "Dry seamer pitch    : +0.02 to +0.05\n" +
             "Normal              :  0.00\n" +
             "Wet / rain-affected : −0.05 to −0.15\n\n" +
             "Clamped to [0, 1] at runtime.")]
    [Range(-0.5f, 0.5f)]
    public float pitchFrictionDelta = 0f;

    [Tooltip("Multiplier applied to the spin velocity added at the first bounce.\n\n" +
             "• > 1.0  Dry / rough / crumbling surface — seam or finger-spin grips hard.\n" +
             "         Classic Day 4–5 Test pitch: spinners unplayable.\n" +
             "• = 1.0  Neutral.\n" +
             "• < 1.0  Wet / damp — ball skids through on impact, spin barely takes.\n" +
             "         Batsman can pad up safely; spinners suffer.\n\n" +
             "Typical values:\n" +
             "Day 4–5 dusty pitch   : 1.5 – 2.0\n" +
             "Normal dry            : 1.0\n" +
             "Dew / after rain      : 0.4 – 0.7\n" +
             "Waterlogged pitch     : 0.2 – 0.4")]
    [Range(0f, 3f)]
    public float spinGripMultiplier = 1f;

    // ── Outfield Conditions ───────────────────────────────────────────────────

    [Header("Outfield Conditions — affect Phase 2b rolling")]

    [Tooltip("Delta added to the outfield SurfaceConfigSO's rollingFriction per simulation step.\n" +
             "The base asset is never modified — this is applied per-delivery only.\n\n" +
             "rollingFriction is a per-step velocity multiplier (e.g. 0.978 = 2.2% decay per step).\n\n" +
             "Negative → damp / wet outfield — ball decelerates faster, stops well short.\n" +
             "           Heavy dew or just-rained outfields behave this way.\n" +
             "Positive → dry / fast outfield — ball races away, fewer steps to boundary.\n" +
             "           Sub-continental grounds in peak summer.\n\n" +
             "Typical values:\n" +
             "Fast dry outfield     : +0.010 to +0.020\n" +
             "Normal                :  0.000\n" +
             "Damp outfield         : −0.010 to −0.020\n" +
             "Dew / heavy rain      : −0.020 to −0.040\n\n" +
             "Clamped to [0.8, 1.0] at runtime.")]
    [Range(-0.1f, 0.1f)]
    public float outfieldRollingDelta = 0f;
}
