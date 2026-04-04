using TMPro;
using UnityEngine;

/// <summary>
/// In-game HUD. Drives the bowler type and bowling arm selectors.
///
/// Listener hygiene: RemoveListener is always called before AddListener so that
/// toggling the GameObject never stacks duplicate callbacks.
/// </summary>
public class CricketInGameHUD : MonoBehaviour
{
    [SerializeField] private Canvas canvasHUD;
    [SerializeField] private TMP_Dropdown bowlerTypeSelector;
    [Tooltip("Dropdown options must be ordered to match BowlerBowlingArm enum: 0=Left, 1=Right.")]
    [SerializeField] private TMP_Dropdown bowlingArmSelector;

    private void OnEnable()
    {
        bowlerTypeSelector.onValueChanged.RemoveListener(OnBowlerTypeSelected);
        bowlerTypeSelector.onValueChanged.AddListener(OnBowlerTypeSelected);
        bowlerTypeSelector.value = (int)CricketGameModel.Instance.GetBowlerType();

        bowlingArmSelector.onValueChanged.RemoveListener(OnBowlingArmSelected);
        bowlingArmSelector.onValueChanged.AddListener(OnBowlingArmSelected);
        bowlingArmSelector.value = (int)CricketGameModel.Instance.GetBowlingArm();
    }

    private void OnDisable()
    {
        bowlerTypeSelector.onValueChanged.RemoveListener(OnBowlerTypeSelected);
        bowlingArmSelector.onValueChanged.RemoveListener(OnBowlingArmSelected);
    }

    private void OnBowlerTypeSelected(int index)
    {
        CricketGameModel.Instance.SetBowlerType((BowlerType)index);
    }

    private void OnBowlingArmSelected(int index)
    {
        CricketGameModel.Instance.SetBowlingArm((BowlerBowlingArm)index);
    }
}
