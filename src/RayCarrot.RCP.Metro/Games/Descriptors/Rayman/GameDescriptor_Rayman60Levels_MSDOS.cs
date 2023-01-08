using BinarySerializer.Ray1;
using RayCarrot.RCP.Metro.Archive;
using RayCarrot.RCP.Metro.Archive.Ray1;
using RayCarrot.RCP.Metro.Games.Components;
using RayCarrot.RCP.Metro.Games.Options;
using RayCarrot.RCP.Metro.Games.OptionsDialog;
using RayCarrot.RCP.Metro.Games.Structure;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman 60 Levels (MS-DOS) game descriptor
/// </summary>
public sealed class GameDescriptor_Rayman60Levels_MSDOS : MsDosGameDescriptor
{
    #region Public Properties

    public override string GameId => "Rayman60Levels_MSDOS";
    public override Game Game => Game.Rayman60Levels;
    public override GameCategory Category => GameCategory.Rayman;
    public override LegacyGame? LegacyGame => Metro.LegacyGame.Rayman60Levels;

    public override LocalizedString DisplayName => "Rayman 60 Levels";
    public override DateTime ReleaseDate => new(1999, 01, 01); // Not exact

    public override GameIconAsset Icon => GameIconAsset.Rayman60Levels;

    public override bool HasArchives => true;

    #endregion

    #region Protected Methods

    protected override void RegisterComponents(IGameComponentBuilder builder)
    {
        base.RegisterComponents(builder);

        builder.Register(new ProgressionManagersComponent(x => new GameProgressionManager_Rayman60Levels(x, "Rayman 60 Levels")));
        builder.Register<GameValidationCheckComponent, Ray1MsDosGameDataGameValidationCheckComponent>();
        builder.Register(new GameConfigComponent(x => new RaymanByHisFansConfigViewModel(this, x)));
        builder.Register<OnGameAddedComponent, SetRay1MsDosDataOnGameAddedComponent>();
        builder.Register<OnGameAddedComponent, AddToJumpListOnGameAddedComponent>();
        builder.Register<LaunchArgumentsComponent, Ray1LaunchArgumentsComponent>();
        builder.Register<MsDosGameRequiresDiscComponent>();
        builder.Register(new GameOptionsComponent(x => new Ray1MsDosGameOptionsViewModel(x)));
        builder.Register(new RayMapComponent(RayMapComponent.RayMapViewer.Ray1Map, "Rayman60LevelsPC", "r1/pc_60n"));
        builder.Register<BinarySettingsComponent>(new Ray1BinarySettingsComponent(new Ray1Settings(Ray1EngineVersion.PC_Fan)));
    }

    protected override GameInstallationStructure GetStructure() => new(new GameInstallationPath[]
    {
        // Files
        new GameInstallationFilePath("RAYPLUS.EXE", GameInstallationPathType.PrimaryExe, required: true),

        // Directories
        new GameInstallationDirectoryPath("PCMAP", GameInstallationPathType.Data, required: true),
    });

    #endregion

    #region Public Methods

    public override IArchiveDataManager GetArchiveDataManager(GameInstallation? gameInstallation) => 
        new Ray1PCArchiveDataManager(new Ray1Settings(Ray1EngineVersion.PC_Fan));

    public override IEnumerable<string> GetArchiveFilePaths(GameInstallation? gameInstallation) => new[]
    {
        @"PCMAP\COMMON.DAT",
        @"PCMAP\SNDD8B.DAT",
        @"PCMAP\SNDH8B.DAT",
        @"PCMAP\VIGNET.DAT",

        @"PCMAP\AL\SNDSMP.DAT",
        @"PCMAP\AL\SPECIAL.DAT",

        @"PCMAP\FR\SNDSMP.DAT",
        @"PCMAP\FR\SPECIAL.DAT",

        @"PCMAP\USA\SNDSMP.DAT",
        @"PCMAP\USA\SPECIAL.DAT",
    };

    #endregion
}