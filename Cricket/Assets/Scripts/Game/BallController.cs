using UnityEngine;

public class BallController : MonoBehaviour
{
    public BallCollisionDetection ballCollisionDetection; // Reference to the BallCollisionDetection script

    private Vector3 initPosition;
    private bool throwBall = false;

    private void OnEnable()
    {
        initPosition = transform.position;

        ResetBall();

        ThrowBallEvent.Instance.AddListener(OnThrowBallEventHandler);
    }

    private void OnDisable()
    {
        ThrowBallEvent.Instance.RemoveListener(OnThrowBallEventHandler);
    }

    private void FixedUpdate()
    {
        if (throwBall)
        {
            // Current velocity of the ball
            Vector3 velocity = new(0f, 0f, CricketGameModel.Instance.GetBowlerMaxSpeed());
            ballCollisionDetection.MoveBallAndCheckForCollision(velocity, CricketGameModel.Instance.GetBounceFactor());
        }
    }

    private void OnThrowBallEventHandler()
    {
        ResetBall();

        throwBall = true;
    }

    private void ResetBall()
    {
        transform.position = initPosition;
        throwBall = false;
    }
}
