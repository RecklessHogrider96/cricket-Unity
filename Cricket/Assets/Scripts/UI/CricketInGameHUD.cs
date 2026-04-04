using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// In-game HUD for configuring and triggering deliveries.
///
/// ── Inspector wiring guide ───────────────────────────────────────────────────
///
///  [Bowler Selection]
///    bowlerDropdown    — TMP_Dropdown — auto-populated from BowlerRosterSO
///
///  [Arm Selector — shown only when bowler.bowlerArm == Both]
///    armSelectorGroup  — GameObject  — parent wrapping the dropdown + its label;
///                                      shown/hidden automatically
///    bowlingArmDropdown — TMP_Dropdown — options: Left (0), Right (1)
///
///  [Speed — always visible]
///    speedSlider       — Slider      — range set automatically from bowler SO
///    speedLabel        — TMP_Text    — e.g. "Speed: 38.0 m/s  (137 km/h)"
///
///  [Swing Group — shown for Fast and Medium bowlers]
///    swingGroup        — GameObject  — parent of swing slider + label;
///                                      shown/hidden by bowler type
///    swingSlider       — Slider      — signed: negative = out-swing, positive = in-swing
///    swingLabel        — TMP_Text    — e.g. "Swing: 1.20 m/s²  (In)"
///
///  [Spin Group — shown for Spin and Medium bowlers]
///    spinGroup         — GameObject  — parent of spin slider + label;
///                                      shown/hidden by bowler type
///    spinSlider        — Slider      — signed: negative = leg-break, positive = off-break
///    spinLabel         — TMP_Text    — e.g. "Spin: 3.50 m/s  (Leg)"
///
/// ── Flow ─────────────────────────────────────────────────────────────────────
///   OnEnable  → populate bowler dropdown → select index 0
///   Bowler change → model.SetSelectedBowler (auto-sets arm if fixed)
///               → RefreshArmSelector  (show/hide arm group, sync dropdown)
///               → RefreshPanelVisibility (swing/spin groups)
///               → RefreshSliderRanges
///   Arm dropdown (only when Both) → model.SetBowlingArm
///   Sliders → push values to CricketGameModel
///   Space (CricketGameController) → reads model → fires BallThrowData
/// </summary>
public class CricketInGameHUD : MonoBehaviour
{
    [Header("Bowler Selection")]
    [SerializeField] private TMP_Dropdown bowlerDropdown;

    [Header("Arm Selector — visible only when bowler.bowlerArm == Both")]
    [Tooltip("Parent GameObject containing the arm dropdown and its label. " +
             "Set active/inactive automatically based on the selected bowler's arm preference.")]
    [SerializeField] private GameObject   armSelectorGroup;
    [Tooltip("Options must match BowlerBowlingArm enum: index 0 = Left, index 1 = Right.")]
    [SerializeField] private TMP_Dropdown bowlingArmDropdown;

    [Header("Speed — all bowlers")]
    [SerializeField] private Slider   speedSlider;
    [SerializeField] private TMP_Text speedLabel;

    [Header("Swing — Fast and Medium bowlers")]
    [Tooltip("Parent GameObject containing the swing slider and label.")]
    [SerializeField] private GameObject swingGroup;
    [SerializeField] private Slider     swingSlider;
    [SerializeField] private TMP_Text   swingLabel;

    [Header("Spin — Spin and Medium bowlers")]
    [Tooltip("Parent GameObject containing the spin slider and label.")]
    [SerializeField] private GameObject spinGroup;
    [SerializeField] private Slider     spinSlider;
    [SerializeField] private TMP_Text   spinLabel;

    private BowlerRosterSO roster;
    private BowlerConfigSO currentBowler;

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    private void OnEnable()
    {
        roster = CricketGameModel.Instance.GetDataController().GetBowlerRoster();

        PopulateBowlerDropdown();
        SubscribeListeners();

        // Initialise with the first bowler in the roster (fires all Refresh* methods)
        if (roster != null && roster.bowlers.Count > 0)
            OnBowlerSelected(0);
    }

    private void OnDisable()
    {
        UnsubscribeListeners();
    }

    // ── Dropdown population ───────────────────────────────────────────────────

    private void PopulateBowlerDropdown()
    {
        if (bowlerDropdown == null) return;
        if (roster == null)
        {
            Debug.LogWarning("[CricketInGameHUD] BowlerRosterSO is null — " +
                             "assign it to CricketDataController in the Inspector.");
            return;
        }

        bowlerDropdown.ClearOptions();
        var options = new List<TMP_Dropdown.OptionData>();
        foreach (BowlerConfigSO bowler in roster.bowlers)
            options.Add(new TMP_Dropdown.OptionData(bowler.bowlerName));
        bowlerDropdown.AddOptions(options);
    }

    // ── Listener management ───────────────────────────────────────────────────

    private void SubscribeListeners()
    {
        if (bowlerDropdown != null)
        {
            bowlerDropdown.onValueChanged.RemoveListener(OnBowlerSelected);
            bowlerDropdown.onValueChanged.AddListener(OnBowlerSelected);
        }
        if (bowlingArmDropdown != null)
        {
            bowlingArmDropdown.onValueChanged.RemoveListener(OnArmSelected);
            bowlingArmDropdown.onValueChanged.AddListener(OnArmSelected);
        }
        if (speedSlider != null)
        {
            speedSlider.onValueChanged.RemoveListener(OnSpeedChanged);
            speedSlider.onValueChanged.AddListener(OnSpeedChanged);
        }
        if (swingSlider != null)
        {
            swingSlider.onValueChanged.RemoveListener(OnSwingChanged);
            swingSlider.onValueChanged.AddListener(OnSwingChanged);
        }
        if (spinSlider != null)
        {
            spinSlider.onValueChanged.RemoveListener(OnSpinChanged);
            spinSlider.onValueChanged.AddListener(OnSpinChanged);
        }
    }

