using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallController : MonoBehaviour
{
    public Vector3 bounceTargetPosition; // The position on the pitch where the ball should bounce
    public float bounceFactor = 0.7f; // The factor to reduce velocity after bounce
    public float gravity = 9.81f; // Gravity constant

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

        // Simple parabola generation using the current and target positions
        float totalTime = Mathf.Sqrt(2 * Mathf.Abs(startPosition.y - bounceTargetPosition.y) / gravity);
        int steps = Mathf.CeilToInt(totalTime / Time.fixedDeltaTime);

        for (int i = 0; i <= steps; i++)
        {
            float time = i * Time.fixedDeltaTime;
            float progress = time / totalTime;
            Vector3 point = Vector3.Lerp(startPosition, bounceTargetPosition, progress);
            point.y = startPosition.y + (0.5f * gravity * time * time);
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
