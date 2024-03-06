using System.Text.Json;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;
using hvhgg_essentials.Enums;

namespace hvhgg_essentials.Features;

public class Misc
{
    private readonly Plugin _plugin;
    private readonly Dictionary<uint, float> _lastRulePrint = new();

    public Misc(Plugin plugin)
    {
        _plugin = plugin;
    }

    [ConsoleCommand("settings", "Print settings")]
    public void OnSettings(CCSPlayerController? player, CommandInfo inf)
    {
        AnnounceRules(player, true);
    }
    
    [ConsoleCommand("hvh_cfg_reload", "Reload the config in the current session without restarting the server")]
    [RequiresPermissions("@css/generic")]
    [CommandHelper(minArgs: 0, whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
    public void OnReloadConfigCommand(CCSPlayerController? player, CommandInfo info)
    {
        var config = File.ReadAllText(Plugin.CfgPath);
        try
        {
            _plugin.Config = JsonSerializer.Deserialize<Cs2EssentialsConfig>(config, new JsonSerializerOptions() { ReadCommentHandling = JsonCommentHandling.Skip })!;
            info.ReplyToCommand($"{(player == null ? "HvH.gg" : Helpers.FormatMessage(_plugin.Config.ChatPrefix))} Config reloaded successfully");
        }
        catch (Exception e)
        {
            info.ReplyToCommand($"{(player == null ? "HvH.gg" : Helpers.FormatMessage(_plugin.Config.ChatPrefix))} Failed to reload config: {e.Message}");
        }
    }
    
    public void AnnounceRules(CCSPlayerController? player, bool force = false)
    {
        if (!player.IsPlayer())
            return;

        if (_lastRulePrint.TryGetValue(player!.Pawn.Index, out var lastPrintTime) &&
            lastPrintTime + 600 > Server.CurrentTime && !force)
            return;

        _lastRulePrint[player.Pawn.Index] = Server.CurrentTime;

        if (_plugin.Config.AllowSettingsPrint)
        {
            player.PrintToChat("Ruleset:");
            player.PrintToChat(
                $"unmatched.{ChatColors.Orange}gg{ChatColors.Default} friendly fire: {(_plugin.Config.UnmatchedFriendlyFire ? $"{ChatColors.Lime}enabled" : $"{ChatColors.Red}disabled")}");
            player.PrintToChat(
                $"Teleport/Airstuck: {(!_plugin.Config.RestrictTeleport ? $"{ChatColors.Lime}enabled" : $"{ChatColors.Red}disabled")}");
            player.PrintToChat(
                $"Rapid fire: {(_plugin.Config.RapidFireFixMethod == FixMethod.Allow ? $"{ChatColors.Lime}enabled" : $"{ChatColors.Red}disabled")}");

            switch (_plugin.Config.RapidFireFixMethod)
            {
                case FixMethod.Allow:
                    break;
                case FixMethod.Ignore:
                    player.PrintToChat($"Method: {ChatColors.Red}block damage");
                    break;
                case FixMethod.Reflect:
                    player.PrintToChat(
                        $"Method: {ChatColors.Red}reflect damage{ChatColors.Default} at {ChatColors.Orange}{_plugin.Config.RapidFireReflectScale}x{ChatColors.Default}");
                    break;
                case FixMethod.ReflectSafe:
                    player.PrintToChat(
                        $"Method: {ChatColors.Red}reflect damage{ChatColors.Default} at {ChatColors.Orange}{_plugin.Config.RapidFireReflectScale}x{ChatColors.Default} and prevent death");
                    break;
                default:
                    break;
            }

            player.PrintToChat(" ");
            player.PrintToChat("Weapon restriction:");
            if (_plugin.Config.AllowedAwpCount != -1)
                player.PrintToChat(
                    $"AWP: {(_plugin.Config.AllowedAwpCount == 0 ? ChatColors.Red : ChatColors.Orange)}{_plugin.Config.AllowedAwpCount} per team");
            if (_plugin.Config.AllowedScoutCount != -1)
                player.PrintToChat(
                    $"Scout: {(_plugin.Config.AllowedScoutCount == 0 ? ChatColors.Red : ChatColors.Orange)}{_plugin.Config.AllowedScoutCount} per team");
            if (_plugin.Config.AllowedAutoSniperCount != -1)
                player.PrintToChat(
                    $"Auto: {(_plugin.Config.AllowedAutoSniperCount == 0 ? ChatColors.Red : ChatColors.Orange)}{_plugin.Config.AllowedAutoSniperCount} per team");

            player.PrintToChat(" ");
            player.PrintToChat(
                $"{Helpers.FormatMessage(_plugin.Config.ChatPrefix)} Type {ChatColors.Red}!settings{ChatColors.Default} to see these settings again");
        }
        
        if (_plugin.Config.AllowAdPrint)
            player.PrintToChat(Helpers.FormatMessage("powered by {HvHgg}"));
        
    }
}