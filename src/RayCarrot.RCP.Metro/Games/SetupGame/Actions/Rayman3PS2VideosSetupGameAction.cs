namespace RayCarrot.RCP.Metro.Games.SetupGame;

public class Rayman3PS2VideosSetupGameAction : InstallModSetupGameAction
{
    protected override long GameBananaModId => 491386;
    protected override string ModId => "Rayman3.Gamefiles.PS2Videos";

    // TODO-LOC
    public override LocalizedString Header => "Install the PS2 Videos mod";
    public override LocalizedString Info => "The video cutscenes in the PC version are more compressed and lower quality than in the console versions. The PS2 Videos mod replaces the videos with the higher quality videos from the PlayStation 2 version.";

    public override SetupGameActionType Type => SetupGameActionType.Optional;
}