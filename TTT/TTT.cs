using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Timers;

namespace CS2_TTT;

[MinimumApiVersion(142)] 
public class TTT : BasePlugin, IPluginConfig<TTTConfig>
{
    public override string ModuleName => TTTGlobals.BaseName;
    public override string ModuleVersion => TTTGlobals.BaseVersion;
    public override string ModuleAuthor => TTTGlobals.BaseAuthor;
    public override string ModuleDescription => TTTGlobals.BaseDescription;

    public TTTConfig Config
    {
        get => TTTGlobals.Config;
        set => TTTGlobals.Config = value;
    }

    public void OnConfigParsed(TTTConfig config)
    {
        if (config.TraitorRatio < 0) config.TraitorRatio = 0;
        if (config.DetectiveRatio < 0) config.DetectiveRatio = 0;
        if (config.GraceTime < 0) config.GraceTime = 0;

        Config = config;
    }

    public override void Load(bool hotReload)
    {
        base.Load(hotReload);
        
        // Register Command Listeners
        AddCommandListener("kill", OnKillCommand);
        AddCommandListener("explode", OnKillCommand);
        AddCommandListener("spectate", OnKillCommand);
        AddCommandListener("jointeam", OnKillCommand);
        AddCommandListener("explodevector", OnKillCommand);
        AddCommandListener("killvector", OnKillCommand);
    }

    // Commands
    [ConsoleCommand("css_setrole", "Sets the role of a TTT player")]
    [CommandHelper(2, "<player> <i/t/d>")]
    [RequiresPermissions("@css/slay")]
    public void OnSetRoleCommand(CCSPlayerController? caller, CommandInfo info)
    {
        if (TTTGlobals.RoundStatus != RoundStatus.Active) return;
        var player = TTTGlobals.GetTTTPlayer(info);
        if (player == null) return;

        if (!player.Alive)
        {
            info.ReplyToCommand(TTTGlobals.FormatMessage("You cannot change the role of a dead player."));
            return;
        }

        var newRole = info.GetArg(2).ToLower();
        switch (newRole)
        {
            case "i":
                player.SetRole(Role.Innocent);
                break;
            case "t":
                player.SetRole(Role.Traitor);
                break;
            case "d":
                player.SetRole(Role.Detective);
                break;
            default:
                info.ReplyToCommand(TTTGlobals.FormatMessage("Role is not valid, must be one of: i, t, d."));
                return;
        }
        
        info.ReplyToCommand(TTTGlobals.FormatMessage($"Successfully changed {player.Controller.PlayerName}'s role to {player.GetRole().GetStringColoured()}"));
    }
    
    // Command Listeners
    
    private HookResult OnKillCommand(CCSPlayerController? caller, CommandInfo info)
    {
        if (caller == null) return HookResult.Continue;
        if (!Config.BlockSuicide || !caller.PawnIsAlive) return HookResult.Continue;
        caller.PrintToChat(TTTGlobals.FormatMessage("Suicide Blocked"));
        return HookResult.Stop;
    }

    // EVENTS
    
    [GameEventHandler(HookMode.Pre)]
    public void OnPlayerDeathPre(EventPlayerDeath @event, GameEventInfo info)
    {
        if (!@event.Userid.IsValid || !@event.Attacker.IsValid) return;
        var player = TTTGlobals.GetTTTPlayer(@event.Userid.SteamID);
        if (player == null) return;

        if (player.GetRole() > Role.Unassigned)
        {
            player.Killer = TTTGlobals.GetTTTPlayer(@event.Attacker.SteamID);
        }
    }

    [GameEventHandler(HookMode.Pre)]
    public void OnRoundStartPre(EventRoundPrestart @event, GameEventInfo info)
    {
        foreach (TTTPlayer player in TTTGlobals.Players.Values)
        {
            player.Reset();
        }
    }

    [GameEventHandler(HookMode.Pre)]
    public HookResult OnPlayerSpawnPre(EventPlayerSpawn @event, GameEventInfo info)
    {
        if (!@event.Userid.IsValid) return HookResult.Continue;
        
        if (TTTGlobals.RoundStatus == RoundStatus.Active)
        {
            @event.Userid.ClanName = "Unassigned";
            return HookResult.Stop;
        }

        return HookResult.Continue;
    }

    [GameEventHandler]
    public void OnRoundStart(EventRoundStart @event, GameEventInfo info)
    {
        if (TTTGlobals.GameRules().WarmupPeriod) return;
        AddTimer(Config.GraceTime, OnGraceTimeOver, TimerFlags.STOP_ON_MAPCHANGE);
    }
    
    [GameEventHandler]
    public void OnPlayerConnectFull(EventPlayerConnectFull @event, GameEventInfo info)
    {
        // TODO: Figure out what to do here
    }
    
    [GameEventHandler]
    public void OnPlayerSpawn(EventPlayerSpawn @event, GameEventInfo info)
    {
        if (!@event.Userid.IsValid) return;
        CCSPlayerController controller = @event.Userid;

        if (TTTGlobals.RoundStatus != RoundStatus.Active)
        {
            controller.RemoveWeapons();
            controller.GiveNamedItem("weapon_knife");
            if (Config.StartingSecondary.Length > 0)
                controller.GiveNamedItem(Config.StartingSecondary);
            if (Config.StartingPrimary.Length > 0)
                controller.GiveNamedItem(Config.StartingPrimary);
        }
    }
    
    [GameEventHandler]
    public void OnPlayerDeath(EventPlayerDeath @event, GameEventInfo info)
    {
        if (!@event.Userid.IsValid || !@event.Attacker.IsValid) return;
        var victim = TTTGlobals.GetTTTPlayer(@event.Userid.SteamID);
        if (victim == null) return;
        var attacker = victim.Killer;
        if (attacker == null) return;
        
        victim.Controller.PrintToChat(TTTGlobals.FormatMessage($"You were killed by a(n) {attacker.GetRole().GetStringColoured()}."));
        attacker.Controller.PrintToChat(TTTGlobals.FormatMessage($"You killed a(n) {victim.GetRole().GetStringColoured()}."));
        // TODO: REMOVE UPON SWITCH TO BODIES
        victim.RevealRole();
    }
    
    [GameEventHandler]
    public void OnWinPanelRound(EventCsWinPanelRound @event, GameEventInfo info)
    {
        TTTGlobals.RoundStatus = RoundStatus.Ending;
    }
    
    [GameEventHandler]
    public void OnWinPanelMatch(EventCsWinPanelMatch @event, GameEventInfo info)
    {
        TTTGlobals.RoundStatus = RoundStatus.Ending;
    }
    
    [GameEventHandler]
    public void OnMatchEndRestart(EventCsMatchEndRestart @event, GameEventInfo info)
    {
        TTTGlobals.RoundStatus = RoundStatus.Ending;
    }
    
    // TIMERS
    public void OnGraceTimeOver()
    {
        
    }
}

// TODO: Create TTTPlayers
// TODO: Ensure that TTTPlayers is always valid
// TODO: Figure out Grace
// TODO: Prevent game from starting without > 2 people
// TODO: Ensure player can't spawn in middle of game
// TODO: Check for round end
// TODO: Figure out CS_OnTerminateRound
// TODO: Figure out team chat
// TODO: Figure out bodies
// TODO: Select roles
