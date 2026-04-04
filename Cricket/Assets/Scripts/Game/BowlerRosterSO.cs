using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The full squad of bowlers available in the game.
/// Add one BowlerConfigSO per named bowler (e.g. Roshan, Malik, Singh).
/// CricketDataController holds a reference to this; the HUD reads it
/// to populate the bowler selection dropdown automatically.
/// </summary>
[CreateAssetMenu(fileName = "BowlerRoster", menuName = "Scriptable Objects/BowlerRoster")]
public class BowlerRosterSO : ScriptableObject
{
    public List<BowlerConfigSO> bowlers = new List<BowlerConfigSO>();
}
