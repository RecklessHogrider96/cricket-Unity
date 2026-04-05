using UnityEngine;

/// <summary>
/// Central runtime state for the cricket game.
///
/// Bowler selection flow:
///   HUD bowler dropdown  → SetSelectedBowler(BowlerConfigSO)
///   HUD weather dropdown → SetSelectedWeather(WeatherConfigSO)
///   HUD sliders          → SetDeliverySpeed / SetDeliverySpin / SetDeliverySwing
///   CricketGameController (Space) → GetThrowParameters(markerPos) → BallThrowData
///
/// Delivery values are driven directly by the HUD sliders — no random sampling
/// during gameplay. The HUD initialises the sliders to the bowler's midpoint
/// ranges when a new bowler is selected.
/// </summary>
public class CricketGameModel : Singleton<CricketGameModel>
{
    [SerializeField] private CricketDataController cricketDataController;

    [Header("Runtime State (Inspector shows current values — read-only in play mode)")]
    [SerializeField] private BowlerConfigSO  selectedBowler;
    [SerializeField] private WeatherConfigSO selectedWeather;
    [SerializeField] private BowlerBowlingArm bowlingArm;
    [SerializeField] private WicketApproach  currentApproach = WicketApproach.Over;

    // Current per-delivery values — set by HUD sliders
    private float currentSpeed;
    private float currentSpin;
    private float currentSwing;

    // ── Bowler selection ─────────────────────────────────────────────────────

    /// <summary>
    /// Called by the HUD when the bowler dropdown changes.
    /// Auto-sets bowlingArm when the bowler has a fixed arm preference (Left or Right).
    /// When preference is Both, bowlingArm is left at whatever the HUD dropdown last set.
    /// Resets delivery params to sensible defaults; HUD sliders overwrite them immediately.
    /// </summary>
    public void SetSelectedBowler(BowlerConfigSO bowler)
    {
        selectedBowler = bowler;

        if (bowler == null) return;

        // Auto-set the active arm for fixed-arm bowlers.
        // Both → leave bowlingArm unchanged so the HUD arm dropdown keeps control.
        switch (bowler.bowlerArm)
        {
            case BowlerArmPreference.Left:  bowlingArm = BowlerBowlingArm.Left;  break;
            case BowlerArmPreference.Right: bowlingArm = BowlerBowlingArm.Right; break;
        }

        // Default delivery values to midpoints; HUD overwrites via slider callbacks.
        currentSpeed = (bowler.minSpeed + bowler.maxSpeed) * 0.5f;
        currentSpin  = 0f;
        currentSwing = 0f;
    }

    public BowlerConfigSO GetSelectedBowler() => selectedBowler;

    // ── Weather selection ────────────────────────────────────────────────────

    /// <summary>
    /// Called by the HUD when the weather dropdown changes.
    /// The selected WeatherConfigSO is read by BallController at throw time.
    /// Passing null clears any active weather (all offsets become zero / multipliers become 1).
    /// </summary>
    public void SetSelectedWeather(WeatherConfigSO weather) => selectedWeather = weather;
    public WeatherConfigSO GetSelectedWeather() => selectedWeather;

    // ── Arm ──────────────────────────────────────────────────────────────────

    public void SetBowlingArm(BowlerBowlingArm arm) => bowlingArm = arm;
    public BowlerBowlingArm GetBowlingArm() => bowlingArm;

    // ── Wicket approach ──────────────────────────────────────────────────────

    /// <summary>
    /// Called by the HUD Over/Around dropdown.
    /// Determines which release point is read from the selected BowlerConfigSO.
    /// </summary>
    public void SetWicketApproach(WicketApproach approach) => currentApproach = approach;
    public WicketApproach GetWicketApproach() => currentApproach;

    // ── Per-delivery parameter setters (driven by HUD sliders) ───────────────

    public void SetDeliverySpeed(float speed) => currentSpeed = speed;
    public void SetDeliverySpin(float spin)   => currentSpin  = spin;
    public void SetDeliverySwing(float swing) => currentSwing = swing;

    public float GetDeliverySpeed() => currentSpeed;
    public float GetDeliverySpin()  => currentSpin;
    public float GetDeliverySwing() => currentSwing;

    // ── Data access ──────────────────────────────────────────────────────────

    public CricketDataController GetDataController() => cricketDataController;

    // ── Throw parameter assembly ─────────────────────────────────────────────

    /// <summary>
    /// Builds the BallThrowData from the currently selected bowler and
    /// the delivery values last set by the HUD sliders.
    /// Returns a safe fallback and logs an error if no bowler is selected.
    /// </summary>
    public BallThrowData GetThrowParameters(Vector3 bounceTarget)
    {
        if (selectedBowler == null)
        {
            Debug.LogError("[CricketGameModel] GetThrowParameters called but no bowler is selected. " +
                           "Select a bowler in the HUD first.");
            return new BallThrowData
            {
                releasePoint = Vector3.zero,
                bounceTarget = bounceTarget,
                speed        = 30f,
                spin         = 0f,
                swingAmount  = 0f,
                bowlingArm   = bowlingArm
            };
        }

        // Pick the release point that matches the currently selected wicket approach.
        Vector3 releasePoint = currentApproach == WicketApproach.Over
            ? selectedBowler.overTheWicketReleasePoint
            : selectedBowler.aroundTheWicketReleasePoint;

        return new BallThrowData
        {
            releasePoint = releasePoint,
            bounceTarget = bounceTarget,
            speed        = currentSpeed,
            spin         = currentSpin,
            swingAmount  = currentSwing,   // already signed from HUD swing slider
            bowlingArm   = bowlingArm
        };
    }
}
