using UnityEngine;

/// <summary>
/// Defines the identity and delivery capability of one specific named bowler.
/// Create one asset per bowler in your squad (e.g. "Shane Warne", "Wasim Akram").
///
/// ── Direction convention ──────────────────────────────────────────────────────
///   Both swing and spin directions use the same signed range:
///     -1  =  ball curves / turns toward world -X
///     +1  =  ball curves / turns toward world +X
///     0   =  no deviation
///
///   The HUD slider range is computed as [minDir × maxMagnitude, maxDir × maxMagnitude].
///   A pure out-swinger would have minSwingDirection=-1, maxSwingDirection=0.
///   A dual swinger would have minSwingDirection=-1, maxSwingDirection=1.
///   Ensure minSwingDirection ≤ maxSwingDirection (same for spin).
/// </summary>
[CreateAssetMenu(fileName = "BowlerConfigSO", menuName = "Scriptable Objects/BowlerConfigSO")]
public class BowlerConfigSO : ScriptableObject
{
    [Header("Bowler Identity")]
    [Tooltip("Display name shown in the bowler selection dropdown.")]
    public string bowlerName = "New Bowler";

    [Tooltip("Determines which delivery controls appear in the HUD.\n" +
             "Fast   → Speed + Swing\n" +
             "Medium → Speed + Swing + Spin\n" +
             "Spin   → Speed + Spin")]
    public BowlerType bowlerType = BowlerType.Fast;

    [Tooltip("Left / Right → arm is fixed; model auto-sets it when this bowler is selected " +
             "and the HUD hides the arm selector.\n" +
             "Both → arm selector is shown so the user can choose per delivery.")]
    public BowlerArmPreference bowlerArm = BowlerArmPreference.Right;

    [Header("Speed (m/s)")]
    [Tooltip("Fast: 35–42  |  Medium: 28–35  |  Spin: 18–25")]
    public float minSpeed = 30f;
    public float maxSpeed = 40f;

    [Header("Swing Magnitude (m/s²)  —  lateral acceleration during flight")]
    [Tooltip("Fast: 0.3–1.5  |  Medium: 0.2–1.2  |  Spin drift: 0–0.3")]
    public float minSwing = 0f;
    public float maxSwing = 1.5f;

    [Header("Swing Direction Range  [−1 = world −X  …  +1 = world +X]")]
    [Tooltip("Slider lower bound. Set to −1 for full out-swing capability, 0 if bowler cannot out-swing.")]
    [Range(-1f, 1f)]
    public float minSwingDirection = 0f;
    [Tooltip("Slider upper bound. Set to +1 for full in-swing capability, 0 if bowler cannot in-swing.")]
    [Range(-1f, 1f)]
    public float maxSwingDirection = 1f;

    [Header("Spin Magnitude (m/s)  —  lateral velocity added after first bounce")]
    [Tooltip("Fast seam: 0–1.5  |  Medium cutter: 0–2.5  |  Spin: 1.5–5")]
    public float minSpin = 0f;
    public float maxSpin = 2f;

    [Header("Spin Direction Range  [−1 = world −X  …  +1 = world +X]")]
    [Tooltip("Slider lower bound. Off-break spins −X so set −1 here if bowler bowls off-break.")]
    [Range(-1f, 1f)]
    public float minSpinDirection = 0f;
    [Tooltip("Slider upper bound. Leg-break spins +X so set +1 here if bowler bowls leg-break.")]
    [Range(-1f, 1f)]
    public float maxSpinDirection = 1f;
}
