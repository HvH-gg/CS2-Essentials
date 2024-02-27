using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Memory.DynamicFunctions;
using CounterStrikeSharp.API.Modules.Utils;
using hvhgg_essentials.Enums;

namespace hvhgg_essentials.Features;

public class RapidFire
{
    private readonly Dictionary<uint, int> _lastPlayerShotTick = new();
    private readonly HashSet<uint> _rapidFireBlockUserIds = new();
    private readonly Dictionary<uint, float> _rapidFireBlockWarnings = new();

    private readonly Plugin _plugin;

    public RapidFire(Plugin plugin)
    {
        _plugin = plugin;
    }

    public HookResult OnWeaponFire(EventWeaponFire eventWeaponFire, GameEventInfo info)
    {
        if (!eventWeaponFire.Userid.IsPlayer())
            return HookResult.Continue;
        
        var nextPrimaryAttackTick = eventWeaponFire.Userid.Pawn.Value?.WeaponServices?.ActiveWeapon.Value?.NextPrimaryAttackTick ?? 0;
        var index = eventWeaponFire.Userid.Pawn.Index;
            
        if (!_lastPlayerShotTick.TryGetValue(index, out var lastShotTick))
        {
            _lastPlayerShotTick[index] = Server.TickCount;
            return HookResult.Continue;
        }
            
        _lastPlayerShotTick[index] = Server.TickCount;

        // this is ghetto but should work for now
        if (nextPrimaryAttackTick > lastShotTick)
            return HookResult.Continue;

        // no chat message if we allow rapid fire
        if (_plugin.Config.RapidFireFixMethod == FixMethod.Allow)
            return HookResult.Continue;
            
        Console.WriteLine($"[HvH.gg] Detected rapid fire from {eventWeaponFire.Userid.PlayerName}");
            
        // clear list every frame (in case of misses)
        if (_rapidFireBlockUserIds.Count == 0)
            Server.NextFrame(_rapidFireBlockUserIds.Clear);
            
        _rapidFireBlockUserIds.Add(index);
            
        // skip warning if we already warned this player in the last 3 seconds
        if (_rapidFireBlockWarnings.TryGetValue(index, out var lastWarningTime) &&
            lastWarningTime + 3 > Server.CurrentTime) 
            return HookResult.Continue;
            
        // warn player
        Server.PrintToChatAll($"{Helpers.FormatMessage(_plugin.Config.ChatPrefix)} Player {ChatColors.Red}{eventWeaponFire.Userid.PlayerName}{ChatColors.Default} tried using {ChatColors.Red}rapid fire{ChatColors.Default}!");
        _rapidFireBlockWarnings[index] = Server.CurrentTime;

        return HookResult.Continue;
    }
    
    public HookResult OnTakeDamage(DynamicHook h)
    {
        var damageInfo = h.GetParam<CTakeDamageInfo>(1);

        // attacker is invalid
        if (damageInfo.Attacker.Value == null)
            return HookResult.Continue;

        // attacker is not in the list
        if (!_rapidFireBlockUserIds.Contains(damageInfo.Attacker.Index))
            return HookResult.Continue;
            
        // set damage according to config
        switch (_plugin.Config.RapidFireFixMethod)
        {
            case FixMethod.Allow:
                break;
            case FixMethod.Ignore:
                damageInfo.Damage = 0;
                break;
            case FixMethod.Reflect:
            case FixMethod.ReflectSafe:
                damageInfo.Damage *= _plugin.Config.RapidFireReflectScale;
                h.SetParam<CEntityInstance>(0, damageInfo.Attacker.Value);
                if (_plugin.Config.RapidFireFixMethod == FixMethod.ReflectSafe)
                    damageInfo.DamageFlags |= TakeDamageFlags_t.DFLAG_PREVENT_DEATH; //https://docs.cssharp.dev/api/CounterStrikeSharp.API.Core.TakeDamageFlags_t.html
                break;
            default:
                break;
        }

        return HookResult.Changed;
    }
}