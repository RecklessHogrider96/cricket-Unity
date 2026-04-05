using UnityEngine;

/// <summary>
/// Translates player input into game events and keeps scene cricket objects
/// (pitch mesh, stumps) aligned to the values in CricketGameConstants.
///
/// ── Scene alignment ────────────────────────────────────────────────────────────
///   Assign the Pitch mesh Transform and hit "Align Cricket Objects" from the
///   right-click context menu (works in edit mode — no Play required).
///   Stumps are instantiated as Unity Cylinder primitives in code; assign a
///   Material to the Stump Material slot so they inherit it automatically.
///
/// ── Debug mode ─────────────────────────────────────────────────────────────────
///   When debugMode is true LineRenderers + Gizmos draw the pitch boundary,
///   stump lines, and popping-crease lines for both Scene and Game views.
/// </summary>
public class CricketGameController : MonoBehaviour
{
    // ── Cricket Law constants used for gizmos only (Law 7) ───────────────────
    private const float PoppingCreaseOffset = 1.22f;   // Law 7.2 — 4 ft in front of stumps
    private const float StumpHalfSpan       = 0.1143f; // half of 9-inch overall stump span

    // ── Inspector — Scene References ──────────────────────────────────────────

    [Header("Scene References")]

    [Tooltip("Direct reference to CricketGameConstants.\n" +
             "Required for edit-mode alignment (the runtime singleton is not available then).\n" +
             "Assign the same asset that CricketDataController references.")]
    [SerializeField] private CricketGameConstants constants;

    [Tooltip("The Pitch mesh Transform.\n\n" +
             "Position is centred on the pitch; XZ scale covers the exact bounds from\n" +
             "CricketGameConstants. Y scale is preserved.\n\n" +
             "Set 'Pitch Mesh Native Size' to match your mesh at localScale (1,1,1):\n" +
             "  Unity default Plane → (10, 10)\n" +
             "  Unity Quad          → (1, 1)")]
    [SerializeField] private Transform pitchObject;

    [Tooltip("Native XZ footprint of the Pitch mesh at localScale (1, 1, 1).\n" +
             "Unity default Plane = (10, 10).  A Quad = (1, 1).")]
    [SerializeField] private Vector2 pitchMeshNativeSize = new Vector2(10f, 10f);

    // ── Inspector — Stumps ────────────────────────────────────────────────────

    [Header("Stumps")]

    [Tooltip("Material applied to every spawned stump cylinder.\n" +
             "Leave empty to keep the default Unity material.")]
    [SerializeField] private Material stumpMaterial;

    [Tooltip("World-space height of each stump above groundLevel.\n" +
             "Law 8.2 real value = 0.7112 m (28 inches).")]
    [SerializeField] private float stumpHeight = 0.7112f;

    [Tooltip("World-space diameter of each stump cylinder.\n" +
             "Real value = 0.0349 m (1.375 in) — very thin.\n" +
             "0.05 – 0.08 m is a good game-art compromise for visibility.")]
    [SerializeField] private float stumpDiameter = 0.06f;

    [Tooltip("World-space centre-to-centre distance between the middle stump and each outer stump.\n" +
             "Real value = 0.09685 m.")]
    [SerializeField] private float stumpSpacing = 0.09685f;

    // ── Inspector — Cameras ───────────────────────────────────────────────────

    [Header("Cameras")]

    [Tooltip("All gameplay cameras. Only the active one is enabled at a time.\n" +
             "Press Tab at runtime to cycle forward through the list.\n" +
             "The first camera in the list is activated on Start; the rest are disabled.")]
    [SerializeField] private Camera[] cameras = new Camera[0];

    // ── Inspector — Debug ─────────────────────────────────────────────────────

    [Header("Debug Overlay")]

    [Tooltip("Enables pitch markings in both Scene and Game views.\n\n" +
             "Green  — pitch boundary\n" +
             "White  — stump span (9 inches) at both ends\n" +
             "Blue   — popping crease (1.22 m from stumps) at both ends")]
    [SerializeField] private bool debugMode = false;

    [Tooltip("Colour used for the pitch boundary overlay.\n" +
             "Alpha controls Scene-view fill opacity; Game-view lines are fully opaque.")]
    [SerializeField] private Color debugPitchColour = new Color(0.15f, 0.85f, 0.25f, 0.25f);

