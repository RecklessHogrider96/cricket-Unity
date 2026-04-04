// DEPRECATED — replaced by BowlerRosterSO + individual named BowlerConfigSO assets.
// BowlerConfigData mapped generic (BowlerType + Arm) pairs to SOs.
// The named roster system gives each bowler their own SO with full per-bowler stats.
// This file is kept so existing .asset references do not break.
// Do not use BowlerConfigData or BowlerConfig in new code.
using System;
using System.Collections.Generic;
using UnityEngine;

public enum BowlerBowlingArm
{
    Left,
    Right
}

/// <summary>
/// Whether a bowler delivers with a fixed arm or can bowl with either.
/// Left / Right → arm is fixed; the HUD hides the arm selector and the model
/// auto-sets BowlerBowlingArm when that bowler is chosen.
/// Both → arm selector is shown in the HUD so the user can choose per delivery.
/// </summary>
public enum BowlerArmPreference
{
    Left,
    Right,
    Both
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