    private void UnsubscribeListeners()
    {
        bowlerDropdown?.onValueChanged.RemoveListener(OnBowlerSelected);
        bowlingArmDropdown?.onValueChanged.RemoveListener(OnArmSelected);
        speedSlider?.onValueChanged.RemoveListener(OnSpeedChanged);
        swingSlider?.onValueChanged.RemoveListener(OnSwingChanged);
        spinSlider?.onValueChanged.RemoveListener(OnSpinChanged);
    }

    // ── Callbacks ─────────────────────────────────────────────────────────────

    private void OnBowlerSelected(int index)
    {
        if (roster == null || index < 0 || index >= roster.bowlers.Count) return;

        currentBowler = roster.bowlers[index];

        // SetSelectedBowler auto-sets bowlingArm for Left/Right bowlers
        CricketGameModel.Instance.SetSelectedBowler(currentBowler);

        RefreshArmSelector();
        RefreshPanelVisibility();
        RefreshSliderRanges();
    }

    private void OnArmSelected(int index)
    {
        // Only reachable when armSelectorGroup is visible (bowlerArm == Both)
        CricketGameModel.Instance.SetBowlingArm((BowlerBowlingArm)index);
    }

    private void OnSpeedChanged(float value)
    {
        CricketGameModel.Instance.SetDeliverySpeed(value);
        if (speedLabel != null)
            speedLabel.text = $"Speed: {value:F1} m/s  ({value * 3.6f:F0} km/h)";
    }

    private void OnSwingChanged(float value)
    {
        CricketGameModel.Instance.SetDeliverySwing(value);
        if (swingLabel != null)
        {
            string dir = value >  0.01f ? "In"
                       : value < -0.01f ? "Out"
                       : "Straight";
            swingLabel.text = $"Swing: {Mathf.Abs(value):F2} m/s\u00B2  ({dir})";
        }
    }

    private void OnSpinChanged(float value)
    {
        CricketGameModel.Instance.SetDeliverySpin(value);
        if (spinLabel != null)
        {
            string dir = value >  0.01f ? "Off"
                       : value < -0.01f ? "Leg"
                       : "Straight";
            spinLabel.text = $"Spin: {Mathf.Abs(value):F2} m/s  ({dir})";
        }
    }

    // ── Panel & arm refresh ───────────────────────────────────────────────────

    /// <summary>
    /// Shows the arm selector only when the bowler's arm preference is Both.
    /// For Left or Right bowlers the selector is hidden — the model already
    /// has the correct arm set by SetSelectedBowler.
    /// Always syncs the dropdown value to the model's current arm so it
    /// reflects the auto-set value when it becomes visible again.
    /// </summary>
    private void RefreshArmSelector()
    {
        if (currentBowler == null) return;

        bool showArm = currentBowler.bowlerArm == BowlerArmPreference.Both;
        if (armSelectorGroup != null) armSelectorGroup.SetActive(showArm);

        // Sync dropdown to model (reflects auto-set or last manual selection)
        if (bowlingArmDropdown != null)
            bowlingArmDropdown.value = (int)CricketGameModel.Instance.GetBowlingArm();
    }

    /// <summary>
    /// Shows swing controls for Fast and Medium; spin controls for Spin and Medium.
    /// </summary>
    private void RefreshPanelVisibility()
    {
        if (currentBowler == null) return;

        bool showSwing = currentBowler.bowlerType == BowlerType.Fast   ||
                         currentBowler.bowlerType == BowlerType.Medium;
        bool showSpin  = currentBowler.bowlerType == BowlerType.Spin   ||
                         currentBowler.bowlerType == BowlerType.Medium;

        if (swingGroup != null) swingGroup.SetActive(showSwing);
        if (spinGroup  != null) spinGroup.SetActive(showSpin);
    }

    /// <summary>
    /// Reconfigures slider min/max from the selected bowler's SO ranges.
    /// Setting slider.value fires the OnValueChanged callback which pushes
    /// the new value into CricketGameModel automatically.
    /// </summary>
    private void RefreshSliderRanges()
    {
        if (currentBowler == null) return;

        if (speedSlider != null)
        {
            speedSlider.minValue = currentBowler.minSpeed;
            speedSlider.maxValue = currentBowler.maxSpeed;
            speedSlider.value    = (currentBowler.minSpeed + currentBowler.maxSpeed) * 0.5f;
        }

        if (swingSlider != null)
        {
            // Signed range: negative = out-swing, positive = in-swing
            swingSlider.minValue = currentBowler.minSwingDirection * currentBowler.maxSwing;
            swingSlider.maxValue = currentBowler.maxSwingDirection * currentBowler.maxSwing;
            swingSlider.value    = 0f;
        }

        if (spinSlider != null)
        {
            // Signed range: negative = leg-break, positive = off-break
            spinSlider.minValue = currentBowler.minSpinDirection * currentBowler.maxSpin;
            spinSlider.maxValue = currentBowler.maxSpinDirection * currentBowler.maxSpin;
            spinSlider.value    = (spinSlider.minValue + spinSlider.maxValue) * 0.5f;
        }
    }
}
