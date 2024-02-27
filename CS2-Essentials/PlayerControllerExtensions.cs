using System.Globalization;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;

namespace hvhgg_essentials;

public static class PlayerControllerExtensions
{
    internal static void Kick(this CCSPlayerController? playerController, string reason)
    {
        Console.WriteLine("[HvH.gg] Kicking player");

        if (!playerController.IsPlayer())
            return;

        var kickCommand = string.Create(CultureInfo.InvariantCulture,
            $"kickid {playerController!.UserId!.Value} \"{reason}\"");
        
        Console.WriteLine(kickCommand);
        
        Server.ExecuteCommand(kickCommand);
    }
    
    internal static void MoveToTeam(this CCSPlayerController? playerController, CsTeam team)
    {
        if (!playerController.IsPlayer() || playerController!.TeamNum == (byte)team)
            return;

        Console.WriteLine("[HvH.gg] Moving player from {0} to {1}", playerController.TeamNum, (int)team);
        
        playerController.ChangeTeam(team);
    }

    internal static bool IsPlayer(this CCSPlayerController? player)
    {
        return player is { IsValid: true, IsHLTV: false, IsBot: false, UserId: not null, SteamID: >0 };
    }
}