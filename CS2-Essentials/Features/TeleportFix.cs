using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Modules.Utils;

namespace hvhgg_essentials.Features;

public class TeleportFix
{
    private readonly Plugin _plugin;
    private static readonly Dictionary<uint, Tuple<Vector, QAngle>> PreviousPosition = new();
    private readonly Dictionary<uint, float> _teleportBlockWarnings = new();

    public TeleportFix(Plugin plugin)
    {
        _plugin = plugin;
    }

    public void OnTick()
    {
        if (!_plugin.Config.RestrictTeleport)
            return;
        
        foreach (var player in Utilities.GetPlayers().Where(p => p.IsPlayer() && p.Pawn.Value != null))
        {
            var origin = player.Pawn.Value?.AbsOrigin;
            var rotation = player.Pawn.Value?.AbsRotation;
                
            if (origin is null)
                continue;

            if (origin.X != 0 && origin.Y != 0)
            {
                PreviousPosition[player.Pawn.Index] = new Tuple<Vector, QAngle>(new Vector(origin.X, origin.Y, origin.Z), new QAngle(rotation?.X, rotation?.Y, rotation?.Z));
                continue;
            }
            
            Console.WriteLine($"[HvH.gg] Detected teleport from {player.PlayerName}");

            // not warned yet or last warning was more than 3 seconds ago
            if (!_teleportBlockWarnings.TryGetValue(player.Index, out var lastWarningTime) ||
                lastWarningTime + 3 <= Server.CurrentTime)
            {
                Server.PrintToChatAll($"{Helpers.FormatMessage(_plugin.Config.ChatPrefix)} Player {ChatColors.Red}{player.PlayerName}{ChatColors.Default} tried using {ChatColors.Red}teleport{ChatColors.Default}!");
                _teleportBlockWarnings[player.Index] = Server.CurrentTime;
            }

            if (!player.Pawn.IsValid || !PreviousPosition.TryGetValue(player.Pawn.Index, out var previousPosition)) 
                continue;
            
            Console.WriteLine($"[HvH.gg] Teleporting {player.PlayerName} back to {previousPosition}");
            player.Pawn.Value?.Teleport(previousPosition.Item1, previousPosition.Item2, player.Pawn.Value?.AbsVelocity ?? new Vector());
        }
    }
}