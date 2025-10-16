namespace RayCarrot.RCP.Metro.Games.SetupGame;

public class LevelTransparencyFixesSetupGameAction : InstallModSetupGameAction
{
    protected override long GameBananaModId => 578235;
    protected override string[] ModIds => ["Rayman3.Gamefiles.LevelTransparencyFixes"];

    public override LocalizedString Header => new ResourceLocString(nameof(Resources.SetupGameAction_LevelTransparencyFixes_Header));
    public override LocalizedString Info => new ResourceLocString(nameof(Resources.SetupGameAction_LevelTransparencyFixes_Info));

    public override SetupGameActionType Type => SetupGameActionType.Optional;
}