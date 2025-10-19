namespace RayCarrot.RCP.Metro.Games.SetupGame;

public class MinimumRaymanOriginsLoadingTimesSetupGameAction : InstallModSetupGameAction
{
    protected override long GameBananaModId => 510228;
    protected override string[] ModIds => 
    [
        "RaymanOrigins.MinLoading", 
        "RaymanOrigins.LoadlessOrigins.Steam", 
        "RaymanOrigins.LoadlessOrigins.Uplay",
    ];

    public override LocalizedString Header => new ResourceLocString(nameof(Resources.SetupGameAction_MinimumRaymanOriginsLoadingTimes_Header));
    public override LocalizedString Info => new ResourceLocString(nameof(Resources.SetupGameAction_MinimumRaymanOriginsLoadingTimes_Info));

    public override SetupGameActionType Type => SetupGameActionType.Optional;
}