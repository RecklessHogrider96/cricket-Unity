using UnityEngine;

/// <summary>
/// Translates player input into game events.
/// Space bar queries the pitch marker position then fires a delivery.
/// </summary>
public class CricketGameController : MonoBehaviour
{
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            GetPitchMarkerPositionEvent.Instance.Invoke(OnMarkerPositionReceived);
        }
    }

    private void OnMarkerPositionReceived(Vector3 markerPosition)
    {
        BallThrowData throwData = CricketGameModel.Instance.GetThrowParameters(markerPosition);
        ThrowBallEvent.Instance.Invoke(throwData);
    }
}
