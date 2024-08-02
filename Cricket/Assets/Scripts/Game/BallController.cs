using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public class BallController : MonoBehaviour
{
    public Vector3 bounceTargetPosition; // The position on the pitch where the ball should bounce
    public float bounceFactor = 0.7f; // The factor to reduce velocity after bounce
    public float gravity = 9.81f; // Gravity constant

    private float speed;
    private Vector3 startPosition;
    private Vector3 pos;
    private List<Vector3> trajectoryPoints; // List to store trajectory points for gizmo

    private void OnEnable()
    {
        startPosition = transform.position;
        pos = startPosition;
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
         
        // Calculate initial velocity based on the current position and the bounce target
        Vector3 velocity = CalculateInitialVelocity(startPosition, bounceTargetPosition);

        // Calculate the trajectory with the calculated initial velocity
        CalculateTrajectory(velocity);
        StartCoroutine(MoveBallAlongTrajectory());
    }

    private Vector3 CalculateInitialVelocity(Vector3 startPosition, Vector3 targetPosition)
    {
        Vector3 directionToTarget = targetPosition - startPosition;

        // Horizontal distance
        float distanceZ = targetPosition.z - startPosition.z;
        float distanceX = targetPosition.x - startPosition.x;
        float distanceY = targetPosition.y - startPosition.y;

        // Calculate the time of flight based on the vertical motion equation
        // y = vy * t + 0.5 * g * t^2 => -5 = vy * t + 0.5 * (-9.81) * t^2
        float a = 0.5f * -gravity;
        float b = Mathf.Sqrt(2 * gravity * Mathf.Abs(distanceY));
        float c = distanceY;

        // Solve for t using the quadratic formula: t = (-b ± sqrt(b^2 - 4ac)) / 2a
        float discriminant = b * b - 4 * a * c;
        float sqrtDiscriminant = Mathf.Sqrt(discriminant);

        float t1 = (-b + sqrtDiscriminant) / (2 * a);
        float t2 = (-b - sqrtDiscriminant) / (2 * a);

        float timeToTarget = Mathf.Max(t1, t2);

        // Calculate the required velocity components
        float initialVelocityX = distanceX / timeToTarget;
        float initialVelocityZ = distanceZ / timeToTarget;
        float initialVelocityY = b / 2;  // Using vy = sqrt(2 * g * |distanceY|)

        return new Vector3(initialVelocityX, initialVelocityY, initialVelocityZ);
    }

    private void CalculateTrajectory(Vector3 initialVelocity)
    {
        trajectoryPoints = new List<Vector3>();
        Vector3 currentPosition = pos;
        Vector3 velocity = initialVelocity;

        float timeStep = Time.fixedDeltaTime;
        float time = 0;

        while (currentPosition.y > bounceTargetPosition.y || currentPosition.z < bounceTargetPosition.z)
        {
            currentPosition = pos + velocity * time + 0.5f * Physics.gravity * time * time;
            trajectoryPoints.Add(currentPosition);
            time += timeStep;

            if (currentPosition.z >= bounceTargetPosition.z && currentPosition.y <= bounceTargetPosition.y)
            {
                // Handle the bounce
                velocity = Vector3.Reflect(velocity, Vector3.up) * bounceFactor;
                currentPosition = bounceTargetPosition;
                pos = currentPosition;
                time = 0;
            }

            if (currentPosition.y < 0)
            {
                break;
            }
        }
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
        transform.position = startPosition;
        pos = startPosition;
    }
}
