using UnityEngine;

public class BallCollisionDetection : MonoBehaviour
{
    [SerializeField] private Vector3 velocity;
    [SerializeField] private float bounceFactor = 0.7f;

    private Rigidbody rb;
    private Vector3 previousPosition;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        previousPosition = transform.position;
    }

    private void FixedUpdate()
    {
        // Move the ball manually if Rigidbody is disabled
        //rb.MovePosition(rb.position + velocity * Time.deltaTime);

        // Ray from previous position to current position
        Ray ray = new Ray(previousPosition, transform.position - previousPosition);
        RaycastHit hit;

        // Check if the ball has collided with the pitch
        if (Physics.Raycast(ray, out hit, (transform.position - previousPosition).magnitude))
        {
            Debug.Log("Raycast hit: " + hit.collider.name);

            if (hit.collider.tag == "Pitch")
            {
                // Handle the collision
                Debug.Log("Ball collided with the pitch");
                HandleBallCollision(hit.point, hit.normal);
            }
            else
            {
                Debug.Log($"Collision detected but not with Pitch = {hit.collider.tag}.");
            }
        }
        else
        {
            Debug.Log("No collision detected.");
        }

        // Update the previous position
        previousPosition = transform.position;
    }

    void HandleBallCollision(Vector3 collisionPoint, Vector3 collisionNormal)
    {
        // Calculate the bounce
        Vector3 incomingVelocity = rb.linearVelocity;

        // Reflect the incoming velocity based on the collision normal
        Vector3 reflectedVelocity = Vector3.Reflect(incomingVelocity, collisionNormal);

        // Apply a bounce factor to control the intensity of the bounce
        bounceFactor = 0.7f; // Adjust this value for more or less bounce
        Vector3 bounceVelocity = reflectedVelocity * bounceFactor;

        // Apply the new velocity to the ball
        rb.linearVelocity = bounceVelocity;

        // Update the ball's position slightly above the collision point to prevent it from getting stuck
        transform.position = collisionPoint + collisionNormal * 0.05f;
    }
}
