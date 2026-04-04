using UnityEngine.Events;

/// <summary>
/// Fired by CricketGameController when the player commits to a delivery.
/// Carries the full BallThrowData so every subscriber (BallController,
/// BowlerAnimationController, etc.) receives consistent parameters.
/// </summary>
public class ThrowBallEvent : UnityEvent<BallThrowData>
{
    public static readonly ThrowBallEvent Instance = new ThrowBallEvent();
}
