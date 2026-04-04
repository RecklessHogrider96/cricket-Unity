using UnityEngine;

/// <summary>
/// Defines the delivery parameters for one bowler archetype.
/// All ranges are sampled randomly per delivery to produce natural variation.
/// </summary>
[CreateAssetMenu(fileName = "BowlerConfigSO", menuName = "Scriptable Objects/BowlerConfigSO")]
public class BowlerConfigSO : ScriptableObject
{
    [Header("Speed (m/s)")]
    public float minSpeed = 10f;
    public float maxSpeed = 20f;

    [Header("Spin — lateral velocity added after first bounce (m/s)")]
    [Tooltip("Spin magnitude is sampled from this range then oriented by BallController using the bowling arm.")]
    public float minSpin = 0f;
    public float maxSpin = 5f;

    [Header("Swing — lateral acceleration during flight (m/s²)")]
    public float minSwing = 0f;
    public float maxSwing = 3f;

    [Tooltip("+1 swings toward world +X, -1 swings toward world -X. " +
             "Right-arm in-swing is typically -1 (moves toward right-handed batsman's legs).")]
    public float swingDirection = 1f;
}
