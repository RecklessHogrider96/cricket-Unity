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
///
///   Weather is folded in before any equations run:
///     effectiveSwing  = (bowlerSwing × weather.swingMultiplier) + weather.windX
///     effectiveSpeed  = bowlerSpeed + weather.windZ                (changes T)
///     effectiveG      = gravity − weather.windY                    (updraft reduces gravity)
///
///   x(t) = x0 + vx0·t + ½·effectiveSwing·t²
///   y(t) = y0 + vy0·t − ½·effectiveG·t²
///   z(t) = z0 + vz·t
///
///   Solved analytically so the ball always arrives at bounceTarget:
///     T   = horizontalDistance / effectiveSpeed
///     vz  = distZ / T
///     vx0 = (distX − ½·effectiveSwing·T²) / T
///     vy0 = (distY + ½·effectiveG·T²) / T
///
/// Phase 2a — Bouncing (Euler integration, step = Time.fixedDeltaTime)
///   Each step:  velocity.y -= effectiveG · dt
///               position   += velocity · dt
///   Ground contact (position.y ≤ groundLevel):
///     bounceFactor   = surface.bounceFactor   + weather.pitchBounceDelta   (clamped 0–1)
///     frictionFactor = surface.frictionFactor + weather.pitchFrictionDelta (clamped 0–1)
///     • Snap position.y to groundLevel BEFORE adding to trajectory
///     • Reflect:  velocity.y  = −velocity.y · bounceFactor
///     • Friction: velocity.xz *= frictionFactor
///     • Spin:     velocity.x  += bowlerSpin · weather.spinGripMultiplier  (first bounce only)
///   Surface (pitch vs outfield) chosen per-contact via CricketGameConstants.IsOnPitch().
///
/// Phase 2b — Rolling
///   rollingFriction = outfield.rollingFriction + weather.outfieldRollingDelta (clamped 0.8–1)
///   Ball locked to groundLevel; horizontal speed decays until speed < 0.05 m/s.
///
/// ── Coroutine playback ───────────────────────────────────────────────────────
///   All trajectory points are one simulationStep apart (from CricketGameConstants).
///   The step used during baking is stored in trajectoryStep and re-used by the
///   coroutine, so both always agree regardless of Unity's physics settings or framerate.
/// </summary>
public class BallController : MonoBehaviour
{
    [SerializeField] private List<Vector3> trajectoryPoints = new List<Vector3>();

    private Vector3   startPosition;
    private Coroutine moveCoroutine;
    private float     trajectoryStep; // time between consecutive trajectory points — set during baking

    private const int   MaxBounces    = 20;
    private const int   MaxRollSteps  = 600;    // 600 × simulationStep (0.02 s default) = 12 s max rolling
    private const float RollStopSpeed = 0.05f;  // m/s — below this the ball is considered still

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
        CricketDataController dataCtrl  = CricketGameModel.Instance.GetDataController();
        CricketGameConstants  constants = dataCtrl.GetGameConstants();
        WeatherConfigSO       weather   = CricketGameModel.Instance.GetSelectedWeather();

        float g           = constants.gravity;
        float groundLevel = constants.groundLevel;
        // Read simulation step from the SO — independent of Unity's physics timestep and framerate.
        // Stored in trajectoryStep so the playback coroutine uses the exact same value.
        float dt          = constants.simulationStep;
        trajectoryStep    = dt;
        bool  hasWeather  = weather != null;

        // ── Apply weather to Phase 1 parameters ──────────────────────────────

        // Wind Z: headwind/tailwind shifts effective speed → changes flight time T
        float effectiveSpeed = hasWeather
            ? Mathf.Max(1f, data.speed + weather.windZ)
            : data.speed;

        // Wind X: constant lateral force on top of bowler swing
        // Swing multiplier: atmosphere boosts or reduces conventional swing
        float effectiveSwing = hasWeather
            ? (data.swingAmount * weather.swingMultiplier) + weather.windX
            : data.swingAmount;

        // Wind Y: updraft/downdraft modifies effective gravity
        // Positive windY = updraft = reduces effective downward pull
        float effectiveG = hasWeather
            ? Mathf.Max(0.1f, g - weather.windY)
            : g;

        // ── Solve Phase 1 initial velocities ─────────────────────────────────
        float distX = data.bounceTarget.x - startPosition.x;
        float distY = data.bounceTarget.y - startPosition.y;
        float distZ = data.bounceTarget.z - startPosition.z;

        float horizontalDist = Mathf.Sqrt(distX * distX + distZ * distZ);
        if (horizontalDist < 0.001f)
        {
            Debug.LogWarning("[BallController] Bounce target is at the same XZ as ball start.");
            return false;
        }

