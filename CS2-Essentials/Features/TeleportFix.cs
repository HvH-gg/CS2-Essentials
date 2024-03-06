using System.Runtime.CompilerServices;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Memory.DynamicFunctions;
using CounterStrikeSharp.API.Modules.Utils;
using hvhgg_essentials.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace hvhgg_essentials.Features;

public class TeleportFix
{
    private readonly Plugin _plugin;
    private readonly Dictionary<uint, float> _teleportBlockWarnings = new();

    public TeleportFix(Plugin plugin)
    {
        _plugin = plugin;
    }

    public HookResult RunCommand(DynamicHook h)
    {
        if (!_plugin.Config.RestrictTeleport)
            return HookResult.Continue;
        
        // check if the player is a valid player
        var player = h.GetParam<CCSPlayer_MovementServices>(0).Pawn.Value.Controller.Value!.As<CCSPlayerController>();
        if (!player.IsPlayer())
            return HookResult.Continue;
        
        // get the user command and view angles
        var userCmd = new CUserCmd(h.GetParam<IntPtr>(1));
        var viewAngles = userCmd.GetViewAngles();
        
        // no valid view angles or not infinite
        if (viewAngles is null || !viewAngles.IsInfinity()) 
            return HookResult.Continue;
        
        // fix the view angles (prevents the player from using teleport or airstuck)
        viewAngles.FixInfinity();

        // not warned yet or last warning was more than 3 seconds ago
        if (_teleportBlockWarnings.TryGetValue(player.Index, out var lastWarningTime) &&
            !(lastWarningTime + 3 <= Server.CurrentTime)) 
            return HookResult.Changed;
        
        // print a warning to all players
        var feature = player.Pawn.Value.As<CCSPlayerPawn>().OnGroundLastTick ? "teleport" : "airstuck";
        Server.PrintToChatAll($"{Helpers.FormatMessage(_plugin.Config.ChatPrefix)} Player {ChatColors.Red}{player.PlayerName}{ChatColors.Default} tried using {ChatColors.Red}{feature}{ChatColors.Default}!");
        _teleportBlockWarnings[player.Index] = Server.CurrentTime;

        return HookResult.Changed;
    }
}