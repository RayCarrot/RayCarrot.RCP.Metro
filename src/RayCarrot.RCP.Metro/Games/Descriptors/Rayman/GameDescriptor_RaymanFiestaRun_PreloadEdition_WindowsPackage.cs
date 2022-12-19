using RayCarrot.RCP.Metro.Games.Components;
using RayCarrot.RCP.Metro.Games.OptionsDialog;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman Fiesta Run Preload Edition (Windows Package) game descriptor
/// </summary>
public sealed class GameDescriptor_RaymanFiestaRun_PreloadEdition_WindowsPackage : WindowsPackageGameDescriptor
{
    #region Private Constant Fields

    private const string MicrosoftStoreID = "9wzdncrdcw9b";

    #endregion

    #region Public Properties

    public override string GameId => "RaymanFiestaRunPreloadEdition_WindowsPackage";
    public override Game Game => Game.RaymanFiestaRun;
    public override GameCategory Category => GameCategory.Rayman;
    public override LegacyGame? LegacyGame => Metro.LegacyGame.RaymanFiestaRun;

    public override LocalizedString DisplayName => "Rayman Fiesta Run Preload Edition";
    public override string DefaultFileName => "RFR_WinRT_OEM.exe";
    public override DateTime ReleaseDate => new(2014, 06, 04);

    public override GameIconAsset Icon => GameIconAsset.RaymanFiestaRun;

    public override string PackageName => "UbisoftEntertainment.RaymanFiestaRunPreloadEdition";
    public override string FullPackageName => "UbisoftEntertainment.RaymanFiestaRunPreloadEdition_dbgk1hhpxymar";

    #endregion

    #region Protected Methods

    protected override void RegisterComponents(DescriptorComponentBuilder builder)
    {
        base.RegisterComponents(builder);

        builder.Register(new ProgressionManagersComponent(x => new GameProgressionManager_RaymanFiestaRun(this, x, "Rayman Fiesta Run (Preload)", 1)));
        builder.Register(new GameConfigComponent(x => new RaymanFiestaRunConfigViewModel(this, x)));
        builder.Register<OnGameAddedComponent, AddToJumpListOnGameAddedComponent>();

        builder.Register(new UtilityComponent(x => new Utility_RaymanFiestaRun_SaveFix(this, x, 1)));
    }

    #endregion

    #region Public Methods

    public override IEnumerable<GameUriLink> GetExternalUriLinks(GameInstallation gameInstallation) => new[]
    {
        new GameUriLink(
            Header: new ResourceLocString(nameof(Resources.GameDisplay_OpenInWinStore)),
            Uri: MicrosoftStoreHelpers.GetStorePageURI(MicrosoftStoreID),
            Icon: GenericIconKind.GameAction_Microsoft)
    };

    #endregion
}