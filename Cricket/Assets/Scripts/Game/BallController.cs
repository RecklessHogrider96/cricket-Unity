using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallController : MonoBehaviour
{
    public Vector3 bounceTargetPosition; // The position on the pitch where the ball should bounce
    public float bounceFactor = 0.7f; // The factor to reduce velocity after bounce

    private Vector3 initPosition;
    private Vector3 currentVelocity;
    private List<Vector3> trajectoryPoints; // List to store trajectory points for gizmo
    private bool hasBounced = false;

    private void OnEnable()
    {
        initPosition = transform.position;
        ThrowBallEvent.Instance.AddListener(OnThrowBallEventHandler);
    }

    private void OnDisable()
    {
        ThrowBallEvent.Instance.RemoveListener(OnThrowBallEventHandler);
    }

    private void OnThrowBallEventHandler(Vector3 initialVelocity, Vector3 bounceTargetPosition)
    {
        ResetBall();

        this.bounceTargetPosition = bounceTargetPosition;
        this.currentVelocity = initialVelocity;
        this.hasBounced = false;

        CalculateTrajectory();
        StartCoroutine(MoveBallAlongTrajectory());
    }

    private void CalculateTrajectory()
    {
        trajectoryPoints = new List<Vector3>();
        Vector3 currentPosition = initPosition;
        Vector3 velocity = currentVelocity;

        while (currentPosition.y > 0)
        {
            trajectoryPoints.Add(currentPosition);

            float timeStep = Time.fixedDeltaTime;
            currentPosition += velocity * timeStep;
            velocity += Physics.gravity * timeStep;

            // Check for bounce
            if (!hasBounced && currentPosition.z >= bounceTargetPosition.z && currentPosition.y <= bounceTargetPosition.y)
            {
                // Snap to the exact bounce target position
                currentPosition = bounceTargetPosition;
                trajectoryPoints.Add(currentPosition);
                hasBounced = true;

                // Reflect the velocity to simulate the bounce
                velocity = Vector3.Reflect(velocity, Vector3.up) * bounceFactor;
            }

            // Break the loop if the ball has bounced and is below the target position
            if (hasBounced && currentPosition.y < bounceTargetPosition.y && currentPosition.z > bounceTargetPosition.z)
            {
                break;
            }
        }

        // Add the final position to ensure the loop ends correctly
        trajectoryPoints.Add(currentPosition);
    }

    private IEnumerator MoveBallAlongTrajectory()
    {
        foreach (var point in trajectoryPoints)
        {
            transform.position = point;
            yield return new WaitForFixedUpdate();
        }
    }

    private void OnDrawGizmos()
    {
        if (trajectoryPoints != null && trajectoryPoints.Count > 0)
        {
            Gizmos.color = Color.red;
            for (int i = 0; i < trajectoryPoints.Count - 1; i++)
            {
                Gizmos.DrawLine(trajectoryPoints[i], trajectoryPoints[i + 1]);
            }
        }
    }

    private void ResetBall()
    {
        transform.position = initPosition;
        hasBounced = false;
    }
}
