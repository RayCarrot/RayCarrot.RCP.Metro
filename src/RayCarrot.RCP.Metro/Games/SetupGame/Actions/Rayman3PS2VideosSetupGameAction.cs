namespace RayCarrot.RCP.Metro.Games.SetupGame;

public class Rayman3PS2VideosSetupGameAction : InstallModSetupGameAction
{
    protected override long GameBananaModId => 491386;
    protected override string[] ModIds => ["Rayman3.Gamefiles.PS2Videos"];

    public override LocalizedString Header => new ResourceLocString(nameof(Resources.SetupGameAction_Rayman3PS2Videos_Header));
    public override LocalizedString Info => new ResourceLocString(nameof(Resources.SetupGameAction_Rayman3PS2Videos_Info));

    public override SetupGameActionType Type => SetupGameActionType.Optional;
}