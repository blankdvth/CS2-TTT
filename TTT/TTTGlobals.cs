using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;
using CSSTargetResult = CounterStrikeSharp.API.Modules.Commands.Targeting.TargetResult;

namespace CS2_TTT;

public enum RoundStatus
{
    Inactive,
    Warmup,
    Selection,
    Active,
    Ending,
    Ended
}

public enum Role
{
    Unassigned,
    Innocent,
    Traitor,
    Detective
}

public static class RoleExtension
{
    public static String GetString(this Role role, bool lower = true)
    {
        var str = role.ToString();
        if (lower) str = str.ToLower();
        else str = str.ToUpper();
        return str;
    }

    public static String GetStringColoured(this Role role, bool lower = false)
    {
        var colour = role switch
        {
            Role.Innocent => ChatColors.Lime,
            Role.Traitor => ChatColors.Red,
            Role.Detective => ChatColors.Blue,
            _ => ChatColors.Grey
        };

        return colour + GetString(role, lower) + ChatColors.Default;
    }
}

public static class TTTGlobals
{
    public const string BaseName = "Trouble in Terrorist Town";
    public const string BaseVersion = "0.0.1";
    public const string BaseAuthor = "blank_dvth";
    public const string BaseDescription = "A plugin to handle the Trouble in Terrorist Town game mode logic for Counter-Strike 2";
    
    public static RoundStatus RoundStatus = RoundStatus.Inactive;
    public static Dictionary<ulong, TTTPlayer> Players = new();
    public static TTTConfig Config { get; set; } = new();
    
    public static string FormatMessage(string message)
    {
        return
            $"{ChatColors.Magenta}[{ChatColors.Lime}T{ChatColors.Red}T{ChatColors.Blue}T{ChatColors.Magenta}]{ChatColors.Default} {message}";
    }
    
    public static CCSGameRules GameRules()
    {
        return Utilities.FindAllEntitiesByDesignerName<CCSGameRulesProxy>("cs_gamerules").First().GameRules!;
    }
    
    public static CSSTargetResult? GetTarget(CommandInfo info, bool allowMultiple = true, bool noError = false)
    {
        var matches = info.GetArgTargetResult(1);

        if (!matches.Any())
        {
            if (!noError)
                info.ReplyToCommand(FormatMessage($"Could not find target {info.GetArg(1)}."));
            return null;
        }
        
        if (!(matches.Count() > 1) || (info.GetArg(1).StartsWith('@') && allowMultiple)) 
            return matches;
        
        if (!noError)
            info.ReplyToCommand(FormatMessage($"Group targeters are not allowed for this command."));

        return null;
    }

    public static TTTPlayer? GetTTTPlayer(CCSPlayerController controller)
    {
        return Players.GetValueOrDefault(controller.SteamID);
    }

    public static TTTPlayer? GetTTTPlayer(CommandInfo info, bool noError = false)
    {
        var controller = GetTarget(info, false, noError);
        if (controller == null) return null;
        return GetTTTPlayer(controller.Players[0]);
    }

    public static TTTPlayer? GetTTTPlayer(ulong steamId)
    {
        return Players.GetValueOrDefault(steamId);
    }
}