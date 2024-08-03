using TMPro;
using UnityEngine;

public class CricketInGameHUD : MonoBehaviour
{
    [SerializeField] private Canvas canvasHUD;
    [SerializeField] private TMP_Dropdown bowlerSelector;

    private void OnEnable()
    {
        bowlerSelector.value = 0;
        bowlerSelector.onValueChanged.AddListener(OnBowlerSelected);
    }

    private void OnBowlerSelected(int arg0)
    {
        CricketGameModel.Instance.SetBowlerType((BowlerType)arg0);
    }
}
