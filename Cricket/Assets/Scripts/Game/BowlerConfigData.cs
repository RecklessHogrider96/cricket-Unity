using System;
using System.Collections.Generic;
using UnityEngine;

public enum BowlerBowlingArm
{
    Left,
    Right
}

public enum BowlerType
{
    Fast,
    Medium,
    Spin
}

[Serializable]
public class BowlerConfig
{
    public BowlerType bowlerType;
    public BowlerBowlingArm bowlerBowlingArm;
    public BowlerConfigSO bowlerConfigSO;
}

[CreateAssetMenu(fileName = "BowlerConfigData", menuName = "Scriptable Objects/BowlerConfigData")]
public class BowlerConfigData : ScriptableObject
{
    public List<BowlerConfig> bowlerConfigs;
}
