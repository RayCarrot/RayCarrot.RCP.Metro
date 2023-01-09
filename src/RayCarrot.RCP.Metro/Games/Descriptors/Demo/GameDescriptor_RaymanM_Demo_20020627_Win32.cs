using RayCarrot.RCP.Metro.Games.Components;
using RayCarrot.RCP.Metro.Games.OptionsDialog;
using RayCarrot.RCP.Metro.Games.Structure;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman M Demo 2002/06/27 (Win32) game descriptor
/// </summary>
public sealed class GameDescriptor_RaymanM_Demo_20020627_Win32 : Win32GameDescriptor
{
    #region Public Properties

    public override string GameId => "RaymanM_Demo_20020627_Win32";
    public override Game Game => Game.RaymanMArena;
    public override GameCategory Category => GameCategory.Rayman;
    public override bool IsDemo => true;
    public override LegacyGame? LegacyGame => Metro.LegacyGame.Demo_RaymanM;

    public override LocalizedString DisplayName => "Rayman M Demo (2002/06/27)";
    public override DateTime ReleaseDate => new(2002, 06, 27);

    public override GameIconAsset Icon => GameIconAsset.RaymanM_Demo;
    public override GameBannerAsset Banner => GameBannerAsset.RaymanM;

    public override IEnumerable<string> DialogGroupNames => new[] { UbiIniFileGroupName };

    #endregion

    #region Protected Methods

    protected override void RegisterComponents(IGameComponentBuilder builder)
    {
        base.RegisterComponents(builder);

        builder.Register(new ProgressionManagersComponent(x => new GameProgressionManager_RaymanMArena(x, "Rayman M Demo", true)));
        builder.Register(new GameConfigComponent(x => new RaymanMDemoConfigViewModel(x)));
        builder.Register<LocalGameLinksComponent, RaymanMArenaSetupLocalGameLinksComponent>();
        builder.Register<BinaryGameModeComponent>(new CPAGameModeComponent(CPAGameMode.RaymanM_PC));
        builder.Register<ArchiveComponent>(new CPAArchiveComponent(_ => new[]
        {
            @"FishBin\tex32.cnt",
            @"FishBin\vignette.cnt",
            @"MenuBin\tex32.cnt",
            @"MenuBin\vignette.cnt",
            @"TribeBin\tex32.cnt",
            @"TribeBin\vignette.cnt",
        }));
    }

    protected override GameInstallationStructure GetStructure() => new(new GameInstallationPath[]
    {
        // Files
        new GameInstallationFilePath("RaymanM.exe", GameInstallationPathType.PrimaryExe, required: true),
    });

    #endregion

    #region Public Methods

    public override IEnumerable<GameAddAction> GetAddActions() => base.GetAddActions().Concat(new GameAddAction[]
    {
        new DownloadGameAddAction(this, new Uri[]
        {
            new(AppURLs.Games_RMDemo_Url),
        })
    });

    #endregion
}