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
    // ── Identity ─────────────────────────────────────────────────────────────

    [Header("Bowler Identity")]

    [Tooltip("Display name shown in the bowler selection dropdown in the HUD.\n" +
             "Example: \"Jasprit Bumrah\", \"Shane Warne\"")]
    public string bowlerName = "New Bowler";

    [Tooltip("Controls which delivery panels are visible in the HUD:\n\n" +
             "• Fast   → Speed + Swing sliders only\n" +
             "• Medium → Speed + Swing + Spin sliders\n" +
             "• Spin   → Speed + Spin sliders only")]
    public BowlerType bowlerType = BowlerType.Fast;

    [Tooltip("The bowling arm for this bowler:\n\n" +
             "• Right / Left → arm is fixed; selecting this bowler auto-sets the model arm " +
             "and the arm selector is hidden in the HUD.\n" +
             "• Both → shows an arm selector dropdown in the HUD so the player can choose " +
             "per delivery (useful for generic template bowlers).")]
    public BowlerArmPreference bowlerArm = BowlerArmPreference.Right;

    // ── Release Points ────────────────────────────────────────────────────────

    [Header("Release Points (World-Space)")]

    [Tooltip("World-space position from which the ball is released when bowling OVER the wicket.\n\n" +
             "Over the wicket = natural side for the bowler's arm:\n" +
             "• Right-arm bowler → stands to the LEFT  of the stumps (from batsman's view) → positive X\n" +
             "• Left-arm  bowler → stands to the RIGHT of the stumps (from batsman's view) → negative X\n\n" +
             "Typical values (right-arm, pitch at Z = 0 → 20 m, stumps at X = 0):\n" +
             "  X ≈ +0.3 to +0.5 m   (just outside the return crease)\n" +
             "  Y ≈  2.0 to  2.3 m   (release height — tall fast bowler ≈ 2.3 m)\n" +
             "  Z ≈  1.2 to  2.0 m   (just past the bowling crease)")]
    public Vector3 overTheWicketReleasePoint = new Vector3(0.4f, 2.1f, 1.5f);

    [Tooltip("World-space position from which the ball is released when bowling AROUND the wicket.\n\n" +
             "Around the wicket = opposite side to the bowler's natural arm:\n" +
             "• Right-arm bowler → stands to the RIGHT of the stumps (from batsman's view) → negative X\n" +
             "• Left-arm  bowler → stands to the LEFT  of the stumps (from batsman's view) → positive X\n\n" +
             "Typical values (right-arm):\n" +
             "  X ≈ −0.3 to −0.5 m\n" +
             "  Y ≈  2.0 to  2.3 m\n" +
             "  Z ≈  1.2 to  2.0 m\n\n" +
             "Around the wicket brings a different angle of attack to the batsman — particularly\n" +
             "effective for right-arm bowlers targeting left-handed batsmen and vice versa.")]
    public Vector3 aroundTheWicketReleasePoint = new Vector3(-0.4f, 2.1f, 1.5f);

    // ── Speed ─────────────────────────────────────────────────────────────────

    [Header("Speed (m/s)  —  how fast the ball travels down the pitch")]

    [Tooltip("Slowest delivery speed this bowler can bowl, in metres per second.\n\n" +
             "Typical ranges:\n" +
             "• Fast bowler  : 35 – 42 m/s  (126 – 151 km/h)\n" +
             "• Medium pacer : 28 – 35 m/s  (101 – 126 km/h)\n" +
             "• Spin bowler  : 18 – 25 m/s  ( 65 –  90 km/h)\n\n" +
             "The HUD speed slider's left end is clamped to this value.")]
    public float minSpeed = 30f;

    [Tooltip("Fastest delivery speed this bowler can bowl, in metres per second.\n\n" +
             "Typical ranges:\n" +
             "• Fast bowler  : 35 – 42 m/s  (126 – 151 km/h)\n" +
             "• Medium pacer : 28 – 35 m/s  (101 – 126 km/h)\n" +
             "• Spin bowler  : 18 – 25 m/s  ( 65 –  90 km/h)\n\n" +
             "The HUD speed slider's right end is clamped to this value.")]
    public float maxSpeed = 40f;

    // ── Swing ─────────────────────────────────────────────────────────────────

    [Header("Swing Magnitude (m/s²)  —  lateral acceleration applied during flight")]

    [Tooltip("Minimum swing acceleration this bowler generates, in metres per second squared.\n" +
             "Set to 0 if the bowler always swings a little (the slider will start at 0).\n\n" +
             "Typical realistic range: 0 – 2 m/s².\n" +
             "Values above ~2 produce unrealistically large lateral drift over 20 m.")]
    public float minSwing = 0f;

    [Tooltip("Maximum swing acceleration this bowler can generate, in metres per second squared.\n\n" +
             "Typical realistic values:\n" +
             "• Legendary swing (Anderson, Wasim, Waqar)  : 12 – 15 m/s²\n" +
             "• Heavy swing (Steyn, Rabada, Zaheer)       : 10 – 12 m/s²\n" +
             "• Moderate swing (Bumrah, McGrath, Broad)   :  8 – 10 m/s²\n" +
             "• Pace dominant, little swing (Lee, Malinga):  6 –  8 m/s²\n" +
             "• Spin bowler drift (Warne, Murali)         :  2 –  3 m/s²\n\n" +
             "The HUD swing slider's outer extent equals maxSwingDirection × maxSwing.")]
    public float maxSwing = 1.5f;

    // ── Swing Direction ───────────────────────────────────────────────────────

    [Header("Swing Direction Range  [−1 = world −X  ·  0 = straight  ·  +1 = world +X]")]

    [Tooltip("Lower bound of the HUD swing direction slider (signed).\n\n" +
             "−1 = maximum out-swing  (ball curves toward world −X)\n" +
             " 0 = no swing in this direction\n\n" +
             "Examples:\n" +
             "• Pure out-swinger only      → set to −1\n" +
             "• Both ways (dual swinger)   → set to −1\n" +
             "• Pure in-swinger only       → set to  0\n\n" +
             "Must be ≤ maxSwingDirection.")]
    [Range(-1f, 1f)]
    public float minSwingDirection = 0f;

    [Tooltip("Upper bound of the HUD swing direction slider (signed).\n\n" +
             "+1 = maximum in-swing  (ball curves toward world +X)\n" +
             " 0 = no swing in this direction\n\n" +
             "Examples:\n" +
             "• Pure in-swinger only       → set to +1\n" +
             "• Both ways (dual swinger)   → set to +1\n" +
             "• Pure out-swinger only      → set to  0\n\n" +
             "Must be ≥ minSwingDirection.")]
    [Range(-1f, 1f)]
    public float maxSwingDirection = 1f;

    // ── Spin ──────────────────────────────────────────────────────────────────

    [Header("Spin Magnitude (m/s)  —  lateral velocity added to the ball after its first bounce")]

    [Tooltip("Minimum spin deviation this bowler applies after the ball bounces, in metres per second.\n" +
             "Set to 0 if the bowler might bowl a straight ball (arm ball, quicker one).\n\n" +
             "Fast seamers rarely spin intentionally — keep at 0.\n" +
             "Spinners with a very consistent action can set this above 0.")]
    public float minSpin = 0f;

    [Tooltip("Maximum spin deviation this bowler applies after the ball bounces, in metres per second.\n\n" +
             "Typical realistic values:\n" +
             "• Elite leg/off spinner (Warne, Muralitharan) : 4.0 – 6.0 m/s\n" +
             "• Good Test spinner (Kumble, Ashwin)          : 2.5 – 4.5 m/s\n" +
             "• Medium-pace cutter                          : 0.5 – 2.0 m/s\n" +
             "• Fast seam (unintentional)                   : 0.0 – 1.5 m/s\n\n" +
             "The HUD spin slider's outer extent equals maxSpinDirection × maxSpin.")]
    public float maxSpin = 2f;

    // ── Spin Direction ────────────────────────────────────────────────────────

    [Header("Spin Direction Range  [−1 = world −X (leg-break)  ·  +1 = world +X (off-break)]")]

    [Tooltip("Lower bound of the HUD spin direction slider (signed).\n\n" +
             "−1 = ball turns hard toward world −X after bounce  (leg-break / left-arm off-break)\n" +
             " 0 = no turn in this direction\n\n" +
             "Examples:\n" +
             "• Leg-break bowler (Warne)  → set to −1\n" +
             "• Off-break bowler (Ashwin) → set to  0  (only turns the other way)\n" +
             "• Bowls both ways (googly)  → set to −1\n\n" +
             "Must be ≤ maxSpinDirection.")]
    [Range(-1f, 1f)]
    public float minSpinDirection = 0f;

    [Tooltip("Upper bound of the HUD spin direction slider (signed).\n\n" +
             "+1 = ball turns hard toward world +X after bounce  (off-break / left-arm leg-break)\n" +
             " 0 = no turn in this direction\n\n" +
             "Examples:\n" +
             "• Off-break bowler (Ashwin) → set to +1\n" +
             "• Leg-break bowler (Warne)  → set to  0  (only turns the other way)\n" +
             "• Bowls both ways (googly)  → set to +1\n\n" +
             "Must be ≥ minSpinDirection.")]
    [Range(-1f, 1f)]
    public float maxSpinDirection = 1f;
}
