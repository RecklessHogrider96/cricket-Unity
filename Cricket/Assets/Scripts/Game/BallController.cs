using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallController : MonoBehaviour
{
    private Vector3 bounceTargetPosition; // The position on the pitch where the ball should bounce
    private float bounceFactor; // The factor to reduce velocity after bounce
    private float gravity; // Gravity constant
    private float speed;
    private Vector3 startPosition;
    private List<Vector3> trajectoryPoints; // List to store trajectory points for gizmo

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
        this.bounceTargetPosition = bounceTargetPosition;

        // Calculate the initial trajectory points from start to bounce target
        CalculateTrajectory();

        // Start moving the ball along the calculated trajectory at the specified speed
        StartCoroutine(MoveBallAlongTrajectory());
    }

    private void CalculateTrajectory()
    {
        trajectoryPoints = new List<Vector3>();
        Vector3 directionToTarget = bounceTargetPosition - startPosition;

        // Horizontal distances
        float distanceX = directionToTarget.x;
        float distanceZ = directionToTarget.z;

        // Vertical distance
        float distanceY = directionToTarget.y;

        // Total horizontal distance in XZ plane
        float horizontalDistance = Mathf.Sqrt(distanceX * distanceX + distanceZ * distanceZ);

        // Time to reach the target, considering gravity
        float timeToTarget = Mathf.Sqrt(2 * Mathf.Abs(distanceY) / gravity);

        // Initial horizontal velocities
        float velocityX = distanceX / timeToTarget;
        float velocityZ = distanceZ / timeToTarget;

        // Calculate vertical velocity needed to reach the target considering gravity
        float initialVelocityY = (distanceY + 0.5f * gravity * timeToTarget * timeToTarget) / timeToTarget;

        // Number of steps for the trajectory
        int steps = Mathf.CeilToInt(timeToTarget / Time.fixedDeltaTime);

        for (int i = 0; i <= steps; i++)
        {
            float time = i * Time.fixedDeltaTime;
            Vector3 point = new Vector3();

            // Calculate X and Z positions linearly
            point.x = startPosition.x + velocityX * time;
            point.z = startPosition.z + velocityZ * time;

            // Calculate Y position using the parabolic equation
            point.y = startPosition.y + initialVelocityY * time - 0.5f * gravity * time * time;

            trajectoryPoints.Add(point);
        }
    }

    private IEnumerator MoveBallAlongTrajectory()
    {
        float journeyLength = Vector3.Distance(startPosition, bounceTargetPosition);
        float startTime = Time.time;

        for (int i = 0; i < trajectoryPoints.Count - 1; i++)
        {
            float journeyTime = Vector3.Distance(trajectoryPoints[i], trajectoryPoints[i + 1]) / speed;

            while (Time.time - startTime < journeyTime)
            {
                transform.position = Vector3.Lerp(trajectoryPoints[i], trajectoryPoints[i + 1], (Time.time - startTime) / journeyTime);
                yield return null;
            }

            startTime = Time.time;
        }

        transform.position = bounceTargetPosition;
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
