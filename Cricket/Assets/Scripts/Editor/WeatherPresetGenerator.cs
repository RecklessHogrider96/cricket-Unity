using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

/// <summary>
/// Editor utility that programmatically creates all WeatherConfigSO preset assets.
///
/// Usage: Cricket → Generate Weather Presets
///
/// Individual preset assets go into Assets/Configurations/WeatherConfigurations/Weather/.
/// The WeatherRoster.asset is placed in Assets/Configurations/WeatherConfigurations/.
/// Re-running the tool overwrites any existing assets at those paths,
/// so you can regenerate safely after tweaking the values below.
/// </summary>
public static class WeatherPresetGenerator
{
    private const string RosterFolder  = "Assets/Configurations/WeatherConfigurations";
    private const string PresetsFolder = "Assets/Configurations/WeatherConfigurations/Weather";

    [MenuItem("Cricket/Generate Weather Presets")]
    public static void GenerateAll()
    {
        // Ensure folders exist
        if (!AssetDatabase.IsValidFolder("Assets/Configurations"))
            AssetDatabase.CreateFolder("Assets", "Configurations");
        if (!AssetDatabase.IsValidFolder(RosterFolder))
            AssetDatabase.CreateFolder("Assets/Configurations", "WeatherConfigurations");
        if (!AssetDatabase.IsValidFolder(PresetsFolder))
            AssetDatabase.CreateFolder(RosterFolder, "Weather");

        var all = new List<WeatherConfigSO>();

        // ── Neutral ──────────────────────────────────────────────────────────
        //                                name                  wX    wZ    wY  swing  bnc     frc    spin   roll
        all.Add(Make("Clear",                                    0f,   0f,  0f, 0.9f,  0.00f,  0.00f, 1.0f,  0.000f));
        all.Add(Make("Overcast",                                 0f,   0f,  0f, 1.4f,  0.00f,  0.00f, 1.0f,  0.000f));
        all.Add(Make("Heavy Overcast",                           0f,   0f,  0f, 1.8f,  0.00f,  0.00f, 1.0f,  0.000f));

        // ── Rain & Moisture ───────────────────────────────────────────────────
        all.Add(Make("Drizzle",                                  0f,   0f,  0f, 0.85f,-0.04f, -0.02f, 0.90f,-0.008f));
        all.Add(Make("Light Rain",                               0f,   0f,  0f, 0.65f,-0.10f, -0.06f, 0.75f,-0.015f));
        all.Add(Make("Heavy Rain (Stopped)",                     0f,   0f,  0f, 0.40f,-0.20f, -0.12f, 0.40f,-0.030f));

        // ── Dew ───────────────────────────────────────────────────────────────
        all.Add(Make("Morning Dew",                              0f,   0f,  0f, 0.75f,-0.02f, -0.02f, 0.80f,-0.015f));
        all.Add(Make("Dew (Day-Night)",                          0f,   0f,  0f, 0.60f,-0.02f, -0.03f, 0.65f,-0.025f));
        all.Add(Make("Heavy Dew",                                0f,   0f,  0f, 0.50f,-0.03f, -0.04f, 0.50f,-0.035f));

        // ── Heat & Hard Pitches ───────────────────────────────────────────────
        all.Add(Make("Scorching Heat",                           0f,   0f,  0f, 0.85f, 0.10f, -0.04f, 1.00f, 0.012f));
        all.Add(Make("Perth (Baked Pitch)",                      0f,   0f,  0f, 0.80f, 0.18f, -0.06f, 1.00f, 0.015f));
        all.Add(Make("Dusty Pitch (Day 4-5)",                    0f,   0f,  0f, 0.90f, 0.08f,  0.02f, 1.90f, 0.010f));

        // ── Wind — Crosswind ──────────────────────────────────────────────────
        all.Add(Make("Crosswind (Left)",                        -5f,   0f,  0f, 1.0f,  0.00f,  0.00f, 1.0f,  0.000f));
        all.Add(Make("Crosswind (Right)",                        5f,   0f,  0f, 1.0f,  0.00f,  0.00f, 1.0f,  0.000f));
        all.Add(Make("Strong Crosswind (Left)",                -10f,   0f,  0f, 1.0f,  0.00f,  0.00f, 1.0f,  0.000f));
        all.Add(Make("Strong Crosswind (Right)",                10f,   0f,  0f, 1.0f,  0.00f,  0.00f, 1.0f,  0.000f));

        // ── Wind — Headwind / Tailwind ────────────────────────────────────────
        all.Add(Make("Headwind",                                 0f,  -4f,  0f, 1.0f,  0.00f,  0.00f, 1.0f,  0.000f));
        all.Add(Make("Tailwind",                                 0f,   4f,  0f, 1.0f,  0.00f,  0.00f, 1.0f,  0.000f));

        // ── Combined Conditions ───────────────────────────────────────────────
        all.Add(Make("English Summer",                          -3f,   0f,  0f, 1.5f, -0.03f,  0.00f, 1.0f, -0.005f));
        all.Add(Make("Tropical Humid",                           0f,   0f, -1.5f,1.3f,-0.05f,  0.00f, 1.1f, -0.010f));
        all.Add(Make("Gusty Storm",                             -8f,  -5f,  1.5f,1.3f,-0.15f, -0.08f, 0.5f, -0.020f));

        // ── Build / update WeatherRoster ──────────────────────────────────────
        string rosterPath = $"{RosterFolder}/WeatherRoster.asset";
        WeatherRosterSO roster = AssetDatabase.LoadAssetAtPath<WeatherRosterSO>(rosterPath);
        if (roster == null)
        {
            roster = ScriptableObject.CreateInstance<WeatherRosterSO>();
            AssetDatabase.CreateAsset(roster, rosterPath);
        }

        roster.weathers = all;
        EditorUtility.SetDirty(roster);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"[WeatherPresetGenerator] Created {all.Count} weather presets in {PresetsFolder} " +
                  $"and WeatherRoster in {RosterFolder}");
    }

    // ── Helper ────────────────────────────────────────────────────────────────

    private static WeatherConfigSO Make(
        string  presetName,
        float   windX,
        float   windZ,
        float   windY,
        float   swingMultiplier,
        float   pitchBounceDelta,
        float   pitchFrictionDelta,
        float   spinGripMultiplier,
        float   outfieldRollingDelta)
    {
        // Sanitise filename: replace spaces and brackets with underscores
        string safeName = presetName
            .Replace(" ", "_")
            .Replace("(", "")
            .Replace(")", "")
            .Replace("-", "_");

        string assetPath = $"{PresetsFolder}/{safeName}.asset";

        // Load existing asset to overwrite, or create a new instance
        WeatherConfigSO preset = AssetDatabase.LoadAssetAtPath<WeatherConfigSO>(assetPath);
        if (preset == null)
        {
            preset = ScriptableObject.CreateInstance<WeatherConfigSO>();
            AssetDatabase.CreateAsset(preset, assetPath);
        }

        preset.weatherName           = presetName;
        preset.windX                 = windX;
        preset.windZ                 = windZ;
        preset.windY                 = windY;
        preset.swingMultiplier       = swingMultiplier;
        preset.pitchBounceDelta      = pitchBounceDelta;
        preset.pitchFrictionDelta    = pitchFrictionDelta;
        preset.spinGripMultiplier    = spinGripMultiplier;
        preset.outfieldRollingDelta  = outfieldRollingDelta;

        EditorUtility.SetDirty(preset);
        return preset;
    }
}
