using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Cvars.Validators;
using CounterStrikeSharp.API.Modules.Entities.Constants;
using CounterStrikeSharp.API.Modules.Memory.DynamicFunctions;
using CounterStrikeSharp.API.Modules.Utils;
using CSSharpUtils.Extensions;
using CSSharpUtils.Utils;

namespace hvhgg_essentials.Features;

public class WeaponRestrict
{
    private readonly Plugin _plugin;
    private readonly Dictionary<string, Tuple<ItemDefinition, int>> _weaponPrices = new()
    {
        { "weapon_ssg08", new Tuple<ItemDefinition, int>(ItemDefinition.SSG_08, 1700) },
        { "weapon_awp", new Tuple<ItemDefinition, int>(ItemDefinition.AWP, 4750) },
        { "weapon_scar20", new Tuple<ItemDefinition, int>(ItemDefinition.SCAR_20, 5000) },
        { "weapon_g3sg1", new Tuple<ItemDefinition, int>(ItemDefinition.G3SG1, 5000) },
    };
    private readonly Dictionary<uint, float> _lastWeaponRestrictPrint = new();
    public static readonly FakeConVar<int> hvh_restrict_awp = new("hvh_restrict_awp", "Restrict awp to X per team", -1, ConVarFlags.FCVAR_REPLICATED, new RangeValidator<int>(-1, int.MaxValue));
    public static readonly FakeConVar<int> hvh_restrict_scout = new("hvh_restrict_scout", "Restrict scout to X per team", -1, ConVarFlags.FCVAR_REPLICATED, new RangeValidator<int>(-1, int.MaxValue));
    public static readonly FakeConVar<int> hvh_restrict_auto = new("hvh_restrict_auto", "Restrict autosniper to X per team", -1, ConVarFlags.FCVAR_REPLICATED, new RangeValidator<int>(-1, int.MaxValue));

    public WeaponRestrict(Plugin plugin)
    {
        _plugin = plugin;
        _plugin.RegisterFakeConVars(this);
        hvh_restrict_awp.Value = _plugin.Config.AllowedAwpCount;
        hvh_restrict_scout.Value = _plugin.Config.AllowedScoutCount;
        hvh_restrict_auto.Value = _plugin.Config.AllowedAutoSniperCount;
    }
    
    public HookResult OnWeaponCanUse(DynamicHook hook)
    {
        var weaponServices = hook.GetParam<CCSPlayer_WeaponServices>(0);
        var weapon = hook.GetParam<CBasePlayerWeapon>(1);

        var player = new CCSPlayerController(weaponServices.Pawn.Value.Controller.Value!.Handle);

        // not a player
        if (!player.IsPlayer())
            return HookResult.Continue;

        var item = weapon.DesignerName;
        
        // not a weapon we want to restrict
        if (!_weaponPrices.ContainsKey(item))
            return HookResult.Continue;

        // not exceeding limits
        var weaponsInTeam = GetWeaponCountInTeam(item, player.Team);
        var allowedWeapons = GetAllowedWeaponCount(item);

        var willExceedLimits = allowedWeapons != -1 && weaponsInTeam + 1 > allowedWeapons;

        if (!willExceedLimits)
            return HookResult.Continue;
        
        // skip warning if we already warned this player in the last 10 seconds
        if (!_lastWeaponRestrictPrint.TryGetValue(player.Pawn.Index, out var lastPrintTime) ||
            lastPrintTime + 10 <= Server.CurrentTime)
        {
            player.PrintToChat($"{ChatUtils.FormatMessage(_plugin.Config.ChatPrefix)} {ChatColors.Red}{item}{ChatColors.Default} is restricted to {ChatColors.Red}{allowedWeapons}{ChatColors.Default} per team!");
            
            _lastWeaponRestrictPrint[player.Pawn.Index] = Server.CurrentTime;
        }
        
        // weapon was created this tick (aka purchased and not picked up)
        if (Math.Abs(weapon.CreateTime - Server.CurrentTime) < 0.01f)
        {
            CCSWeaponBaseGun weaponBaseGun = new(weapon.Handle);
            weaponBaseGun.Remove();
        }
        
        hook.SetReturn(false);
        return HookResult.Handled;
    }
    
    public HookResult OnItemPurchase(EventItemPurchase eventItemPurchase, GameEventInfo info)
    {
        var player = eventItemPurchase.Userid;

        // not a player
        if (!player.IsPlayer())
            return HookResult.Continue;

        // get weapon name
        var item = eventItemPurchase.Weapon;

        // not a weapon we want to restrict
        if (!_weaponPrices.ContainsKey(item))
            return HookResult.Continue;

        // not exceeding limits
        var weaponsInTeam = GetWeaponCountInTeam(item, player!.Team);
        var allowedWeapons = GetAllowedWeaponCount(item);

        var willExceedLimits = allowedWeapons != -1 && weaponsInTeam + 1 > allowedWeapons;

        Console.WriteLine(weaponsInTeam);
        Console.WriteLine(allowedWeapons);
        
        if (!willExceedLimits)
            return HookResult.Continue;

        player.PrintToChat($"{ChatUtils.FormatMessage(_plugin.Config.ChatPrefix)} {ChatColors.Red}{item}{ChatColors.Default} is restricted to {ChatColors.Red}{allowedWeapons}{ChatColors.Default} per team!");

        RefundItem(player, item);

        return HookResult.Continue;
    }
    
    private void RefundItem(CCSPlayerController player, string item)
    {
        var moneyServices = player.InGameMoneyServices!;
        moneyServices.Account += _weaponPrices[item].Item2;
        Console.WriteLine($"[HvH.gg] Refunding {item} for {_weaponPrices[item].Item2}");
    }
    
    private int GetAllowedWeaponCount(string item)
    {
        return item switch
        {
            "weapon_awp" => hvh_restrict_awp.Value,
            "weapon_ssg08" => hvh_restrict_scout.Value,
            "weapon_scar20" or "weapon_g3sg1" => hvh_restrict_auto.Value,
            _ => -1
        };
    }

    private int GetWeaponCountInTeam(string item, CsTeam team)
    {
        // get all active players in team
        var activePlayers = Utilities.GetPlayers()
            .Where(pl => pl is { IsValid: true, PlayerPawn.IsValid: true, PlayerPawn.Value.IsValid: true } &&
                         pl.TeamNum == (byte)team).ToList();

        // get all weapons in team
        var weaponCount = activePlayers
            .Select(player => player.PlayerPawn.Value!.WeaponServices!.MyWeapons) // get all weapons of player
            .Select(playerWeapons => playerWeapons
                .Where(weapon => weapon.IsValid && weapon.Value!.IsValid) // filter out invalid weapons
                .Count(weapon => weapon.Value!.AttributeManager.Item.ItemDefinitionIndex ==
                                 (ushort)_weaponPrices[item].Item1)) // count weapons of type
            .Sum(); // sum up all weapons of type

        return weaponCount;
    }
}