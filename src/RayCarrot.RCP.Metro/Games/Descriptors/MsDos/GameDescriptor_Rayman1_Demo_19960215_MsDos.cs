using RayCarrot.RCP.Metro.Games.Components;
using RayCarrot.RCP.Metro.Games.Finder;
using RayCarrot.RCP.Metro.Games.OptionsDialog;
using RayCarrot.RCP.Metro.Games.Structure;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman 1 Demo 1996/02/15 (MS-DOS) game descriptor
/// </summary>
public sealed class GameDescriptor_Rayman1_Demo_19960215_MsDos : MsDosGameDescriptor
{
    #region Public Properties

    public override string GameId => "Rayman1_Demo_19960215_MsDos";
    public override string LegacyGameId => "Demo_Rayman1_2";
    public override Game Game => Game.Rayman1;
    public override GameCategory Category => GameCategory.Rayman;
    public override bool IsDemo => true;

    public override LocalizedString DisplayName => new ResourceLocString(nameof(Resources.Rayman1_Demo_19960215_MsDos_Title));
    public override DateTime ReleaseDate => new(1996, 02, 15);

    public override GameIconAsset Icon => GameIconAsset.Rayman1_Demo;
    public override GameBannerAsset Banner => GameBannerAsset.Rayman1;

    #endregion

    #region Private Methods

    private static Task TryFindMountPath(GameInstallation gameInstallation)
    {
        // Set the default mount path if available
        FileSystemPath mountPath = gameInstallation.InstallLocation.Directory + "Disc" + "RAY1DEMO.cue";

        if (mountPath.FileExists)
            gameInstallation.SetValue(GameDataKey.Client_DosBox_MountPath, mountPath);

        return Task.CompletedTask;
    }

    #endregion

    #region Protected Methods

    protected override void RegisterComponents(IGameComponentBuilder builder)
    {
        base.RegisterComponents(builder);

        builder.Register(new GameConfigComponent(x => new Rayman1ConfigViewModel(this, x)));
        builder.Register(new OnGameAddedComponent(TryFindMountPath));
        builder.Register<MsDosGameRequiresDiscComponent>();
        builder.Register<BinaryGameModeComponent>(new Ray1GameModeComponent(Ray1GameMode.Rayman1_PC));
    }

    protected override ProgramInstallationStructure GetStructure() => new DirectoryProgramInstallationStructure(new GameInstallationPath[]
    {
        // Files
        new GameInstallationFilePath("RAYMAN.EXE", GameInstallationPathType.PrimaryExe, required: true),

        // Directories
        new GameInstallationDirectoryPath("PCMAP", GameInstallationPathType.Data, required: true),
    });

    #endregion

    #region Public Methods

    public override IEnumerable<GameAddAction> GetAddActions() => new GameAddAction[]
    {
        new LocateDirectoryGameAddAction(this),
        new DownloadGameAddAction(this, new Uri[]
        {
            new(AppURLs.Games_R1Demo2_Url),
        })
    };

    public override FinderQuery[] GetFinderQueries() => new FinderQuery[]
    {
        new PreviouslyDownloadedGameFinderQuery(GameId, LegacyGameId),
    };

    #endregion
}