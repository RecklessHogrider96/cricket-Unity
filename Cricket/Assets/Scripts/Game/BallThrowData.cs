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
    /// Lateral velocity (m/s) added to the ball on the X axis after the first bounce.
    /// Positive = world +X, negative = world -X.
    /// </summary>
    public float spin;

    /// <summary>
    /// Constant lateral acceleration (m/s²) applied to the X axis during flight.
    /// Sign encodes direction: derived from BowlerConfigSO.swingDirection.
    /// </summary>
    public float swingAmount;

    /// <summary>Bowling arm, used to orient spin direction after bounce.</summary>
    public BowlerBowlingArm bowlingArm;
}
