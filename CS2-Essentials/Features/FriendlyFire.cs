using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Memory.DynamicFunctions;
using CS2_CustomVotes.Shared.Models;

namespace hvhgg_essentials.Features;

public class FriendlyFire
{
    private readonly Plugin _plugin;
    public static readonly FakeConVar<bool> hvh_unmatched_friendlyfire = new("hvh_unmatched_friendlyfire", "Enables unmatched.gg style friendly fire (utility only)", true, ConVarFlags.FCVAR_REPLICATED);

    public FriendlyFire(Plugin plugin)
    {
        _plugin = plugin;
        _plugin.RegisterFakeConVars(this);
        hvh_unmatched_friendlyfire.Value = _plugin.Config.UnmatchedFriendlyFire;
    }
    
    private static readonly string[] AllowedDamageInflicter =
    {
        "inferno", "hegrenade_projectile", "flashbang_projectile", "smokegrenade_projectile", "decoy_projectile",
        "planted_c4"
    };
    
    public static void RegisterCustomVotes(Plugin plugin)
    {
        if (!plugin.Config.CustomVoteSettings.FriendlyFireVote)
            return;
        
        var defaultOption = plugin.Config.UnmatchedFriendlyFire ? "Enable" : "Disable";
        
        Plugin.CustomVotesApi.Get()?.AddCustomVote(
            "friendlyfire",
            new List<string> {
                "ff"  
            },
            "unmatched.gg friendly fire", 
            defaultOption, 
            30,
            new Dictionary<string, VoteOption>
            {
                { "Enable", new("{Green}Enable", new List<string> { "hvh_unmatched_friendlyfire 0" })},
                { "Disable", new("{Red}Disable", new List<string> { "hvh_unmatched_friendlyfire 1" })},
            },
            plugin.Config.CustomVoteSettings.Style);
    }
    public static void UnregisterCustomVotes(Plugin plugin)
    {
        Plugin.CustomVotesApi.Get()?.RemoveCustomVote("friendlyfire");
    }

    public HookResult OnTakeDamage(DynamicHook hook)
    {
        if (!_plugin.Config.UnmatchedFriendlyFire)
            return HookResult.Continue;

        try
        {
            var victim = hook.GetParam<CEntityInstance>(0);
            var damageInfo = hook.GetParam<CTakeDamageInfo>(1);
        
            // attacker is null or invalid
            if (damageInfo.Attacker.Value == null || !damageInfo.Attacker.Value.IsValid)
                return HookResult.Continue;
        
            var attackerPlayer = new CCSPlayerPawn(damageInfo.Attacker.Value.Handle);
            var victimPlayer = new CCSPlayerController(victim.Handle);
        
            // attacker or victim is invalid
            if (!attackerPlayer.IsValid || !victimPlayer.IsValid)
                return HookResult.Continue;

            // attacker and victim are on the same team or victim is not a player
            if (attackerPlayer.TeamNum != victimPlayer.TeamNum || victim.DesignerName != "player")
                return HookResult.Continue;
            
            // allow damage from certain inflicter types
            var inflicter = damageInfo.Inflictor.Value?.DesignerName ?? "";
            if (AllowedDamageInflicter.Contains(inflicter))
                return HookResult.Continue;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return HookResult.Continue;
        }
        
        Console.WriteLine("[HvH.gg] Blocked friendly fire");
        
        // otherwise block the damage
        return HookResult.Handled;
    }
}