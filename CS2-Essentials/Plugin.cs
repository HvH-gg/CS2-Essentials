using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Entities.Constants;
using CounterStrikeSharp.API.Modules.Memory;
using CounterStrikeSharp.API.Modules.Memory.DynamicFunctions;
using CounterStrikeSharp.API.Modules.Timers;
using CounterStrikeSharp.API.Modules.Utils;
using hvhgg_essentials.Enums;
using hvhgg_essentials.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace hvhgg_essentials;

public partial class Cs2EssentialsConfig : BasePluginConfig
{
    [JsonPropertyName("RapidFireFixMethod")] public FixMethod RapidFireFixMethod { get; set; } = FixMethod.Ignore;
    [JsonPropertyName("RapidFireReflectScale")] public float RapidFireReflectScale { get; set; } = 1f;
    [JsonPropertyName("AllowedAwpCount")] public int AllowedAwpCount { get; set; } = -1;
    [JsonPropertyName("AllowedScoutCount")] public int AllowedScoutCount { get; set; } = -1;
    [JsonPropertyName("AllowedAutoSniperCount")] public int AllowedAutoSniperCount { get; set; } = -1;
    [JsonPropertyName("UnmatchedFriendlyFire")] public bool UnmatchedFriendlyFire { get; set; } = true;
    [JsonPropertyName("RestrictTeleport")] public bool RestrictTeleport { get; set; } = true;
    [JsonPropertyName("AllowAdPrint")] public bool AllowAdPrint { get; set; } = true;
    [JsonPropertyName("AllowResetScore")] public bool AllowResetScore { get; set; } = true;
    [JsonPropertyName("AllowRageQuit")] public bool AllowRageQuit { get; set; } = true;
    [JsonPropertyName("ChatPrefix")] public string ChatPrefix { get; set; } = "[{Red}Hv{DarkRed}H{Default}.gg]";
    [JsonPropertyName("ConfigVersion")] public override int Version { get; set; } = 2;
}

public class Plugin : BasePlugin, IPluginConfig<Cs2EssentialsConfig>
{
    public override string ModuleName => "HvH.gg - Essentials";
    public override string ModuleVersion => "1.0.1";
    public override string ModuleAuthor => "imi-tat0r";
    public override string ModuleDescription => "Essential features for CS2 HvH servers";
    public Cs2EssentialsConfig Config { get; set; } = new();
    
    private ServiceProvider? _serviceProvider = null;
    
    private static readonly string AssemblyName = Assembly.GetExecutingAssembly().GetName().Name ?? "";
    public static readonly string CfgPath = $"{Server.GameDirectory}/csgo/addons/counterstrikesharp/configs/plugins/{AssemblyName}/{AssemblyName}.json";

    public void OnConfigParsed(Cs2EssentialsConfig config)
    {
        Config = config;
        UpdateConfig(config);
    }
    
    private static void UpdateConfig<T>(T config) where T : BasePluginConfig, new()
    {
        // get current config version
        var newCfgVersion = new T().Version;
        
        // loaded config is up to date
        if (config.Version == newCfgVersion)
            return;
        
        // update the version
        config.Version = newCfgVersion;
        
        // serialize the updated config back to json
        var updatedJsonContent = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(CfgPath, updatedJsonContent);
    }

    public override void Load(bool hotReload)
    {
        base.Load(hotReload);

        Console.WriteLine("[HvH.gg] Start loading HvH.gg Essentials plugin");

        var services = new ServiceCollection();
        
        services.AddLogging(options =>
        {
            options.AddConsole();
        });
        
        services.AddSingleton(this);
        services.AddSingleton<RageQuit>();
        services.AddSingleton<ResetScore>();
        services.AddSingleton<RapidFire>();
        services.AddSingleton<WeaponRestrict>();
        services.AddSingleton<FriendlyFire>();
        services.AddSingleton<TeleportFix>();
        services.AddSingleton<Misc>();
        
        _serviceProvider = services.BuildServiceProvider();
        
        // register features
        UseWeaponRestrict();
        UseRapidFireRestrict();
        UseFriendlyFireRestrict();
        UseRageQuit();
        UseResetScore();
        UseMisc();
        UseTeleport();

        Console.WriteLine("[HvH.gg] Finished loading HvH.gg Essentials plugin");
    }
    
