using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;

namespace CS2_TTT;

public class TTTPlayer
{
    private Role _role = Role.Unassigned;

    public bool Alive => Controller.PawnIsAlive;
    public bool RoleRevealed = false;
    public TTTPlayer? Killer = null;
    public readonly CCSPlayerController Controller;

    public TTTPlayer(CCSPlayerController controller)
    {
        this.Controller = controller;
    }

    public Role GetRole()
    {
        return _role;
    }

    /// <summary>
    /// Set the role of the player, notifying them of it.
    /// </summary>
    /// <param name="newRole">Role to change to</param>
    public void SetRole(Role newRole)
    {
        _role = newRole;
        var message = TTTGlobals.FormatMessage($"You are now a(n) {_role.GetStringColoured()}.");
        Controller.PrintToCenter(message);
        Controller.PrintToChat(message);
    }

    /// <summary>
    /// Reveal the player's role, in chat and via their clan tag.
    /// </summary>
    public void RevealRole()
    {
        RoleRevealed = true;
        Server.PrintToChatAll(TTTGlobals.FormatMessage($"{Controller.PlayerName} was a(n) {_role.GetStringColoured()}."));
        Controller.ClanName = _role.GetString(false);
    }

    /// <summary>
    /// Resets the properties that get changed with each round to those expected at the start of a new round.
    /// </summary>
    public void Reset()
    {
        _role = Role.Unassigned;
        RoleRevealed = false;
        Killer = null;
        Controller.ClanName = " ";
    }
}