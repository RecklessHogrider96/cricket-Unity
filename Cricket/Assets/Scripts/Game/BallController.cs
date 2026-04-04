using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Simulates ball flight using explicit kinematic equations.
/// No Unity Rigidbody or Physics system is used — every delivery is fully deterministic.
///
/// ── Physics model ────────────────────────────────────────────────────────────
///
/// Phase 1 — Pre-bounce flight (bowler release → pitch marker)
///   x(t) = x0 + vx0·t + ½·swingAcc·t²   (swing = constant lateral acceleration, m/s²)
///   y(t) = y0 + vy0·t − ½·g·t²           (g = positive magnitude from CricketGameConstants)
///   z(t) = z0 + vz·t
///
///   Initial velocities are solved analytically so the ball arrives exactly at
///   bounceTarget at t = T = horizontalDistance / speed.
///     vz  = distZ / T
///     vx0 = (distX − ½·swingAcc·T²) / T
///     vy0 = (distY + ½·g·T²) / T
///
/// Phase 2a — Bouncing (Euler integration, fixed timeStep = Time.fixedDeltaTime)
///   Each step:  velocity.y -= g · dt
///               position   += velocity · dt
///   Ground contact (position.y ≤ groundLevel):
///     • Snap ball to groundLevel  ← done BEFORE adding point to trajectory
///     • Reflect:  velocity.y = −velocity.y · surface.bounceFactor
///     • Friction: velocity.xz *= surface.frictionFactor
///     • Spin:     velocity.x  += spin · armSign  (first bounce only)
///   Surface (pitch vs outfield) chosen per-contact via CricketGameConstants.IsOnPitch().
///
/// Phase 2b — Rolling (after bounces become negligible)
///   Ball locked to groundLevel; horizontal speed decays by surface.rollingFriction
///   per step until speed < 0.05 m/s.
///
/// ── Coroutine playback ───────────────────────────────────────────────────────
///   Each trajectory point was calculated one fixedDeltaTime apart.
///   The coroutine maps real elapsed time → point index, interpolating between
///   adjacent points. This correctly represents the ball slowing after each bounce
///   without any speed-based segment calculation.
/// </summary>
public class BallController : MonoBehaviour
{
    [SerializeField] private List<Vector3> trajectoryPoints = new List<Vector3>();

    private Vector3 startPosition;
    private Coroutine moveCoroutine;

    private const int MaxBounces  = 20;
    private const int MaxRollSteps = 600;   // 600 × 0.02 s = 12 s max rolling time
    private const float RollStopSpeed = 0.05f; // m/s — below this the ball is considered still

    // ── Event wiring ─────────────────────────────────────────────────────────

    private void OnEnable()
    {
        startPosition = transform.position;
        ThrowBallEvent.Instance.AddListener(OnThrowBallEvent);
    }

    private void OnDisable()
    {
        ThrowBallEvent.Instance.RemoveListener(OnThrowBallEvent);
    }

    // ── Delivery entry point ──────────────────────────────────────────────────

    private void OnThrowBallEvent(BallThrowData data)
    {
        ResetBall();
        trajectoryPoints = new List<Vector3>();

        if (!BuildTrajectory(data))
        {
            Debug.LogWarning("[BallController] Could not build trajectory — delivery skipped.");
            return;
        }

        moveCoroutine = StartCoroutine(MoveBallAlongTrajectory());
    }

    // ── Trajectory building ───────────────────────────────────────────────────

    private bool BuildTrajectory(BallThrowData data)
    {
        CricketGameConstants constants =
            CricketGameModel.Instance.GetDataController().GetGameConstants();

        float g           = constants.gravity;          // positive magnitude, applied downward
        float groundLevel = constants.groundLevel;
        float dt          = Time.fixedDeltaTime;

        // ── Solve Phase 1 initial velocities ─────────────────────────────
        float distX = data.bounceTarget.x - startPosition.x;
        float distY = data.bounceTarget.y - startPosition.y;
        float distZ = data.bounceTarget.z - startPosition.z;

        float horizontalDist = Mathf.Sqrt(distX * distX + distZ * distZ);
        if (horizontalDist < 0.001f)
        {
            Debug.LogWarning("[BallController] Bounce target is at the same XZ as ball start.");
            return false;
        }

        float T    = horizontalDist / data.speed;
        float vz   = distZ / T;
        float swingAcc = data.swingAmount;
        float vx0  = (distX - 0.5f * swingAcc * T * T) / T;
        float vy0  = (distY + 0.5f * g * T * T) / T;

        // ── Phase 1: flight to bounce point ──────────────────────────────
        for (float t = 0f; t < T; t += dt)
        {
            Vector3 p;
            p.x = startPosition.x + vx0 * t + 0.5f * swingAcc * t * t;
            p.y = startPosition.y + vy0 * t  - 0.5f * g        * t * t;
            p.z = startPosition.z + vz  * t;
            trajectoryPoints.Add(p);
        }

        // Always include the exact bounce target as the final Phase 1 point
        trajectoryPoints.Add(data.bounceTarget);

        // Velocity at the moment the ball reaches the bounce point
        Vector3 velocity = new Vector3(
            vx0 + swingAcc * T,
            vy0 - g * T,        // negative = moving downward
            vz
        );

        // ── Phase 2a + 2b: bouncing then rolling ──────────────────────────
        BuildPostBounceTrajectory(data.bounceTarget, velocity, data, g, groundLevel, dt);

        return trajectoryPoints.Count > 1;
    }