    private void UseTeleport()
    {
        var teleportFix = _serviceProvider!.GetRequiredService<TeleportFix>();
        RegisterListener<Listeners.OnTick>(teleportFix.OnTick);
    }
    private void UseMisc()
    {
        Console.WriteLine("[HvH.gg] Register misc commands");
        
        var misc = _serviceProvider!.GetRequiredService<Misc>();
        RegisterConsoleCommandAttributeHandlers(misc);
        
        RegisterEventHandler<EventPlayerSpawn>((eventPlayerSpawn, info) =>
        {
            misc.AnnounceRules(eventPlayerSpawn.Userid);
            return HookResult.Continue;
        });
        
        Console.WriteLine("[HvH.gg] Finished registering misc commands");
    }
    private void UseResetScore()
    {
        Console.WriteLine("[HvH.gg] Register reset score command");
        
        var resetScore = _serviceProvider!.GetRequiredService<ResetScore>();
        RegisterConsoleCommandAttributeHandlers(resetScore);
        
        Console.WriteLine("[HvH.gg] Finished registering reset score command");
    }
    private void UseRageQuit()
    {
        Console.WriteLine("[HvH.gg] Register rage quit command");
        
        var rageQuit = _serviceProvider!.GetRequiredService<RageQuit>();
        RegisterConsoleCommandAttributeHandlers(rageQuit);
        
        Console.WriteLine("[HvH.gg] Finished registering rage quit command");
    }
    private void UseFriendlyFireRestrict()
    {
        Console.WriteLine("[HvH.gg] Register friendly fire listeners");
        
        var friendlyFire = _serviceProvider!.GetRequiredService<FriendlyFire>();
        VirtualFunctions.CBaseEntity_TakeDamageOldFunc.Hook(friendlyFire.OnTakeDamage, HookMode.Pre);
        
        Console.WriteLine("[HvH.gg] Finished registering friendly fire listeners");
    }
    private void UseRapidFireRestrict()
    {
        Console.WriteLine("[HvH.gg] Register rapid fire listeners");
        
        var rapidFire = _serviceProvider!.GetRequiredService<RapidFire>();
        RegisterEventHandler<EventWeaponFire>(rapidFire.OnWeaponFire);
        VirtualFunctions.CBaseEntity_TakeDamageOldFunc.Hook(rapidFire.OnTakeDamage, HookMode.Pre);
        
        Console.WriteLine("[HvH.gg] Finished registering rapid fire listeners");
    }
    private void UseWeaponRestrict()
    {
        Console.WriteLine("[HvH.gg] Register weapon restriction listeners");
        
        var weaponRestrict = _serviceProvider!.GetRequiredService<WeaponRestrict>();
        RegisterEventHandler<EventItemPurchase>(weaponRestrict.OnItemPurchase);
        VirtualFunctions.CCSPlayer_WeaponServices_CanUseFunc.Hook(weaponRestrict.OnWeaponCanUse, HookMode.Pre);
        
        Console.WriteLine("[HvH.gg] Finished registering weapon restriction listeners");
    }
    
    
    public override void Unload(bool hotReload)
    {
        base.Unload(hotReload);

        if (_serviceProvider is null)
            return;
        
        var friendlyFire = _serviceProvider.GetRequiredService<FriendlyFire>();
        var rapidFire = _serviceProvider.GetRequiredService<RapidFire>();
        
        VirtualFunctions.CBaseEntity_TakeDamageOldFunc.Unhook(rapidFire.OnTakeDamage, HookMode.Pre);
        VirtualFunctions.CBaseEntity_TakeDamageOldFunc.Unhook(friendlyFire.OnTakeDamage, HookMode.Pre);
        
        var weaponRestrict = _serviceProvider.GetRequiredService<WeaponRestrict>();
        RegisterEventHandler<EventItemPurchase>(weaponRestrict.OnItemPurchase);
        VirtualFunctions.CCSPlayer_WeaponServices_CanUseFunc.Unhook(weaponRestrict.OnWeaponCanUse, HookMode.Pre);
        
    }
}