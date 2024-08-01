using UnityEngine;

public class CricketGameController : MonoBehaviour
{
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            GetPitchMarkerPositionEvent.Instance.Invoke(markerPosition => 
            {
                // Current velocity of the ball
                Vector3 velocity = new Vector3(0f, 0f, CricketGameModel.Instance.GetBowlerMaxSpeed());
                // Throw the ball
                ThrowBallEvent.Instance.Invoke(markerPosition, velocity);
            });
        }
    }
}
