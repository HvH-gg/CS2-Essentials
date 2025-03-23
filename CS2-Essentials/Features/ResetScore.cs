using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Cvars.Validators;
using CounterStrikeSharp.API.Modules.Utils;
using CSSharpUtils.Extensions;
using CSSharpUtils.Utils;

namespace hvhgg_essentials.Features;

public class ResetScore
{
    private readonly Plugin _plugin;
    public static readonly FakeConVar<bool> hvh_resetscore = new("hvh_resetscore", "Enables the reset score feature", true, ConVarFlags.FCVAR_REPLICATED);
    public static readonly FakeConVar<int> hvh_resetdeath = new("hvh_resetdeath_admin", "Enables the reset death feature for specific flags", 1, ConVarFlags.FCVAR_REPLICATED, new RangeValidator<int>(0, 2));
    private bool showResetScorePrint;
    private bool showResetDeathPrint;

    public ResetScore(Plugin plugin)
    {
        _plugin = plugin;
        _plugin.RegisterFakeConVars(this);
        hvh_resetscore.Value = _plugin.Config.AllowResetScore;
        showResetScorePrint = _plugin.Config.ShowResetScorePrint;
        hvh_resetdeath.Value = _plugin.Config.AllowResetDeath;
        showResetDeathPrint = _plugin.Config.ShowResetDeathPrint;
    }
    
    [ConsoleCommand("css_rs", "Reset score")]
    [ConsoleCommand("css_resetscore", "Reset score")]
    [ConsoleCommand("css_куыуеысщку", "Reset score")]
    [ConsoleCommand("css_кы", "Reset score")]
    public void OnResetScore(CCSPlayerController? player, CommandInfo inf)
    {
        if (!hvh_resetscore.Value)
            return;
        
        if (!player.IsPlayer()) 
            return;
        
        var stats = player!.ActionTrackingServices!.MatchStats;
        
        if (player is { Score: 0, MVPs: 0 } && 
            stats is { Kills: 0, HeadShotKills: 0, Deaths: 0, Assists: 0, UtilityDamage: 0, Damage: 0, Objective: 0 })
        {
            player.PrintToChat($"{ChatUtils.FormatMessage(_plugin.Config.ChatPrefix)} Your stats are already 0.");
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
        
        if(showResetScorePrint)
            Server.PrintToChatAll($"{ChatUtils.FormatMessage(_plugin.Config.ChatPrefix)} Player {ChatColors.Red}{player.PlayerName}{ChatColors.Default} has reset their stats!");
    }

    [ConsoleCommand("css_rd", "Reset death")]
    [ConsoleCommand("css_resetdeath", "Reset death")]
    [ConsoleCommand("css_кв", "Reset death")]
    [ConsoleCommand("css_куыуевуфер", "Reset death")]
    public void OnResetDeath(CCSPlayerController? player, CommandInfo inf)
    {
        if (hvh_resetdeath.Value == 0)
            return;

        if (!player.IsPlayer())
            return;

        if (hvh_resetdeath.Value == 1 && !AdminManager.PlayerHasPermissions(player, "@css/general"))
            return;

        var stats = player!.ActionTrackingServices!.MatchStats;

        if (stats.Deaths == 0)
        {
            player.PrintToChat($"{ChatUtils.FormatMessage(_plugin.Config.ChatPrefix)} Your deaths are already 0.");
            return;
        }

        stats.Deaths = 0;

        Utilities.SetStateChanged(player, "CCSPlayerController", "m_pActionTrackingServices");

        if (showResetDeathPrint)
            Server.PrintToChatAll($"{ChatUtils.FormatMessage(_plugin.Config.ChatPrefix)} Player {ChatColors.Red}{player.PlayerName}{ChatColors.Default} has reset their deaths!");
    }
}