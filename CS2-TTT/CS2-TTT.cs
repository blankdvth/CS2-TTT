using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes;

namespace CS2_TTT;

[MinimumApiVersion(142)] // TODO: Work out 
// ReSharper disable once InconsistentNaming
public class CS2_TTT : BasePlugin
{
    public override string ModuleName => "Trouble in Terrorist Town";
    public override string ModuleVersion => "0.0.1";
    public override string ModuleAuthor => "blank_dvth";
    public override string ModuleDescription => 
        "A plugin to handle the Trouble in Terrorist Town game mode logic for Counter-Strike 2";
}