using UnityEngine;

public class BallCollisionDetection : MonoBehaviour
{
    private Vector3 previousPosition;
    private Vector3 currentVelocity;

    private void Start()
    {
        previousPosition = transform.position;
    }

    public void MoveBallAndCheckForCollision(Vector3 initialVelocity, Vector3 bounceTargetPosition, float bounceFactor)
    {
        currentVelocity = initialVelocity;

        // Apply gravity to the velocity
        currentVelocity += new Vector3(0, CricketGameModel.Instance.GetGravity() * Time.deltaTime, 0);

        // Calculate the direction towards the bounce target
        Vector3 directionToTarget = (bounceTargetPosition - transform.position).normalized;
        currentVelocity = Vector3.Lerp(currentVelocity, directionToTarget * currentVelocity.magnitude, Time.deltaTime * 0.5f);

        // Move the ball manually
        transform.position += currentVelocity * Time.deltaTime;

        // Ray from previous position to current position
        Ray ray = new Ray(previousPosition, transform.position - previousPosition);
        RaycastHit hit;

        // Check if the ball has collided with the pitch
        if (Physics.Raycast(ray, out hit, (transform.position - previousPosition).magnitude))
        {
            if (hit.collider.tag == "Pitch")
            {
                HandleBallCollision(currentVelocity, bounceFactor, hit.point, hit.normal);
            }
        }

        // Update the previous position
        previousPosition = transform.position;
    }

    private void HandleBallCollision(Vector3 velocity, float bounceFactor, Vector3 collisionPoint, Vector3 collisionNormal)
    {
        // Reflect the velocity based on the collision normal
        Vector3 reflectedVelocity = Vector3.Reflect(velocity, collisionNormal);
        currentVelocity = reflectedVelocity * bounceFactor;

        // Adjust the ball's position slightly above the target point to simulate a bounce
        transform.position = collisionPoint + collisionNormal * 0.05f;
    }
}