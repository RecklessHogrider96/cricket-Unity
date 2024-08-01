using System;
using UnityEngine;

public class DraggableBallPitchMarker : MonoBehaviour
{
    private Vector3 offset;
    private Camera cam;

    void Start()
    {
        cam = Camera.main; // Assuming you have a main camera

        GetPitchMarkerPositionEvent.Instance.AddListener(OnGetPitchMarkerPositionEventHandler);
    }

    public void OnGetPitchMarkerPositionEventHandler(Action<Vector3> callback)
    {
        callback(transform.position);
    }

    void OnMouseDown()
    {
        // Calculate offset between mouse position and object position
        offset = transform.position - GetMouseWorldPosition();
    }

    void OnMouseDrag()
    {
        // Move the circle to the new mouse position
        Vector3 newPosition = GetMouseWorldPosition() + offset;
        newPosition.y = transform.position.y; // Lock the Y axis to stay on the pitch
        transform.position = newPosition;
    }

    Vector3 GetMouseWorldPosition()
    {
        // Get the mouse position in world space
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        Plane plane = new Plane(Vector3.up, transform.position); // Assuming a horizontal plane (Y = 0)
        float distance;
        if (plane.Raycast(ray, out distance))
        {
            return ray.GetPoint(distance);
        }
        return Vector3.zero;
    }
}
