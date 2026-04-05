using System;
using UnityEngine;

/// <summary>
/// Lets the player click and drag the pitch marker to choose where the ball will pitch.
/// Responds to GetPitchMarkerPositionEvent with its current world position.
///
/// ── Drag model ────────────────────────────────────────────────────────────────
///   The grab offset is stored in SCREEN SPACE (pixels) rather than world space.
///   This means the drag feels correct for any camera rotation — including cameras
///   facing opposite directions — because the screen is always "what the player
///   sees".  The screen-space target is unprojected onto the ground plane each
///   frame using the active camera's own projection, so the result is always
///   consistent with that camera's view.
/// </summary>
public class DraggableBallPitchMarker : MonoBehaviour
{
    [Tooltip("Cameras that are allowed to drive marker dragging.\n\n" +
             "When the mouse button goes down, the first ACTIVE camera in this list whose\n" +
             "viewport contains the mouse position is used for the whole drag.\n" +
             "Add every camera the player might be using (top-down, side-on, etc.).\n\n" +
             "Leaving the list empty falls back to Camera.main with a warning.")]
    [SerializeField] private Camera[] dragCameras = new Camera[0];

    // Screen-space pixel offset between the marker's projected position and the
    // mouse click point.  Stored on MouseDown, reused every MouseDrag frame.
    // Screen space is camera-agnostic so any rotation is handled automatically.
    private Vector2 screenOffset;
    private Camera  activeDragCamera;

    private void Start()
    {
        if (dragCameras == null || dragCameras.Length == 0)
            Debug.LogWarning("[DraggableBallPitchMarker] No drag cameras assigned — " +
                             "falling back to Camera.main. Assign at least one in the Inspector.");
    }

    private void OnEnable()
    {
        GetPitchMarkerPositionEvent.Instance.AddListener(OnGetPitchMarkerPositionEvent);
    }

    private void OnDisable()
    {
        GetPitchMarkerPositionEvent.Instance.RemoveListener(OnGetPitchMarkerPositionEvent);
    }

    private void OnGetPitchMarkerPositionEvent(Action<Vector3> callback)
    {
        callback(transform.position);
    }

    private void OnMouseDown()
    {
        activeDragCamera = FindCameraUnderMouse();
        if (activeDragCamera == null) return;

        // Store the gap between where the marker projects on screen and where
        // the mouse clicked.  Using screen space means we don't have to think
        // about world-space axis directions — it just follows the cursor.
        Vector2 markerScreen = activeDragCamera.WorldToScreenPoint(transform.position);
        screenOffset = markerScreen - (Vector2)Input.mousePosition;
    }

    private void OnMouseDrag()
    {
        if (activeDragCamera == null) return;

        // Reconstruct where the marker should be on screen (cursor + original offset),
        // then unproject that screen point onto the horizontal ground plane.
        Vector2 targetScreen = (Vector2)Input.mousePosition + screenOffset;

        Ray ray = activeDragCamera.ScreenPointToRay(targetScreen);
        Plane plane = new Plane(Vector3.up, transform.position);

        if (plane.Raycast(ray, out float distance))
        {
            Vector3 newPosition = ray.GetPoint(distance);
            newPosition.y = transform.position.y;  // lock Y — marker stays on the pitch plane
            transform.position = newPosition;
        }
    }

    private void OnMouseUp()
    {
        activeDragCamera = null;
    }

    /// <summary>
    /// Returns the first ACTIVE camera in dragCameras whose pixel rect contains
    /// the current mouse position.  Disabled cameras are skipped — they can still
    /// have a full-screen pixelRect and would otherwise be picked incorrectly.
    /// Falls back to Camera.main if the list is empty or no match is found.
    /// </summary>
    private Camera FindCameraUnderMouse()
    {
        if (dragCameras != null)
        {
            Vector2 mousePos = Input.mousePosition;
            foreach (Camera c in dragCameras)
            {
                if (c != null && c.gameObject.activeInHierarchy && c.pixelRect.Contains(mousePos))
                    return c;
            }
        }

        return Camera.main;
    }
}
