using UnityEngine;

[CreateAssetMenu(fileName = "BowlerConfigSO", menuName = "Scriptable Objects/BowlerConfigSO")]
public class BowlerConfigSO : ScriptableObject
{
    public float minSpeed = 10f;
    public float maxSpeed = 20f;
    public float minSpin = 0f;
    public float maxSpin = 10f;
    public float minSwing = 0f;
    public float maxSwing = 10f;
}