    private void BuildPostBounceTrajectory(
        Vector3 position, Vector3 velocity,
        BallThrowData data,
        float g, float groundLevel, float dt)
    {
        CricketDataController dataCtrl =
            CricketGameModel.Instance.GetDataController();

        float spinSign    = data.bowlingArm == BowlerBowlingArm.Right ? 1f : -1f;
        bool  spinApplied = false;
        int   bounceCount = 0;

        // ── Phase 2a: Bouncing ────────────────────────────────────────────
        while (bounceCount < MaxBounces)
        {
            velocity.y -= g * dt;
            position   += velocity * dt;

            // Ground contact — snap BEFORE adding to trajectory so no point
            // ever appears below the surface.
            if (position.y <= groundLevel)
            {
                position.y = groundLevel;

                SurfaceConfigSO surface = dataCtrl.GetSurfaceConfig(position);

                // Vertical reflection
                velocity.y = -velocity.y * surface.bounceFactor;

                // Horizontal damping at impact
                velocity.x *= surface.frictionFactor;
                velocity.z *= surface.frictionFactor;

                // Spin applied once, on the first bounce only
                if (!spinApplied)
                {
                    velocity.x += data.spin * spinSign;
                    spinApplied = true;
                }

                bounceCount++;
            }

            trajectoryPoints.Add(position);

            // Transition to rolling once vertical energy is negligible
            if (bounceCount > 0
                && Mathf.Abs(velocity.y) < 0.2f
                && position.y <= groundLevel + 0.01f)
            {
                break;
            }
        }

        // ── Phase 2b: Rolling — smooth ground-level deceleration ─────────
        SurfaceConfigSO rollSurface = dataCtrl.GetSurfaceConfig(position);

        position.y  = groundLevel;
        velocity.y  = 0f;

        for (int step = 0; step < MaxRollSteps; step++)
        {
            float horizontalSpeed = Mathf.Sqrt(velocity.x * velocity.x + velocity.z * velocity.z);
            if (horizontalSpeed < RollStopSpeed) break;

            velocity.x *= rollSurface.rollingFriction;
            velocity.z *= rollSurface.rollingFriction;
            position.x += velocity.x * dt;
            position.z += velocity.z * dt;
            // position.y stays locked to groundLevel

            trajectoryPoints.Add(position);
        }
    }

    // ── Coroutine playback ────────────────────────────────────────────────────

    /// <summary>
    /// Maps real elapsed time to the pre-baked trajectory array.
    /// Because every point was calculated one fixedDeltaTime apart, elapsed / dt
    /// gives the correct index. Interpolation between adjacent points keeps the
    /// ball visually smooth even when the frame rate doesn't align with dt.
    /// The ball naturally appears to slow down after each bounce because
    /// post-bounce points are physically closer together.
    /// </summary>
    private IEnumerator MoveBallAlongTrajectory()
    {
        if (trajectoryPoints.Count < 2) yield break;

        float dt      = Time.fixedDeltaTime;
        float elapsed = 0f;
        int   last    = trajectoryPoints.Count - 1;

        while (true)
        {
            elapsed += Time.deltaTime;

            int   index = Mathf.FloorToInt(elapsed / dt);
            if (index >= last)
            {
                transform.position = trajectoryPoints[last];
                break;
            }

            float frac = (elapsed - index * dt) / dt;
            transform.position = Vector3.Lerp(
                trajectoryPoints[index],
                trajectoryPoints[index + 1],
                frac);

            yield return null;
        }
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private void ResetBall()
    {
        if (moveCoroutine != null)
            StopCoroutine(moveCoroutine);

        transform.position = startPosition;
    }

    // ── Editor visualisation ──────────────────────────────────────────────────

    private void OnDrawGizmos()
    {
        if (trajectoryPoints == null || trajectoryPoints.Count < 2) return;

        Gizmos.color = Color.red;
        for (int i = 0; i < trajectoryPoints.Count - 1; i++)
            Gizmos.DrawLine(trajectoryPoints[i], trajectoryPoints[i + 1]);
    }
}
