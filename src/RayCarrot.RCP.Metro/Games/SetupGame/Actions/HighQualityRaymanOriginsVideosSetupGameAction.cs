namespace RayCarrot.RCP.Metro.Games.SetupGame;

public class HighQualityRaymanOriginsVideosSetupGameAction : InstallModSetupGameAction
{
    protected override long GameBananaModId => 480156;
    protected override string[] ModIds => ["86256503-f4cd-4724-acac-cc8e583cedf5"];

    public override LocalizedString Header => new ResourceLocString(nameof(Resources.SetupGameAction_HighQualityRaymanOriginsVideos_Header));
    public override LocalizedString Info => new ResourceLocString(nameof(Resources.SetupGameAction_HighQualityRaymanOriginsVideos_Info));

    public override SetupGameActionType Type => SetupGameActionType.Optional;
}