using UnityEngine;
using UnityEngine.Events;

public class ThrowBallEvent : UnityEvent<Vector3, float>
{
    public static ThrowBallEvent Instance = new ThrowBallEvent();
}
