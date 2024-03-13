using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Cvars.Validators;
using CounterStrikeSharp.API.Modules.Utils;

namespace hvhgg_essentials.Features;

public class RageQuit
{
    private readonly Plugin _plugin;
    public static readonly FakeConVar<bool> hvh_ragequit = new("hvh_ragequit", "Enables the rage quit feature", true, ConVarFlags.FCVAR_REPLICATED);

    public RageQuit(Plugin plugin)
    {
        _plugin = plugin;
        _plugin.RegisterFakeConVars(this);
        hvh_ragequit.Value = _plugin.Config.AllowRageQuit;
    }
    
    [ConsoleCommand("css_rq", "Rage quit")]
    [ConsoleCommand("css_ragequit", "Rage quit")]
    public void OnRageQuit(CCSPlayerController? player, CommandInfo inf)
    {
        if (!hvh_ragequit.Value)
            return;
        
        if (!player.IsPlayer()) 
            return;
        
        player!.Kick("Rage quit");
        
        Server.PrintToChatAll($"{Helpers.FormatMessage(_plugin.Config.ChatPrefix)} Player {ChatColors.Red}{player!.PlayerName}{ChatColors.Default} has rage quit!");
    }
}