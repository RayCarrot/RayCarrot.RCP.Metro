namespace RayCarrot.RCP.Metro.Games.SetupGame;

public class LevelTransparencyFixesSetupGameAction : InstallModSetupGameAction
{
    protected override long GameBananaModId => 578235;
    protected override string ModId => "Rayman3.Gamefiles.LevelTransparencyFixes";

    public override LocalizedString Header => "Install the Level Transparency Fixes mod";
    public override LocalizedString Info => "The Level Transparency Fixes mod is a mod by Droolie and ICUP321 which fixes the transparency for objects in certain levels.";

    public override SetupGameActionType Type => SetupGameActionType.Optional;
}