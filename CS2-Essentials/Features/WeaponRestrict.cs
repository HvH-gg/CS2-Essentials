using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Cvars.Validators;
using CounterStrikeSharp.API.Modules.Entities.Constants;
using CounterStrikeSharp.API.Modules.Memory;
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

    private bool checkWarmup;
    public bool InWarmup = false;
    private string AllowedFlag;

    public WeaponRestrict(Plugin plugin)
    {
        _plugin = plugin;
        _plugin.RegisterFakeConVars(this);
        hvh_restrict_awp.Value = _plugin.Config.AllowedAwpCount;
        hvh_restrict_scout.Value = _plugin.Config.AllowedScoutCount;
        hvh_restrict_auto.Value = _plugin.Config.AllowedAutoSniperCount;
        checkWarmup = _plugin.Config.AllowAllWeaponsOnWarmup;
        AllowedFlag = _plugin.Config.AllowWeaponsForFlag is string && _plugin.Config.AllowWeaponsForFlag.Length > 0 
            ? _plugin.Config.AllowWeaponsForFlag 
            : "@css/restrict";
    }

    public HookResult OnWeaponCanAcquire(DynamicHook hook)
    {
        // Warmup check
        if (checkWarmup && InWarmup)
            return HookResult.Continue;

        CCSWeaponBaseVData vdata = VirtualFunctions.GetCSWeaponDataFromKeyFunc.Invoke(-1, hook.GetParam<CEconItemView>(1).ItemDefinitionIndex.ToString()) ?? throw new Exception("Failed to get CCSWeaponBaseVData");

        // Weapon is not restricted
        if (!_weaponPrices.ContainsKey(vdata.Name))
            return HookResult.Continue;

        CCSPlayerController client = hook.GetParam<CCSPlayer_ItemServices>(0).Pawn.Value!.Controller.Value!.As<CCSPlayerController>();

        if (client == null || !client.IsValid || !client.PawnIsAlive)
            return HookResult.Continue;

        // Player is Admin with "@css/cheats" flag and custom one
        if (AdminManager.PlayerHasPermissions(client, ["@css/cheats", AllowedFlag]))
            return HookResult.Continue;

        // Get every valid player that is currently connected
        IEnumerable<CCSPlayerController> players = Utilities.GetPlayers().Where(player =>
            player.IsValid // Unneccessary?
            && player.Connected == PlayerConnectedState.PlayerConnected
            && player.Team == client.Team
            );

        int limit = GetAllowedWeaponCount(vdata.Name);
        bool disabled = limit <= -1;

        if (!disabled)
        {
            int count = GetWeaponCountInTeam(vdata.Name, client.Team);
            if (count < limit)
                return HookResult.Continue;
        }
        else
        {
            return HookResult.Continue;
        }

        // Print chat message if we attempted to do anything except pick up this weapon. This is to prevent chat spam.
        if (hook.GetParam<AcquireMethod>(2) != AcquireMethod.PickUp)
        {
            hook.SetReturn(AcquireResult.AlreadyOwned);

            Server.NextFrame(() => client.PrintToChat($"{ChatUtils.FormatMessage(_plugin.Config.ChatPrefix)} {ChatColors.Red}{vdata.Name}{ChatColors.Default} is restricted to {ChatColors.Red}{limit}{ChatColors.Default} per team!"));
        }
        else
        {
            hook.SetReturn(AcquireResult.InvalidItem);
        }

        RefundItem(client, vdata.Name);

        return HookResult.Stop;
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