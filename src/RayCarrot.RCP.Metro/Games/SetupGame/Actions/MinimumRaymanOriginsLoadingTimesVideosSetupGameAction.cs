namespace RayCarrot.RCP.Metro.Games.SetupGame;

public class MinimumRaymanOriginsLoadingTimesVideosSetupGameAction : InstallModSetupGameAction
{
    protected override long GameBananaModId => 510228;
    protected override string ModId => "RaymanOrigins.MinLoading";

    // TODO-LOC
    public override LocalizedString Header => "Install the Minimum Loading Times mod";
    public override LocalizedString Info => "The Minimum Loading Times mod is a mod by RayCarrot which changes the minimum loading screen time from 4 seconds to 0 seconds, thus making loading times faster.";

    public override SetupGameActionType Type => SetupGameActionType.Optional;
}