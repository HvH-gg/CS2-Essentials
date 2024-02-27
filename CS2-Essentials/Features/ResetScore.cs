using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;

namespace hvhgg_essentials.Features;

public class ResetScore
{
    private readonly Plugin _plugin;

    public ResetScore(Plugin plugin)
    {
        _plugin = plugin;
    }
    
    [ConsoleCommand("rs", "Reset score")]
    [ConsoleCommand("resetscore", "Reset score")]
    public void OnResetScore(CCSPlayerController? player, CommandInfo inf)
    {
        if (!_plugin.Config.AllowResetScore)
            return;
        
        if (!player.IsPlayer()) 
            return;
        
        var stats = player!.ActionTrackingServices!.MatchStats;
        
        if (player is { Score: 0, MVPs: 0 } && 
            stats is { Kills: 0, HeadShotKills: 0, Deaths: 0, Assists: 0, UtilityDamage: 0, Damage: 0, Objective: 0 })
        {
            player.PrintToChat($"{Helpers.FormatMessage(_plugin.Config.ChatPrefix)} Your stats are already 0.");
            return;
        }
        
        stats.Kills = 0;
        stats.HeadShotKills = 0;
        stats.Deaths = 0;
        stats.Assists = 0;
        stats.UtilityDamage = 0;
        stats.Damage = 0;
        stats.Objective = 0;
        player.MVPs = 0;
        player.Score = 0;
        
        Utilities.SetStateChanged(player, "CCSPlayerController", "m_pActionTrackingServices");
        Utilities.SetStateChanged(player, "CCSPlayerController", "m_iMVPs");
        Utilities.SetStateChanged(player, "CCSPlayerController", "m_iScore");
        
        Server.PrintToChatAll($"{Helpers.FormatMessage(_plugin.Config.ChatPrefix)} Player {ChatColors.Red}{player.PlayerName}{ChatColors.Default} has reset their stats!");
    }
}