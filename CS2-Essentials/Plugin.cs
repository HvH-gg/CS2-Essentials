using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Capabilities;
using CounterStrikeSharp.API.Modules.Memory;
using CounterStrikeSharp.API.Modules.Memory.DynamicFunctions;
using CS2_CustomVotes.Shared;
using CSSharpUtils.Extensions;
using hvhgg_essentials.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace hvhgg_essentials;

public class Plugin : BasePlugin, IPluginConfig<Cs2EssentialsConfig>
{
    public override string ModuleName => "HvH.gg - Essentials";
    public override string ModuleVersion => "1.3.0";
    public override string ModuleAuthor => "imi-tat0r";
    public override string ModuleDescription => "Essential features for CS2 HvH servers";
    public Cs2EssentialsConfig Config { get; set; } = new();
    
    private ServiceProvider? _serviceProvider;
    
    public required MemoryFunctionVoid<CCSPlayer_MovementServices, IntPtr> RunCommand;

    public static PluginCapability<ICustomVoteApi> CustomVotesApi { get; } = new("custom_votes:api");
    private bool _isCustomVotesLoaded;
    
    public void OnConfigParsed(Cs2EssentialsConfig config)
    {
        Config = config;
        config.Update();
        
        RapidFire.hvh_restrict_rapidfire.Value = (int) Config.RapidFireFixMethod;
        RapidFire.hvh_rapidfire_reflect_scale.Value = Config.RapidFireReflectScale;
        FriendlyFire.hvh_unmatched_friendlyfire.Value = Config.UnmatchedFriendlyFire;
        TeleportFix.hvh_restrict_teleport.Value = Config.RestrictTeleport;
        WeaponRestrict.hvh_restrict_awp.Value = Config.AllowedAwpCount;
        WeaponRestrict.hvh_restrict_scout.Value = Config.AllowedScoutCount;
        WeaponRestrict.hvh_restrict_auto.Value = Config.AllowedAutoSniperCount;
        ResetScore.hvh_resetscore.Value = Config.AllowResetScore;
        RageQuit.hvh_ragequit.Value = Config.AllowRageQuit;
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

    public override void OnAllPluginsLoaded(bool hotReload)
    {
        base.OnAllPluginsLoaded(hotReload);
        
        try
        {
            if (CustomVotesApi.Get() is null)
                return;
        }
        catch (Exception)
        {
            Console.WriteLine("[HvH.gg] CS2-CustomVotes plugin not found. Custom votes will not be registered.");
            return;
        }
        
        _isCustomVotesLoaded = true;
        Console.WriteLine("[HvH.gg] Registering custom votes");
        RapidFire.RegisterCustomVotes(this);
        FriendlyFire.RegisterCustomVotes(this);
        TeleportFix.RegisterCustomVotes(this);
    }

    private void UseTeleport()
    {
        var teleportFix = _serviceProvider!.GetRequiredService<TeleportFix>();
        
        Console.WriteLine("[HvH.gg] Hooking run command");
        
        RunCommand = new MemoryFunctionVoid<CCSPlayer_MovementServices, IntPtr>(GameData.GetSignature("RunCommand"));
        RunCommand.Hook(teleportFix.RunCommand, HookMode.Pre);
    }
    private void UseMisc()
    {
        Console.WriteLine("[HvH.gg] Register misc commands");
        
        var misc = _serviceProvider!.GetRequiredService<Misc>();
        RegisterConsoleCommandAttributeHandlers(misc);
        
        RegisterEventHandler<EventPlayerSpawn>((eventPlayerSpawn, _) =>
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
        if (_serviceProvider is null)
            return;
        
        var friendlyFire = _serviceProvider.GetRequiredService<FriendlyFire>();
        var rapidFire = _serviceProvider.GetRequiredService<RapidFire>();
        
        VirtualFunctions.CBaseEntity_TakeDamageOldFunc.Unhook(rapidFire.OnTakeDamage, HookMode.Pre);
        VirtualFunctions.CBaseEntity_TakeDamageOldFunc.Unhook(friendlyFire.OnTakeDamage, HookMode.Pre);
        
        var weaponRestrict = _serviceProvider.GetRequiredService<WeaponRestrict>();
        RegisterEventHandler<EventItemPurchase>(weaponRestrict.OnItemPurchase);
        VirtualFunctions.CCSPlayer_WeaponServices_CanUseFunc.Unhook(weaponRestrict.OnWeaponCanUse, HookMode.Pre);
        
        var teleportFix = _serviceProvider.GetRequiredService<TeleportFix>();
        RunCommand.Unhook(teleportFix.RunCommand, HookMode.Pre);
        
        if (!_isCustomVotesLoaded)
            return;
        
        Console.WriteLine("[HvH.gg] Unregistering custom votes");
        
        RapidFire.UnregisterCustomVotes(this);
        FriendlyFire.UnregisterCustomVotes(this);
        TeleportFix.UnregisterCustomVotes(this);
        
        base.Unload(hotReload);
    }
}