        float T   = horizontalDist / effectiveSpeed;
        float vz  = distZ / T;
        float vx0 = (distX - 0.5f * effectiveSwing * T * T) / T;
        float vy0 = (distY + 0.5f * effectiveG      * T * T) / T;

        // ── Phase 1: flight to bounce point ──────────────────────────────────
        for (float t = 0f; t < T; t += dt)
        {
            Vector3 p;
            p.x = startPosition.x + vx0 * t + 0.5f * effectiveSwing * t * t;
            p.y = startPosition.y + vy0 * t  - 0.5f * effectiveG    * t * t;
            p.z = startPosition.z + vz  * t;
            trajectoryPoints.Add(p);
        }

        // Always include the exact bounce target as the final Phase 1 point
        trajectoryPoints.Add(data.bounceTarget);

        // Velocity at the moment the ball reaches the bounce point
        Vector3 velocity = new Vector3(
            vx0 + effectiveSwing * T,
            vy0 - effectiveG     * T,   // negative = moving downward at impact
            vz
        );

        // ── Phase 2a + 2b: bouncing then rolling ──────────────────────────────
        BuildPostBounceTrajectory(
            data.bounceTarget, velocity, data,
            effectiveG, groundLevel, dt,
            weather, dataCtrl);

        return trajectoryPoints.Count > 1;
    }

    private void BuildPostBounceTrajectory(
        Vector3               position,
        Vector3               velocity,
        BallThrowData         data,
        float                 g,
        float                 groundLevel,
        float                 dt,
        WeatherConfigSO       weather,
        CricketDataController dataCtrl)
    {
        bool hasWeather  = weather != null;
        bool spinApplied = false;
        int  bounceCount = 0;

        // ── Phase 2a: Bouncing ────────────────────────────────────────────────
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

                // Weather offsets applied to bounce factors (clamped to valid range)
                float bounceFactor = hasWeather
                    ? Mathf.Clamp01(surface.bounceFactor   + weather.pitchBounceDelta)
                    : surface.bounceFactor;

                float frictionFactor = hasWeather
                    ? Mathf.Clamp01(surface.frictionFactor + weather.pitchFrictionDelta)
                    : surface.frictionFactor;

                // Vertical reflection
                velocity.y = -velocity.y * bounceFactor;

                // Horizontal damping at impact
                velocity.x *= frictionFactor;
                velocity.z *= frictionFactor;

                // Spin applied once, on the first bounce only.
                // spinGripMultiplier: wet pitch = less turn, dusty pitch = more turn.
                if (!spinApplied)
                {
                    float spinGrip = hasWeather ? weather.spinGripMultiplier : 1f;
                    velocity.x += data.spin * spinGrip;
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

        // ── Phase 2b: Rolling — smooth ground-level deceleration ─────────────
        SurfaceConfigSO rollSurface = dataCtrl.GetSurfaceConfig(position);

        // Weather offset on rolling friction (clamped to a sensible minimum)
        float rollingFriction = hasWeather
            ? Mathf.Clamp(rollSurface.rollingFriction + weather.outfieldRollingDelta, 0.8f, 1f)
            : rollSurface.rollingFriction;

        position.y = groundLevel;
        velocity.y = 0f;

        for (int step = 0; step < MaxRollSteps; step++)
        {
            float horizontalSpeed = Mathf.Sqrt(velocity.x * velocity.x + velocity.z * velocity.z);
            if (horizontalSpeed < RollStopSpeed) break;

            velocity.x *= rollingFriction;
            velocity.z *= rollingFriction;
            position.x += velocity.x * dt;
            position.z += velocity.z * dt;
            // position.y stays locked to groundLevel

            trajectoryPoints.Add(position);
        }
    }

    // ── Coroutine playback ────────────────────────────────────────────────────

    /// <summary>
    /// Maps real elapsed time to the pre-baked trajectory array.
    /// Because every point was calculated one simulationStep apart, elapsed / dt
    /// gives the correct index regardless of framerate.
    /// Interpolation between adjacent points keeps the ball visually smooth even
    /// when the render frame rate doesn't align with the simulation step.
    /// The ball naturally slows down after each bounce because post-bounce points
    /// are physically closer together.
    /// </summary>
    private IEnumerator MoveBallAlongTrajectory()
    {
        if (trajectoryPoints.Count < 2) yield break;

        // Use the same step that was used to bake the trajectory — guaranteed to match.
        float dt      = trajectoryStep;
        float elapsed = 0f;
        int   last    = trajectoryPoints.Count - 1;

        while (true)
        {
            elapsed += Time.deltaTime;

            int index = Mathf.FloorToInt(elapsed / dt);
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
