using UnityEngine;
using UnityEngine.Events;

public class ThrowBallEvent : UnityEvent<Vector3, Vector3>
{
    public static ThrowBallEvent Instance = new ThrowBallEvent();
}
