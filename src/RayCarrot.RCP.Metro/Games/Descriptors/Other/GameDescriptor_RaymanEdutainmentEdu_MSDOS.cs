using BinarySerializer.Ray1;
using RayCarrot.RCP.Metro.Archive;
using RayCarrot.RCP.Metro.Archive.Ray1;
using RayCarrot.RCP.Metro.Games.Components;
using RayCarrot.RCP.Metro.Games.Emulators;
using RayCarrot.RCP.Metro.Games.Emulators.DosBox;
using RayCarrot.RCP.Metro.Games.OptionsDialog;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman Edutainment Edu (MS-DOS) game descriptor
/// </summary>
public sealed class GameDescriptor_RaymanEdutainmentEdu_MSDOS : MsDosGameDescriptor
{
    #region Constant Fields

    private const string PrimaryName = "EDU";

    #endregion

    #region Public Properties

    public override string GameId => "RaymanEdutainmentEdu_MSDOS";
    public override Game Game => Game.RaymanEdutainment;
    public override GameCategory Category => GameCategory.Other;
    public override LegacyGame? LegacyGame => Metro.LegacyGame.EducationalDos;

    public override LocalizedString DisplayName => "Rayman Edutainment (Edu)";
    public override string DefaultFileName => $"RAY{PrimaryName}.EXE";
    public override DateTime ReleaseDate => new(1996, 01, 01); // Not exact

    public override GameIconAsset Icon => GameIconAsset.RaymanEdutainment;

    public override bool AllowPatching => false; // Disable patching since game modes differ between releases
    public override bool HasArchives => true;

    public override string ExecutableName => $"RAY{PrimaryName}.EXE";

    #endregion

    #region Private Methods

    private IEnumerable<ActionItemViewModel> GetAdditionalLaunchActions(GameInstallation gameInstallation)
    {
        // Add a lunch action for each game mode
        string[] gameModes = gameInstallation.GetRequiredObject<UserData_Ray1MsDosData>(GameDataKey.Ray1_MsDosData).AvailableGameModes;

        // Only show additional launch actions for the game if we have more than one game mode
        if (gameModes.Length <= 1)
            return Enumerable.Empty<ActionItemViewModel>();

        return gameModes.Select(x =>
            new IconCommandItemViewModel(
                header: x,
                description: null,
                iconKind: GenericIconKind.GameAction_Play,
                command: new AsyncRelayCommand(async () =>
                {
                    EmulatorInstallation? emulatorInstallation = GetEmulator(gameInstallation);

                    if (emulatorInstallation?.EmulatorDescriptor is not DosBoxEmulatorDescriptor emulatorDescriptor)
                        return;

                    string args = GetLaunchArgs(x);
                    bool success = await emulatorDescriptor.LaunchGameAsync(gameInstallation, emulatorInstallation, args);

                    if (success)
                        await GetComponents<OnGameLaunchedComponent>().InvokeAllAsync(gameInstallation);
                })));
    }

    private IEnumerable<GameProgressionManager> GetGameProgressionManagers(GameInstallation gameInstallation)
    {
        UserData_Ray1MsDosData data = gameInstallation.GetRequiredObject<UserData_Ray1MsDosData>(GameDataKey.Ray1_MsDosData);
        return data.AvailableGameModes.Select(x => new GameProgressionManager_RaymanEdutainment(
            gameInstallation: gameInstallation,
            backupName: $"Educational Games - {x}",
            primaryName: PrimaryName,
            secondaryName: x));
    }

    #endregion

    #region Protected Methods

    protected override void RegisterComponents(DescriptorComponentBuilder builder)
    {
        base.RegisterComponents(builder);

        builder.Register(new AdditionalLaunchActionsComponent(GetAdditionalLaunchActions));
        builder.Register(new ProgressionManagersComponent(GetGameProgressionManagers));
        builder.Register<GameValidationCheckComponent, Ray1MsDosGameDataGameValidationCheckComponent>();
        builder.Register(new GameConfigComponent(x => new RaymanEdutainmentConfigViewModel(this, x)));
        builder.Register<OnGameAddedComponent, SetRay1MsDosDataOnGameAddedComponent>();
    }

    #endregion

    #region Public Methods

    public override RayMapInfo GetRayMapInfo() => new(RayMapViewer.Ray1Map, "RaymanEducationalPC", "r1/edu/pc_gb", "GB1");

    public override IArchiveDataManager GetArchiveDataManager(GameInstallation? gameInstallation) =>
        new Ray1PCArchiveDataManager(new Ray1Settings(Ray1EngineVersion.PC_Edu));

    // TODO-14: Based on the modes also include SNDSMP.DAT, SPECIAL.DAT and VIGNET.DAT
    public override IEnumerable<string> GetArchiveFilePaths(GameInstallation? gameInstallation) => new[]
    {
        @"PCMAP\COMMON.DAT",
        @"PCMAP\SNDD8B.DAT",
        @"PCMAP\SNDH8B.DAT",
    };
    
    public override string GetLaunchArgs(GameInstallation gameInstallation)
    {
        string gameMode = gameInstallation.GetRequiredObject<UserData_Ray1MsDosData>(GameDataKey.Ray1_MsDosData).SelectedGameMode;
        return GetLaunchArgs(gameMode);
    }

    public string GetLaunchArgs(string gameMode) => $"ver={gameMode}";

    #endregion
}