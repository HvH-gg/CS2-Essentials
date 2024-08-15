using System.Runtime.CompilerServices;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Memory.DynamicFunctions;
using CounterStrikeSharp.API.Modules.Utils;
using CS2_CustomVotes.Shared.Models;
using hvhgg_essentials.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace hvhgg_essentials.Features;

public class TeleportFix
{
    private readonly Plugin _plugin;
    private readonly Dictionary<uint, float> _teleportBlockWarnings = new();
    public static readonly FakeConVar<bool> hvh_restrict_teleport = new("hvh_restrict_teleport", "Restricts players from teleporting/airstucking and crashing the server", true, ConVarFlags.FCVAR_REPLICATED);

    public TeleportFix(Plugin plugin)
    {
        _plugin = plugin;
        _plugin.RegisterFakeConVars(this);
        hvh_restrict_teleport.Value = _plugin.Config.RestrictTeleport;
    }
    
    public static void RegisterCustomVotes(Plugin plugin)
    {
        if (!plugin.Config.CustomVoteSettings.TeleportFixVote)
            return;
        
        var defaultOption = plugin.Config.RestrictTeleport ? "Block" : "Allow";
        
        Plugin.CustomVotesApi.Get()?.AddCustomVote(
            "teleport", 
            "Teleport/Airstuck", 
            defaultOption, 
            30,
            new Dictionary<string, VoteOption>
            {
                { "Allow", new("{Green}Allow", new List<string> { "hvh_restrict_teleport 0" })},
                { "Block", new("{Red}Block", new List<string> { "hvh_restrict_teleport 1" })},
            },
            plugin.Config.CustomVoteSettings.Style);
    }
    public static void UnregisterCustomVotes(Plugin plugin)
    {
        Plugin.CustomVotesApi.Get()?.RemoveCustomVote("teleport");
    }

    public HookResult RunCommand(DynamicHook h)
    {
        if (!hvh_restrict_teleport.Value)
            return HookResult.Continue;
        
        // check if the player is a valid player
        var player = h.GetParam<CCSPlayer_MovementServices>(0).Pawn.Value.Controller.Value?.As<CCSPlayerController>();
        if (!player.IsPlayer())
            return HookResult.Continue;
        
        // get the user command and view angles
        var userCmd = new CUserCmd(h.GetParam<IntPtr>(1));
        var viewAngles = userCmd.GetViewAngles();
        
        // no valid view angles or not infinite
        if (viewAngles is null || viewAngles.IsValid()) 
            return HookResult.Continue;
        
        // fix the view angles (prevents the player from using teleport or airstuck)
        viewAngles.Fix();

        // not warned yet or last warning was more than 3 seconds ago
        if (_teleportBlockWarnings.TryGetValue(player!.Index, out var lastWarningTime) &&
            !(lastWarningTime + 3 <= Server.CurrentTime)) 
            return HookResult.Changed;
        
        // print a warning to all players
        var feature = player.Pawn.Value!.As<CCSPlayerPawn>().OnGroundLastTick ? "teleport" : "airstuck";
        Server.PrintToChatAll($"{Helpers.FormatMessage(_plugin.Config.ChatPrefix)} Player {ChatColors.Red}{player.PlayerName}{ChatColors.Default} tried using {ChatColors.Red}{feature}{ChatColors.Default}!");
        _teleportBlockWarnings[player.Index] = Server.CurrentTime;

        return HookResult.Changed;
    }
}