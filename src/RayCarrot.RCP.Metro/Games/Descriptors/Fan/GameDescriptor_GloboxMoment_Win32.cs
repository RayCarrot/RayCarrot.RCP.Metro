using RayCarrot.RCP.Metro.Games.Components;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Globox Moment (Win32) game descriptor
/// </summary>
public sealed class GameDescriptor_GloboxMoment_Win32 : Win32GameDescriptor
{
    #region Public Properties

    public override string GameId => "GloboxMoment_Win32";
    public override Game Game => Game.GloboxMoment;
    public override GameCategory Category => GameCategory.Fan;
    public override LegacyGame? LegacyGame => Metro.LegacyGame.GloboxMoment;

    public override LocalizedString DisplayName => "Globox Moment";
    public override string DefaultFileName => "Globox Moment.exe";
    public override DateTime ReleaseDate => new(2019, 07, 26); // Unsure if this is correct

    public override GameIconAsset Icon => GameIconAsset.GloboxMoment;

    // TODO-14: Should we be removing this?
    public override IEnumerable<FileSystemPath> UninstallFiles => new[]
    {
        Environment.SpecialFolder.ApplicationData.GetFolderPath() + "MMFApplications" + "globoxmoment.ini"
    };

    #endregion

    #region Protected Methods

    protected override void RegisterComponents(DescriptorComponentBuilder builder)
    {
        base.RegisterComponents(builder);

        builder.Register(new ProgressionManagersComponent(x => new GameProgressionManager_GloboxMoment(x, "Globox Moment")));
    }

    #endregion

    #region Public Methods

    public override IEnumerable<GameUriLink> GetExternalUriLinks(GameInstallation gameInstallation) => new[]
    {
        new GameUriLink(
            Header: new ResourceLocString(nameof(Resources.GameDisplay_OpenGameJoltPage)),
            Uri: "https://gamejolt.com/games/globoxmoment/428585",
            Icon: GenericIconKind.GameAction_Web)
    };

    public override IEnumerable<GamePurchaseLink> GetPurchaseLinks() => new GamePurchaseLink[]
    {
        new(new ResourceLocString(nameof(Resources.GameDisplay_GameJolt)), "https://gamejolt.com/games/globoxmoment/428585", GenericIconKind.GameAction_Web),
    };

    #endregion
}