using UnityEngine;

public class CricketGameController : MonoBehaviour
{
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // Throw the ball
            ThrowBallEvent.Instance.Invoke();
        }
    }
}
