using BinarySerializer.Ray1;
using RayCarrot.RCP.Metro.Archive;
using RayCarrot.RCP.Metro.Archive.Ray1;
using RayCarrot.RCP.Metro.Games.Components;
using RayCarrot.RCP.Metro.Games.Data;
using RayCarrot.RCP.Metro.Games.Options;
using RayCarrot.RCP.Metro.Games.OptionsDialog;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman Edutainment Qui (MS-DOS) game descriptor
/// </summary>
public sealed class GameDescriptor_RaymanEdutainmentQuiz_MSDOS : MsDosGameDescriptor
{
    #region Constant Fields

    private const string PrimaryName = "QUI";

    #endregion

    #region Public Properties

    public override string GameId => "RaymanEdutainmentQui_MSDOS";
    public override Game Game => Game.RaymanEdutainment;
    public override GameCategory Category => GameCategory.Other;
    public override LegacyGame? LegacyGame => Metro.LegacyGame.EducationalDos;

    public override LocalizedString DisplayName => "Rayman Edutainment (Quiz)";
    public override string DefaultFileName => $"RAY{PrimaryName}.EXE";
    public override DateTime ReleaseDate => new(1996, 01, 01); // Not exact

    public override GameIconAsset Icon => GameIconAsset.RaymanEdutainment;

    public override bool AllowPatching => false; // Disable patching since game modes differ between releases
    public override bool HasArchives => true;

    #endregion

    #region Private Methods

    private static IEnumerable<GameProgressionManager> GetGameProgressionManagers(GameInstallation gameInstallation)
    {
        Ray1MsDosData data = gameInstallation.GetRequiredObject<Ray1MsDosData>(GameDataKey.Ray1_MsDosData);
        return data.AvailableVersions.Select(x => new GameProgressionManager_RaymanEdutainment(
            gameInstallation: gameInstallation,
            backupName: $"Educational Games - {x.Id}",
            primaryName: PrimaryName,
            version: x));
    }

    #endregion

    #region Protected Methods

    protected override void RegisterComponents(IGameComponentBuilder builder)
    {
        base.RegisterComponents(builder);

        builder.Register<AdditionalLaunchActionsComponent, Ray1MsDosAdditionalLaunchActionsComponent>();
        builder.Register(new ProgressionManagersComponent(GetGameProgressionManagers));
        builder.Register<GameValidationCheckComponent, Ray1MsDosGameDataGameValidationCheckComponent>();
        builder.Register(new GameConfigComponent(x => new RaymanEdutainmentConfigViewModel(this, x)));
        builder.Register<OnGameAddedComponent, SetRay1MsDosDataOnGameAddedComponent>();
        builder.Register<LaunchArgumentsComponent, Ray1LaunchArgumentsComponent>();
        builder.Register<MsDosGameRequiresDiscComponent>();
        builder.Register(new GameOptionsComponent(x => new Ray1MsDosGameOptionsViewModel(x)));
        builder.Register(new RayMapComponent(RayMapComponent.RayMapViewer.Ray1Map, "RaymanQuizPC", "r1/quiz/pc_gf", "GF"));
        builder.Register<BinarySettingsComponent>(new Ray1BinarySettingsComponent(new Ray1Settings(Ray1EngineVersion.PC_Edu)));
    }

    #endregion

    #region Public Methods

    public override IArchiveDataManager GetArchiveDataManager(GameInstallation? gameInstallation) =>
        new Ray1PCArchiveDataManager(new Ray1Settings(Ray1EngineVersion.PC_Edu));

    // TODO-14: Based on the modes also include SNDSMP.DAT, SPECIAL.DAT and VIGNET.DAT
    public override IEnumerable<string> GetArchiveFilePaths(GameInstallation? gameInstallation) => new[]
    {
        @"PCMAP\COMMON.DAT",
        @"PCMAP\SNDD8B.DAT",
        @"PCMAP\SNDH8B.DAT",
    };

    #endregion
}