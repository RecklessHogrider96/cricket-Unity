using UnityEngine;

/// <summary>
/// Defines the physical properties of a playing surface.
/// Create separate assets for the pitch and outfield to give them different behaviours.
///
/// All factors are dimensionless ratios applied per-event (bounce) or per-step (rolling).
/// </summary>
[CreateAssetMenu(fileName = "SurfaceConfig", menuName = "Scriptable Objects/SurfaceConfig")]
public class SurfaceConfigSO : ScriptableObject
{
    [Header("Bounce")]
    [Tooltip("Fraction of vertical speed retained after each bounce.\n" +
             "Hard/dry pitch ≈ 0.70  |  Damp/green pitch ≈ 0.55  |  Outfield grass ≈ 0.62")]
    [Range(0f, 1f)]
    public float bounceFactor = 0.70f;

    [Tooltip("Fraction of horizontal (XZ) speed retained at the moment of each bounce. " +
             "Simulates surface grip on impact.\n" +
             "Hard pitch ≈ 0.88  |  Outfield grass ≈ 0.78")]
    [Range(0f, 1f)]
    public float frictionFactor = 0.88f;

    [Header("Rolling")]
    [Tooltip("Fraction of horizontal speed retained every physics step (0.02 s) while the ball rolls along the ground. " +
             "Values very close to 1 produce gradual deceleration.\n" +
             "Hard pitch ≈ 0.985  |  Outfield grass ≈ 0.972\n" +
             "At 0.972 per step the ball loses ~75% of speed in ≈2 s of rolling.")]
    [Range(0.9f, 1f)]
    public float rollingFriction = 0.978f;
}
