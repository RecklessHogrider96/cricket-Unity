using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallController : MonoBehaviour
{
    private float bounceFactor; // The factor to reduce velocity after bounce
    private float gravity; // Gravity constant
    private float speed;
    private Vector3 startPosition;
    [SerializeField] private List<Vector3> trajectoryPoints; // List to store trajectory points for gizmo
    private int maxBounces = 100; // Maximum number of bounces
    private Coroutine moveCorountine;

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

        this.gravity = CricketGameModel.Instance.GetGravity();
        this.bounceFactor = CricketGameModel.Instance.GetBounceFactor();
        this.speed = speed;

        // Clear previous trajectory points
        trajectoryPoints = new List<Vector3>();

        // Calculate the trajectory to the pitch marker
        CalculateInitialTrajectory(bounceTargetPosition);

        // Start moving the ball along the calculated trajectory
        moveCorountine = StartCoroutine(MoveBallAlongTrajectory());
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

        // Calculate the trajectory after the bounce
        CalculateBounceTrajectory(trajectoryPoints[trajectoryPoints.Count - 1], new Vector3(velocityX, initialVelocityY, velocityZ));
    }

    private void CalculateBounceTrajectory(Vector3 bouncePosition, Vector3 currentVelocity)
    {
        Vector3 currentPosition = bouncePosition; // Start from the bounce point
        //Vector3 currentVelocity = new Vector3(0, 0, speed); // Assuming initial horizontal velocity only

        float timeStep = Time.fixedDeltaTime;
        int bounceCount = 0;

        while (bounceCount < maxBounces)
        {
            currentVelocity += Physics.gravity * timeStep; // Apply gravity to the velocity
            currentPosition += currentVelocity * timeStep;

            trajectoryPoints.Add(currentPosition);

            // Check for ground collision
            if (currentPosition.y <= 1f)
            {
                currentPosition.y = 1f; // Snap to ground level
                currentVelocity = Vector3.Reflect(currentVelocity, Vector3.up) * bounceFactor; // Reflect and reduce velocity
                bounceCount++;
            }

            // Stop if velocity is too low to continue bouncing
            if (Mathf.Abs(currentVelocity.y) < 0.01f && currentPosition.y <= 1f)
            {
                break;
            }
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
        if (moveCorountine != null)
        {
            StopCoroutine(moveCorountine);
        }

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
