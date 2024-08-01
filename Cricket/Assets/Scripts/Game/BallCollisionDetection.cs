using UnityEngine;

public class BallCollisionDetection : MonoBehaviour
{
    public Vector3 velocity; // Current velocity of the ball
    public float gravity = -9.81f; // Gravity constant
    public float bounceFactor = 0.7f; // Control the bounce intensity
    private Vector3 previousPosition;

    void Start()
    {
        previousPosition = transform.position;
    }

    void Update()
    {
        // Apply gravity to the velocity
        velocity += new Vector3(0, gravity * Time.deltaTime, 0);

        // Move the ball manually
        transform.position += velocity * Time.deltaTime;

        // Ray from previous position to current position
        Ray ray = new Ray(previousPosition, transform.position - previousPosition);
        RaycastHit hit;

        // Check if the ball has collided with the pitch
        if (Physics.Raycast(ray, out hit, (transform.position - previousPosition).magnitude))
        {
            if (hit.collider.tag == "Pitch")
            {
                HandleBallCollision(hit.point, hit.normal);
            }
        }

        // Update the previous position
        previousPosition = transform.position;
    }

    void HandleBallCollision(Vector3 collisionPoint, Vector3 collisionNormal)
    {
        // Reflect the velocity based on the collision normal
        Vector3 reflectedVelocity = Vector3.Reflect(velocity, collisionNormal);
        velocity = reflectedVelocity * bounceFactor;

        // Adjust the ball's position slightly above the collision point to prevent it from getting stuck
        transform.position = collisionPoint + collisionNormal * 0.05f;
    }
}
