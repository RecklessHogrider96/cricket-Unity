using System;
using UnityEngine;
using UnityEngine.Events;

public class GetPitchMarkerPositionEvent: UnityEvent<Action<Vector3>>
{
    public static GetPitchMarkerPositionEvent Instance = new GetPitchMarkerPositionEvent();
}