    // ── Runtime state ─────────────────────────────────────────────────────────

    private GameObject stumpsContainer;  // parent for all spawned stump cylinders
    private GameObject debugContainer;  // parent for all debug LineRenderers
    private int activeCameraIndex;       // index into cameras[] of the currently enabled camera

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    private void Start()
    {
        AlignSceneObjects();
        RefreshDebugVisuals();
        InitialiseCameras();
    }

    private void OnDestroy()
    {
        DestroyStumps();
        DestroyDebugVisuals();
    }

    private void OnValidate()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.delayCall += () =>
        {
            if (this == null) return;
            AlignSceneObjects();
            if (Application.isPlaying)
                RefreshDebugVisuals();
        };
#else
        AlignSceneObjects();
        RefreshDebugVisuals();
#endif
    }

    // ── Input ─────────────────────────────────────────────────────────────────

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            GetPitchMarkerPositionEvent.Instance.Invoke(OnMarkerPositionReceived);

        if (Input.GetKeyDown(KeyCode.Tab))
            CycleCamera();
    }

    private void OnMarkerPositionReceived(Vector3 markerPosition)
    {
        BallThrowData throwData = CricketGameModel.Instance.GetThrowParameters(markerPosition);
        ThrowBallEvent.Instance.Invoke(throwData);
    }

    // ── Camera cycling ────────────────────────────────────────────────────────

    /// <summary>
    /// Enables only cameras[0] and disables all others.
    /// Called once on Start so the scene state is always consistent regardless
    /// of how the cameras were left in the editor.
    /// </summary>
    private void InitialiseCameras()
    {
        if (cameras == null || cameras.Length == 0) return;

        activeCameraIndex = 0;
        for (int i = 0; i < cameras.Length; i++)
        {
            if (cameras[i] != null)
                cameras[i].gameObject.SetActive(i == 0);
        }
    }

    /// <summary>
    /// Disables the current camera, advances the index (wrapping at the end),
    /// and enables the next one.
    /// </summary>
    private void CycleCamera()
    {
        if (cameras == null || cameras.Length <= 1) return;

        cameras[activeCameraIndex]?.gameObject.SetActive(false);

        activeCameraIndex = (activeCameraIndex + 1) % cameras.Length;

        cameras[activeCameraIndex]?.gameObject.SetActive(true);
    }

    // ── Scene object alignment ────────────────────────────────────────────────

    [ContextMenu("Align Cricket Objects")]
    private void AlignSceneObjects()
    {
        CricketGameConstants c = GetConstants();
        if (c == null)
        {
            Debug.LogWarning("[CricketGameController] Cannot align — assign CricketGameConstants " +
                             "in the Inspector or ensure CricketGameModel is in the scene.");
            return;
        }

        AlignPitch(c);
        RespawnStumps(c);
    }

    // ── Pitch ─────────────────────────────────────────────────────────────────

    private void AlignPitch(CricketGameConstants c)
    {
        if (pitchObject == null) return;

        float width  = c.pitchMaxX - c.pitchMinX;
        float length = c.pitchMaxZ - c.pitchMinZ;
        float cx     = (c.pitchMinX + c.pitchMaxX) * 0.5f;
        float cz     = (c.pitchMinZ + c.pitchMaxZ) * 0.5f;

        pitchObject.position = new Vector3(cx, c.groundLevel, cz);

        Vector3 s = pitchObject.localScale;
        pitchObject.localScale = new Vector3(
            width  / pitchMeshNativeSize.x,
            s.y,
            length / pitchMeshNativeSize.y);
    }

    // ── Stumps ────────────────────────────────────────────────────────────────

    /// <summary>
    /// Destroys any previously spawned stumps and creates fresh ones for both ends.
    ///
    /// Unity Cylinder mesh native dimensions at localScale (1,1,1):
    ///   Height  = 2 units  (vertices at Y = −1 and Y = +1)
    ///   Diameter = 1 unit  (radius = 0.5 in X and Z)
    ///
    /// Therefore:
    ///   localScale.y = stumpHeight / 2   →  world height = stumpHeight
    ///   localScale.x = localScale.z = stumpDiameter  →  world diameter = stumpDiameter
    /// </summary>
    private void RespawnStumps(CricketGameConstants c)
    {
        DestroyStumps();

        stumpsContainer = new GameObject("[Stumps]");
        stumpsContainer.transform.SetParent(transform);

        SpawnStumpSet("StumpsA_Bowling", c.pitchMinZ, c);
        SpawnStumpSet("StumpsB_Batting", c.pitchMaxZ, c);
    }

    private void SpawnStumpSet(string groupName, float pitchEndZ, CricketGameConstants c)
    {
        // Group container keeps the hierarchy tidy.
        GameObject group = new GameObject(groupName);
        group.transform.SetParent(stumpsContainer.transform);

        // Cylinder pivot is at its geometric centre.
        // Raise by half the height so the base sits at groundLevel.
        float pivotY = c.groundLevel + stumpHeight * 0.5f;

        // Unity Cylinder native height = 2 units in Y → divide by 2 to get world height.
        float scaleY = stumpHeight / 2f;

        float[] xPositions = { -stumpSpacing, 0f, stumpSpacing };
        string[] stumpNames = { "Stump_Leg", "Stump_Middle", "Stump_Off" };

        for (int i = 0; i < 3; i++)
        {
            GameObject stump = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            stump.name = stumpNames[i];
            stump.transform.SetParent(group.transform);

            stump.transform.position   = new Vector3(xPositions[i], pivotY, pitchEndZ);
            stump.transform.localScale = new Vector3(stumpDiameter, scaleY, stumpDiameter);

            // Remove the auto-added CapsuleCollider — stumps are visual only for now.
            DestroyObj(stump.GetComponent<CapsuleCollider>());

            // Apply the shared stump material if one is assigned.
            if (stumpMaterial != null)
                stump.GetComponent<Renderer>().sharedMaterial = stumpMaterial;
        }
    }

    private void DestroyStumps()
    {
        if (stumpsContainer == null) return;
        DestroyObj(stumpsContainer);
        stumpsContainer = null;
    }

    // ── Debug visuals (Game view — LineRenderers) ─────────────────────────────

    private void RefreshDebugVisuals()
    {
        DestroyDebugVisuals();
        if (debugMode) BuildDebugVisuals();
    }

    private void BuildDebugVisuals()
    {
        CricketGameConstants c = GetConstants();
        if (c == null) return;

        debugContainer = new GameObject("[Debug] PitchMarkings");
        debugContainer.transform.SetParent(transform);

        float y = c.groundLevel + 0.005f;

        Color solidGreen = debugPitchColour;
        solidGreen.a = 1f;

        CreateLine("PitchBoundary", solidGreen, 0.05f, loop: true, new[]
        {
            new Vector3(c.pitchMinX, y, c.pitchMinZ),
            new Vector3(c.pitchMaxX, y, c.pitchMinZ),
            new Vector3(c.pitchMaxX, y, c.pitchMaxZ),
            new Vector3(c.pitchMinX, y, c.pitchMaxZ),
        });

        CreateLine("Stumps_BowlingEnd", Color.white, 0.04f, loop: false, new[]
        {
            new Vector3(-StumpHalfSpan, y, c.pitchMinZ),
            new Vector3( StumpHalfSpan, y, c.pitchMinZ),
        });

        CreateLine("Stumps_BattingEnd", Color.white, 0.04f, loop: false, new[]
        {
            new Vector3(-StumpHalfSpan, y, c.pitchMaxZ),
            new Vector3( StumpHalfSpan, y, c.pitchMaxZ),
        });

        Color blue = new Color(0.35f, 0.65f, 1f, 1f);
        float bowlingPopping = c.pitchMinZ + PoppingCreaseOffset;
        float battingPopping = c.pitchMaxZ - PoppingCreaseOffset;

        CreateLine("PoppingCrease_BowlingEnd", blue, 0.03f, loop: false, new[]
        {
            new Vector3(c.pitchMinX, y, bowlingPopping),
            new Vector3(c.pitchMaxX, y, bowlingPopping),
        });

        CreateLine("PoppingCrease_BattingEnd", blue, 0.03f, loop: false, new[]
        {
            new Vector3(c.pitchMinX, y, battingPopping),
            new Vector3(c.pitchMaxX, y, battingPopping),
        });
    }

    private void CreateLine(string lineName, Color colour, float width, bool loop, Vector3[] points)
    {
        GameObject go = new GameObject(lineName);
        go.transform.SetParent(debugContainer.transform);

        LineRenderer lr = go.AddComponent<LineRenderer>();
        lr.useWorldSpace     = true;
        lr.loop              = loop;
        lr.positionCount     = points.Length;
        lr.widthMultiplier   = width;
        lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        lr.receiveShadows    = false;
        lr.SetPositions(points);

        Shader shader = Shader.Find("HDRP/Unlit")
                     ?? Shader.Find("Unlit/Color")
                     ?? Shader.Find("Sprites/Default");

        Material mat = new Material(shader);
        if      (mat.HasProperty("_UnlitColor")) mat.SetColor("_UnlitColor", colour);
        else if (mat.HasProperty("_Color"))      mat.SetColor("_Color",      colour);

        lr.material = mat;
    }

    private void DestroyDebugVisuals()
    {
        if (debugContainer == null) return;
        DestroyObj(debugContainer);
        debugContainer = null;
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    /// <summary>Calls Destroy or DestroyImmediate depending on whether the app is playing.</summary>
    private void DestroyObj(Object obj)
    {
        if (obj == null) return;
        if (Application.isPlaying) Destroy(obj);
        else                       DestroyImmediate(obj);
    }

    private CricketGameConstants GetConstants()
    {
        if (constants != null) return constants;
        CricketGameModel model = CricketGameModel.Instance;
        return model?.GetDataController()?.GetGameConstants();
    }

    // ── Scene-view Gizmos ─────────────────────────────────────────────────────

    private void OnDrawGizmos()
    {
        if (!debugMode) return;

        CricketGameConstants c = GetConstants();
        if (c == null) return;

        float y   = c.groundLevel;
        float cx  = (c.pitchMinX + c.pitchMaxX) * 0.5f;
        float cz  = (c.pitchMinZ + c.pitchMaxZ) * 0.5f;
        float w   = c.pitchMaxX - c.pitchMinX;
        float len = c.pitchMaxZ - c.pitchMinZ;

        float bowlingPopping = c.pitchMinZ + PoppingCreaseOffset;
        float battingPopping = c.pitchMaxZ - PoppingCreaseOffset;
        Color blue           = new Color(0.35f, 0.65f, 1f, 1f);
        Color solidGreen     = new Color(debugPitchColour.r, debugPitchColour.g, debugPitchColour.b, 1f);

        Gizmos.color = debugPitchColour;
        Gizmos.DrawCube(new Vector3(cx, y, cz), new Vector3(w, 0f, len));

        Gizmos.color = solidGreen;
        Gizmos.DrawWireCube(new Vector3(cx, y, cz), new Vector3(w, 0f, len));

        Gizmos.color = Color.white;
        Gizmos.DrawLine(new Vector3(-StumpHalfSpan, y, c.pitchMinZ),
                        new Vector3( StumpHalfSpan, y, c.pitchMinZ));
        Gizmos.DrawLine(new Vector3(-StumpHalfSpan, y, c.pitchMaxZ),
                        new Vector3( StumpHalfSpan, y, c.pitchMaxZ));

        Gizmos.color = blue;
        Gizmos.DrawLine(new Vector3(c.pitchMinX, y, bowlingPopping),
                        new Vector3(c.pitchMaxX, y, bowlingPopping));
        Gizmos.DrawLine(new Vector3(c.pitchMinX, y, battingPopping),
                        new Vector3(c.pitchMaxX, y, battingPopping));

#if UNITY_EDITOR
        float lx = c.pitchMaxX + 0.15f;
        UnityEditor.Handles.color = solidGreen;
        UnityEditor.Handles.Label(new Vector3(cx, y + 0.1f, c.pitchMinZ - 0.3f),
                                  $"Pitch  {w:F2} × {len:F2} m");
        UnityEditor.Handles.color = Color.white;
        UnityEditor.Handles.Label(new Vector3(lx, y, c.pitchMinZ),    "Stumps (bowling)");
        UnityEditor.Handles.Label(new Vector3(lx, y, c.pitchMaxZ),    "Stumps (batting)");
        UnityEditor.Handles.color = blue;
        UnityEditor.Handles.Label(new Vector3(lx, y, bowlingPopping), "Popping crease");
        UnityEditor.Handles.Label(new Vector3(lx, y, battingPopping), "Popping crease");
#endif
    }
}
