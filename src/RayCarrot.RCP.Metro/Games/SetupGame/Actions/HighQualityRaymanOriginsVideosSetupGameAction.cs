namespace RayCarrot.RCP.Metro.Games.SetupGame;

public class HighQualityRaymanOriginsVideosSetupGameAction : InstallModSetupGameAction
{
    protected override long GameBananaModId => 480156;
    protected override string ModId => "86256503-f4cd-4724-acac-cc8e583cedf5";

    // TODO-LOC
    public override LocalizedString Header => "Install the High Quality Videos mod";
    public override LocalizedString Info => "The videos in the PC version are more compressed than in the console versions. The High Quality Videos mod replaces the videos with higher quality converted ones from the the console versions of the game.";

    public override SetupGameActionType Type => SetupGameActionType.Optional;
}