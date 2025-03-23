using System.Text.Json.Serialization;
using CounterStrikeSharp.API.Core;
using hvhgg_essentials.Enums;

namespace hvhgg_essentials;

public class Cs2CustomVoteSettings
{
    [JsonPropertyName("FriendlyFireVote")] public bool FriendlyFireVote { get; set; } = false;
    [JsonPropertyName("TeleportFixVote")] public bool TeleportFixVote { get; set; } = false;
    [JsonPropertyName("RapidFireVote")] public string RapidFireVote { get; set; } = "full";
    [JsonPropertyName("Style")] public string Style { get; set; } = "center";
}

public partial class Cs2EssentialsConfig : BasePluginConfig
{
    [JsonPropertyName("RapidFireFixMethod")] public FixMethod RapidFireFixMethod { get; set; } = FixMethod.Ignore;
    [JsonPropertyName("RapidFirePrintMessage")] public bool RapidFirePrintMessage { get; set; } = true;
    [JsonPropertyName("RapidFireReflectScale")] public float RapidFireReflectScale { get; set; } = 1f;
    [JsonPropertyName("AllowedAwpCount")] public int AllowedAwpCount { get; set; } = -1;
    [JsonPropertyName("AllowedScoutCount")] public int AllowedScoutCount { get; set; } = -1;
    [JsonPropertyName("AllowedAutoSniperCount")] public int AllowedAutoSniperCount { get; set; } = -1;
    [JsonPropertyName("AllowAllWeaponsOnWarmup")] public bool AllowAllWeaponsOnWarmup { get; set; } = true;
    [JsonPropertyName("AllowWeaponsForFlag")] public string AllowWeaponsForFlag { get; set; } = "@css/restrict";
    [JsonPropertyName("UnmatchedFriendlyFire")] public bool UnmatchedFriendlyFire { get; set; } = true;
    [JsonPropertyName("RestrictTeleport")] public bool RestrictTeleport { get; set; } = true;
    [JsonPropertyName("TeleportPrintMessage")] public bool TeleportPrintMessage { get; set; } = true;
    [JsonPropertyName("AllowAdPrint")] public bool AllowAdPrint { get; set; } = true;
    [JsonPropertyName("AllowSettingsPrint")] public bool AllowSettingsPrint { get; set; } = true;
    [JsonPropertyName("AllowResetScore")] public bool AllowResetScore { get; set; } = true;
    [JsonPropertyName("ShowResetScorePrint")] public bool ShowResetScorePrint { get; set; } = true;
    [JsonPropertyName("AllowResetDeath")] public int AllowResetDeath { get; set; } = 1;
    [JsonPropertyName("ShowResetDeathPrint")] public bool ShowResetDeathPrint { get; set; } = true;
    [JsonPropertyName("RestrictMetaCommands")] public bool RestrictMetaCommands { get; set; } = true;
    [JsonPropertyName("AllowRageQuit")] public bool AllowRageQuit { get; set; } = true;
    [JsonPropertyName("ChatPrefix")] public string ChatPrefix { get; set; } = "[{Red}Hv{DarkRed}H{Default}.gg]";
    [JsonPropertyName("CustomVoteSettings")] public Cs2CustomVoteSettings CustomVoteSettings { get; set; } = new();
    [JsonPropertyName("ConfigVersion")] public override int Version { get; set; } = 6;
}