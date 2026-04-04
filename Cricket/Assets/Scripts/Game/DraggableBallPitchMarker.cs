using System;
using UnityEngine;

/// <summary>
/// Lets the player click and drag the pitch marker to choose where the ball will pitch.
/// Responds to GetPitchMarkerPositionEvent with its current world position.
/// </summary>
public class DraggableBallPitchMarker : MonoBehaviour
{
    private Vector3 offset;
    private Camera cam;

    private void Start()
    {
        cam = Camera.main;
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
        if (TryGetMouseWorldPosition(out Vector3 worldPos))
            offset = transform.position - worldPos;
    }

    private void OnMouseDrag()
    {
        if (TryGetMouseWorldPosition(out Vector3 worldPos))
        {
            Vector3 newPosition = worldPos + offset;
            newPosition.y = transform.position.y;   // lock Y — marker stays on the pitch plane
            transform.position = newPosition;
        }
    }

    /// <summary>
    /// Projects the mouse cursor onto the horizontal plane at the marker's Y height.
    /// Returns false (and leaves worldPos at zero) if the ray misses the plane,
    /// so callers can safely skip the update rather than snapping to the origin.
    /// </summary>
    private bool TryGetMouseWorldPosition(out Vector3 worldPos)
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        Plane plane = new Plane(Vector3.up, transform.position);

        if (plane.Raycast(ray, out float distance))
        {
            worldPos = ray.GetPoint(distance);
            return true;
        }

        worldPos = Vector3.zero;
        return false;
    }
}
