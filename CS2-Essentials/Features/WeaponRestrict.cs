using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Cvars.Validators;
using CounterStrikeSharp.API.Modules.Entities.Constants;
using CounterStrikeSharp.API.Modules.Memory.DynamicFunctions;
using CounterStrikeSharp.API.Modules.Admin;
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
    public static readonly FakeConVar<string> hvh_bypass_weapon_restrict_flag = new("hvh_bypass_weapon_restrict_flag", "Permission flag to bypass weapon restrictions", "");

    public WeaponRestrict(Plugin plugin)
    {
        _plugin = plugin;
        _plugin.RegisterFakeConVars(this);
        hvh_restrict_awp.Value = _plugin.Config.AllowedAwpCount;
        hvh_restrict_scout.Value = _plugin.Config.AllowedScoutCount;
        hvh_restrict_auto.Value = _plugin.Config.AllowedAutoSniperCount;
        hvh_bypass_weapon_restrict_flag.Value = _plugin.Config.WeaponRestrictBypassFlags;
    }
    
    public HookResult OnWeaponCanAcquire(DynamicHook hook)
    {
        var itemServices = hook.GetParam<CCSPlayer_ItemServices>(0);
        var econItemView = hook.GetParam<CEconItemView>(1);
        var acquireMethod = hook.GetParam<AcquireMethod>(2);

        // find weapon name from item definition index
        var itemDefIndex = econItemView.ItemDefinitionIndex;
        var item = _weaponPrices.FirstOrDefault(kv => (ushort)kv.Value.Item1 == itemDefIndex).Key;

        // not a weapon we want to restrict
        if (item == null)
            return HookResult.Continue;

        var player = itemServices.Pawn.Value.Controller.Value!.As<CCSPlayerController>();

        // not a player
        if (!player.IsPlayer())
            return HookResult.Continue;

        // player has bypass permission
        if (hvh_bypass_weapon_restrict_flag.Value != "" &&
            hvh_bypass_weapon_restrict_flag.Value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Any(flag => AdminManager.PlayerHasPermissions(player, flag)))
            return HookResult.Continue;

        var weaponsInTeam = GetWeaponCountInTeam(item, player.Team);
        var allowedWeapons = GetAllowedWeaponCount(item);
        var willExceedLimits = allowedWeapons != -1 && weaponsInTeam + 1 > allowedWeapons;

        // not exceeding limits
        if (!willExceedLimits)
            return HookResult.Continue;

        // skip warning if we already warned this player in the last 10 seconds
        if (!_lastWeaponRestrictPrint.TryGetValue(player.Pawn.Index, out var lastPrintTime) ||
            lastPrintTime + 10 <= Server.CurrentTime)
        {
            player.PrintToChat($"{ChatUtils.FormatMessage(_plugin.Config.ChatPrefix)} {ChatColors.Red}{item}{ChatColors.Default} is restricted to {ChatColors.Red}{allowedWeapons}{ChatColors.Default} per team!");
            _lastWeaponRestrictPrint[player.Pawn.Index] = Server.CurrentTime;
        }

        hook.SetReturn(acquireMethod == AcquireMethod.PickUp ? AcquireResult.InvalidItem : AcquireResult.AlreadyOwned);
        return HookResult.Stop;
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