using UnityEngine;

public class BallController : MonoBehaviour
{
    public BallCollisionDetection ballCollisionDetection; // Reference to the BallCollisionDetection script
    public Vector3 bounceTargetPosition; // The position on the pitch where the ball should bounce

    private Vector3 initPosition;
    private bool throwBall = false;
    private Vector3 currentVelocity;

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
            ballCollisionDetection.MoveBallAndCheckForCollision(currentVelocity, bounceTargetPosition, CricketGameModel.Instance.GetBounceFactor());
        }
    }

    private void OnThrowBallEventHandler(Vector3 bounceTargetPosition, Vector3 initialVelocity)
    {
        ResetBall();

        this.bounceTargetPosition = bounceTargetPosition;
        this.currentVelocity = initialVelocity;

        throwBall = true;
    }

    private void ResetBall()
    {
        transform.position = initPosition;
        this.currentVelocity = Vector3.zero;

        throwBall = false;
    }
}