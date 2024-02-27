using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;

namespace hvhgg_essentials.Features;

public class RageQuit
{
    private readonly Plugin _plugin;

    public RageQuit(Plugin plugin)
    {
        _plugin = plugin;
    }
    
    [ConsoleCommand("rq", "Rage quit")]
    [ConsoleCommand("ragequit", "Rage quit")]
    public void OnRageQuit(CCSPlayerController? player, CommandInfo inf)
    {
        if (!_plugin.Config.AllowRageQuit)
            return;
        
        if (!player.IsPlayer()) 
            return;
        
        player!.Kick("Rage quit");
        
        Server.PrintToChatAll($"{Helpers.FormatMessage(_plugin.Config.ChatPrefix)} Player {ChatColors.Red}{player!.PlayerName}{ChatColors.Default} has rage quit!");
    }
}