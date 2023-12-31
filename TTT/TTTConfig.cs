using System.Text.Json.Serialization;
using CounterStrikeSharp.API.Core;

namespace CS2_TTT;

public class TTTConfig : BasePluginConfig
{
    [JsonPropertyName("debug_mode")] public bool DebugMode { get; set; } = false;  // TODO: Switch to ConVar when supported
    [JsonPropertyName("grace_time")] public float GraceTime { get; set; } = 15;
    [JsonPropertyName("traitor_ratio")] public int TraitorRatio { get; set; } = 0;
    [JsonPropertyName("detective_ratio")] public int DetectiveRatio { get; set; } = 0;
    [JsonPropertyName("block_suicide")] public bool BlockSuicide { get; set; } = false;
    [JsonPropertyName("starting_secondary")] public string StartingSecondary { get; set; } = "weapon_glock";
    [JsonPropertyName("starting_primary")] public string StartingPrimary { get; set; } = "";
}