using RayCarrot.RCP.Metro.Games.Components;
using RayCarrot.RCP.Metro.Games.Finder;
using RayCarrot.RCP.Metro.Games.Structure;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman 1 Demo 1995/12/07 (MS-DOS) game descriptor
/// </summary>
public sealed class GameDescriptor_Rayman1_Demo_19951207_MsDos : MsDosGameDescriptor
{
    #region Public Properties

    public override string GameId => "Rayman1_Demo_19951207_MsDos";
    public override string LegacyGameId => "Demo_Rayman1_1";
    public override Game Game => Game.Rayman1;
    public override GameCategory Category => GameCategory.Rayman;
    public override GameType Type => GameType.Demo;

    public override LocalizedString DisplayName => new ResourceLocString(nameof(Resources.Rayman1_Demo_19951207_MsDos_Title));
    public override DateTime ReleaseDate => new(1995, 12, 07);

    public override GameIconAsset Icon => GameIconAsset.Rayman1_Demo;
    public override GameBannerAsset Banner => GameBannerAsset.Rayman1;

    #endregion

    #region Private Methods

    private static Task TryFindMountPath(GameInstallation gameInstallation)
    {
        // Set the default mount path if available
        FileSystemPath mountPath = gameInstallation.InstallLocation.Directory + "Disc" + "RAYMAN_PC_DEMO.cue";

        if (mountPath.FileExists)
            gameInstallation.SetValue(GameDataKey.Client_DosBox_MountPath, mountPath);

        return Task.CompletedTask;
    }

    #endregion

    #region Protected Methods

    protected override void RegisterComponents(IGameComponentBuilder builder)
    {
        base.RegisterComponents(builder);

        builder.Register(new OnGameAddedComponent(TryFindMountPath));
        builder.Register<BinaryGameModeComponent>(new Ray1GameModeComponent(Ray1GameMode.Rayman1_PC));
    }

    protected override ProgramInstallationStructure CreateStructure() => new DirectoryProgramInstallationStructure(new ProgramFileSystem(new ProgramPath[]
    {
        // Files
        new ProgramFilePath("RAYMAN.EXE", ProgramPathType.PrimaryExe, required: true),

        // Directories
        new ProgramDirectoryPath("PCMAP", ProgramPathType.Data, required: true),
    }));

    #endregion

    #region Public Methods

    public override IEnumerable<GameAddAction> GetAddActions() => new GameAddAction[]
    {
        new LocateDirectoryGameAddAction(this),
    };

    public override FinderQuery[] GetFinderQueries() => new FinderQuery[]
    {
        new PreviouslyDownloadedGameFinderQuery(GameId, LegacyGameId),
    };

    #endregion
}