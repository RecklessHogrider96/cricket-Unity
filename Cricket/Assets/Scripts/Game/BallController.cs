using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallController : MonoBehaviour
{
    private float bounceFactor; // The factor to reduce velocity after bounce
    private float gravity; // Gravity constant
    private float speed;
    private Vector3 startPosition;
    private List<Vector3> trajectoryPoints; // List to store trajectory points for gizmo
    private int maxBounces = 100; // Maximum number of bounces

    private void OnEnable()
    {
        startPosition = transform.position;
        ThrowBallEvent.Instance.AddListener(OnThrowBallEventHandler);
    }

    private void OnDisable()
    {
        ThrowBallEvent.Instance.RemoveListener(OnThrowBallEventHandler);
    }

    private void OnThrowBallEventHandler(Vector3 bounceTargetPosition, float speed)
    {
        ResetBall();

        gravity = CricketGameModel.Instance.GetGravity();
        bounceFactor = CricketGameModel.Instance.GetBounceFactor();
        this.speed = speed;

        // Clear previous trajectory points
        trajectoryPoints = new List<Vector3>();

        // Calculate the trajectory to the pitch marker
        CalculateInitialTrajectory(bounceTargetPosition);

        // Calculate the trajectory after the bounce
        CalculateBounceTrajectory(bounceTargetPosition);

        // Start moving the ball along the calculated trajectory
        StartCoroutine(MoveBallAlongTrajectory());
    }

    private void CalculateInitialTrajectory(Vector3 targetPosition)
    {
        Vector3 currentPosition = startPosition;

        // Initial horizontal and vertical distances
        float distanceX = targetPosition.x - startPosition.x;
        float distanceZ = targetPosition.z - startPosition.z;
        float distanceY = targetPosition.y - startPosition.y;

        // Calculate time to reach the target based on the desired speed
        float horizontalDistance = Mathf.Sqrt(distanceX * distanceX + distanceZ * distanceZ);
        float timeToTarget = horizontalDistance / speed;

        // Calculate the initial velocities in X, Y, and Z directions
        float velocityX = distanceX / timeToTarget;
        float velocityZ = distanceZ / timeToTarget;
        float initialVelocityY = (distanceY + 0.5f * gravity * timeToTarget * timeToTarget) / timeToTarget;

        float timeStep = Time.fixedDeltaTime;
        float time = 0f;

        // Loop to calculate trajectory to the target
        while (currentPosition.z <= targetPosition.z)
        {
            // Calculate the new position based on current velocity and gravity
            currentPosition.x = startPosition.x + velocityX * time;
            currentPosition.z = startPosition.z + velocityZ * time;
            currentPosition.y = startPosition.y + initialVelocityY * time - 0.5f * gravity * time * time;

            trajectoryPoints.Add(currentPosition);
            time += timeStep;
        }
    }

    private void CalculateBounceTrajectory(Vector3 bouncePosition)
    {
        Vector3 currentPosition = bouncePosition; // Start from the bounce point
        float initialVelocityY = Mathf.Sqrt(2 * gravity * (currentPosition.y - 1f)); // Reflect vertical velocity after bounce

        float timeStep = Time.fixedDeltaTime;
        float time = 0f;
        int bounceCount = 0;

        while (currentPosition.z <= 50 && bounceCount < maxBounces)
        {
            currentPosition.x += 0; // Assuming no X direction change after bounce
            currentPosition.z += speed * timeStep;
            currentPosition.y = currentPosition.y + initialVelocityY * time - 0.5f * gravity * time * time;

            trajectoryPoints.Add(currentPosition);

            // Stop calculating if the ball reaches the ground level again
            if (currentPosition.y <= 1f)
            {
                bounceCount++;
                if (bounceCount >= maxBounces)
                {
                    break;
                }
                currentPosition.y = 1f; // Reset Y to ground level after bounce
                initialVelocityY = -initialVelocityY * bounceFactor; // Reflect and reduce vertical velocity using bounceFactor
                time = 0; // Reset time after each bounce
            }

            time += timeStep;
        }
    }

    private IEnumerator MoveBallAlongTrajectory()
    {
        for (int i = 0; i < trajectoryPoints.Count - 1; i++)
        {
            float journeyTime = Vector3.Distance(trajectoryPoints[i], trajectoryPoints[i + 1]) / speed;

            float startTime = Time.time;

            while (Time.time - startTime < journeyTime)
            {
                transform.position = Vector3.Lerp(trajectoryPoints[i], trajectoryPoints[i + 1], (Time.time - startTime) / journeyTime);
                yield return null;
            }
        }

        // Set final position to the last point in the trajectory
        transform.position = trajectoryPoints[trajectoryPoints.Count - 1];
    }

    private void ResetBall()
    {
        transform.position = startPosition;
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
}
