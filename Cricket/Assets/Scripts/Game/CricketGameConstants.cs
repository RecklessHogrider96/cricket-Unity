using UnityEngine;

/// <summary>
/// Game-wide physical constants. All values are authoritative — nothing uses
/// Unity's built-in Physics system; every equation reads directly from here.
/// </summary>
[CreateAssetMenu(fileName = "CricketGameConstants", menuName = "Scriptable Objects/CricketGameConstants")]
public class CricketGameConstants : ScriptableObject
{
    [Header("Physics")]
    [Tooltip("Gravitational acceleration magnitude in m/s². Applied downward in all kinematic equations. " +
             "Standard value is 9.81; 10 is a common cricket-simulation simplification.")]
    public float gravity = 9.81f;

    [Header("Ground")]
    [Tooltip("World-space Y position of the playing surface. " +
             "Set this to match the top of the pitch/ground mesh in your scene.")]
    public float groundLevel = 0f;

    [Header("Pitch Bounds (World-Space XZ)")]
    [Tooltip("Left edge of the pitch strip.")]
    public float pitchMinX = -1.52f;   // Standard pitch width: 3.05 m (10 ft)
    [Tooltip("Right edge of the pitch strip.")]
    public float pitchMaxX = 1.52f;
    [Tooltip("Bowler's end Z limit.")]
    public float pitchMinZ = 0f;
    [Tooltip("Batsman's end Z limit. Standard pitch length: 20.12 m (22 yards).")]
    public float pitchMaxZ = 20.12f;

    /// <summary>
    /// Returns true when the given world-space XZ position lies inside the pitch strip.
    /// Used by BallController to choose between pitch and outfield surface configs.
    /// </summary>
    public bool IsOnPitch(Vector3 position)
    {
        return position.x >= pitchMinX && position.x <= pitchMaxX &&
               position.z >= pitchMinZ && position.z <= pitchMaxZ;
    }
}
