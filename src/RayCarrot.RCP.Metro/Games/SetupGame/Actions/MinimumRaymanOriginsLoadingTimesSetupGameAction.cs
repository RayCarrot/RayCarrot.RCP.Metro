namespace RayCarrot.RCP.Metro.Games.SetupGame;

public class MinimumRaymanOriginsLoadingTimesSetupGameAction : InstallModSetupGameAction
{
    protected override long GameBananaModId => 510228;
    protected override string ModId => "RaymanOrigins.MinLoading";

    public override LocalizedString Header => new ResourceLocString(nameof(Resources.SetupGameAction_MinimumRaymanOriginsLoadingTimes_Header));
    public override LocalizedString Info => new ResourceLocString(nameof(Resources.SetupGameAction_MinimumRaymanOriginsLoadingTimes_Info));

    public override SetupGameActionType Type => SetupGameActionType.Optional;
}