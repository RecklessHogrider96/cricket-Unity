using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The full list of weather presets available in the game.
/// Add one WeatherConfigSO per condition (e.g. Clear, Overcast, Gusty Storm).
/// CricketDataController holds a reference to this; the HUD reads it
/// to populate the weather selection dropdown automatically.
///
/// Run Cricket → Generate Weather Presets to auto-create all presets
/// and a pre-populated WeatherRoster asset in Assets/Configurations/WeatherConfigurations/.
/// </summary>
[CreateAssetMenu(fileName = "WeatherRoster", menuName = "Scriptable Objects/WeatherRoster")]
public class WeatherRosterSO : ScriptableObject
{
    public List<WeatherConfigSO> weathers = new List<WeatherConfigSO>();
}
