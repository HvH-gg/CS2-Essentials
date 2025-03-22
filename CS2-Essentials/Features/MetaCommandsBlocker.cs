using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Cvars;

namespace hvhgg_essentials.Features;

public class MetaCommandsBlocker
{
    private readonly Plugin _plugin;
    public static readonly FakeConVar<bool> hvh_restrict_meta_commands = new("hvh_restrict_meta_commands", "Restricts players from getting result for meta/cssharp commands", true, ConVarFlags.FCVAR_REPLICATED);

    public MetaCommandsBlocker(Plugin plugin)
    {
        _plugin = plugin;
        _plugin.RegisterFakeConVars(this);
        hvh_restrict_meta_commands.Value = _plugin.Config.RestrictMetaCommands;
    }

    public HookResult CommandListener_BlockOutput(CCSPlayerController? player, CommandInfo info)
    {
        // If settet to false then allow to check meta and cssharp commands
        if (!hvh_restrict_meta_commands.Value)
            return HookResult.Continue;

        // If the CCSPlayerController is null then execute this section
        if (player == null)
        {
            return HookResult.Continue;
        }

        // If the the CCSPlayerController entity is invalid by pointing to a null pointer then execute this section
        if (!player.IsValid)
        {
            return HookResult.Continue;
        }

        // If the pawn associated with the CCSPlayerController is invalid then execute this section
        if (!player.PlayerPawn.IsValid)
        {
            return HookResult.Continue;
        }

        // If the player has root administrator permissions then execute this section
        if (AdminManager.PlayerHasPermissions(player, "@css/root"))
        {
            return HookResult.Continue;
        }

        player.PrintToChat("Access denied.");

        return HookResult.Stop;
    }
}
