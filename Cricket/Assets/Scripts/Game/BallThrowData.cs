using UnityEngine;

/// <summary>
/// All parameters needed to simulate a single ball delivery.
/// Passed through ThrowBallEvent so every subscriber gets consistent data.
/// </summary>
[System.Serializable]
public struct BallThrowData
{
    /// <summary>World-space position where the ball should pitch (first bounce).</summary>
    public Vector3 bounceTarget;

    /// <summary>Release speed in m/s sampled from the bowler's speed range.</summary>
    public float speed;

    /// <summary>
    /// Signed lateral velocity (m/s) added to the ball on the X axis after the first bounce.
    /// Positive = world +X, negative = world -X.
    /// Value comes directly from the HUD spin slider — direction is already encoded in the sign.
    /// BallController applies this as-is: velocity.x += data.spin.
    /// </summary>
    public float spin;

    /// <summary>
    /// Signed constant lateral acceleration (m/s²) applied to the X axis during flight.
    /// Positive = in-swing (+X), negative = out-swing (−X).
    /// Value comes directly from the HUD swing slider — direction is already encoded in the sign.
    /// </summary>
    public float swingAmount;

    /// <summary>
    /// Bowling arm — kept for future animation system use (Phase 4).
    /// No longer used to derive spin direction in BallController.
    /// </summary>
    public BowlerBowlingArm bowlingArm;
}
