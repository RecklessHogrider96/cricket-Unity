using UnityEngine;

public class CricketGameController : MonoBehaviour
{
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            GetPitchMarkerPositionEvent.Instance.Invoke(markerPosition => 
            {
                // Throw the ball
                ThrowBallEvent.Instance.Invoke(markerPosition, CricketGameModel.Instance.GetBowlerMaxSpeed());
            });
        }
    }
}
