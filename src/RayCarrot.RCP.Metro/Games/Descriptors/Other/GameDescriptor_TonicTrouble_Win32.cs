using RayCarrot.RCP.Metro.Archive.CPA;
using RayCarrot.RCP.Metro.Games.Components;
using RayCarrot.RCP.Metro.Games.Finder;
using RayCarrot.RCP.Metro.Games.Structure;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Tonic Trouble (Win32) game descriptor
/// </summary>
public sealed class GameDescriptor_TonicTrouble_Win32 : Win32GameDescriptor
{
    #region Public Override Properties

    public override string GameId => "TonicTrouble_Win32";
    public override string LegacyGameId => "TonicTrouble";
    public override Game Game => Game.TonicTrouble;
    public override GameCategory Category => GameCategory.Other;

    public override LocalizedString DisplayName => "Tonic Trouble";
    public override DateTime ReleaseDate => new(1999, 12, 07);

    public override GameIconAsset Icon => GameIconAsset.TonicTrouble;
    public override GameBannerAsset Banner => GameBannerAsset.TonicTrouble;

    #endregion

    #region Private Methods

    private static string GetLaunchArgs(GameInstallation gameInstallation) => "-cdrom:";

    private static IEnumerable<GameLinksComponent.GameUriLink> GetLocalGameLinks(GameInstallation gameInstallation) => new[]
    {
        new GameLinksComponent.GameUriLink(
            Header: new ResourceLocString(nameof(Resources.GameLink_R2dgVoodoo)), 
            Uri: gameInstallation.InstallLocation.Directory + "dgVoodooCpl.exe"),
    };

    #endregion

    #region Protected Methods

    protected override void RegisterComponents(IGameComponentBuilder builder)
    {
        base.RegisterComponents(builder);

        builder.Register(new ProgressionManagersComponent(x => new GameProgressionManager_TonicTrouble(x, "Tonic Trouble")));
        builder.Register<OnGameAddedComponent, AddToJumpListOnGameAddedComponent>();
        builder.Register(new LaunchArgumentsComponent(GetLaunchArgs));
        builder.Register(new LocalGameLinksComponent(GetLocalGameLinks));
        builder.Register(new RayMapComponent(RayMapComponent.RayMapViewer.RayMap, "tt_pc", "tt_pc"));
        builder.Register<BinaryGameModeComponent>(new CPAGameModeComponent(CPAGameMode.TonicTrouble_PC));
        builder.Register<ArchiveComponent>(new CPAArchiveComponent(_ => new[]
        {
            @"gamedata\Textures.cnt",
            @"gamedata\Vignette.cnt",
        }));
        builder.Register(new CPATextureSyncComponent(
            new CPATextureSyncDataItem(
                Name: "GameData",
                Archives: new[] { "Textures.cnt", "Vignette.cnt" })));
    }

    protected override ProgramInstallationStructure GetStructure() => new(new GameInstallationPath[]
    {
        // Files
        new GameInstallationFilePath("TonicTrouble.exe", GameInstallationPathType.PrimaryExe, required: true),
    });

    #endregion

    #region Public Methods

    public override FinderQuery[] GetFinderQueries() => new FinderQuery[]
    {
        new UbiIniFinderQuery("TONICT"),

        new UninstallProgramFinderQuery("Tonic Trouble"),

        new Win32ShortcutFinderQuery("Tonic Trouble"),
    };

    #endregion
}