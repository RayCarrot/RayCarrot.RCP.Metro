using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using BinarySerializer.Ray1;
using RayCarrot.RCP.Metro.Archive;
using RayCarrot.RCP.Metro.Archive.Ray1;

namespace RayCarrot.RCP.Metro;

// TODO-14: Add validation making sure that there is a game mode obj set and its data is valid

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

    public override string DisplayName => "Rayman Edutainment (Quiz)";
    public override string DefaultFileName => $"RAY{PrimaryName}.EXE";

    public override GameIconAsset Icon => GameIconAsset.RaymanEdutainment;

    public override bool AllowPatching => false; // Disable patching since game modes differ between releases
    public override bool HasArchives => true;

    public override string ExecutableName => $"RAY{PrimaryName}.EXE";

    #endregion

    #region Public Methods

    // TODO-14: Add new options control for setting game mode to use when launching the game
    public override FrameworkElement GetOptionsUI(GameInstallation gameInstallation) =>
        new GameOptions_DOSBox_Control(gameInstallation);

    public override GameOptionsDialog_ConfigPageViewModel GetConfigPageViewModel(GameInstallation gameInstallation) => 
        new Config_RaymanEdutainment_ViewModel(this, gameInstallation);

    public override RayMapInfo GetRayMapInfo() => new(RayMapViewer.Ray1Map, "RaymanQuizPC", "r1/quiz/pc_gf", "GF");

    public override IEnumerable<GameProgressionManager> GetGameProgressionManagers(GameInstallation gameInstallation)
    {
        UserData_Ray1MSDOSData data = gameInstallation.GetRequiredObject<UserData_Ray1MSDOSData>(GameDataKey.Ray1MSDOSData);
        return data.AvailableGameModes.Select(x => new GameProgressionManager_RaymanEdutainment(
            gameInstallation: gameInstallation, 
            backupName: $"Educational Games - {x}", 
            primaryName: PrimaryName, 
            secondaryName: x));
    }

    public override IArchiveDataManager GetArchiveDataManager(GameInstallation? gameInstallation) =>
        new Ray1PCArchiveDataManager(new Ray1Settings(Ray1EngineVersion.PC_Edu));

    // TODO-14: Based on the modes also include SNDSMP.DAT, SPECIAL.DAT and VIGNET.DAT
    public override IEnumerable<string> GetArchiveFilePaths(GameInstallation? gameInstallation) => new[]
    {
        @"PCMAP\COMMON.DAT",
        @"PCMAP\SNDD8B.DAT",
        @"PCMAP\SNDH8B.DAT",
    };

    public override async Task PostGameAddAsync(GameInstallation gameInstallation)
    {
        await base.PostGameAddAsync(gameInstallation);

        // Set the game mode data
        gameInstallation.SetObject(GameDataKey.Ray1MSDOSData, UserData_Ray1MSDOSData.Create(gameInstallation));
    }

    public override string GetLaunchArgs(GameInstallation gameInstallation)
    {
        string gameMode = gameInstallation.GetRequiredObject<UserData_Ray1MSDOSData>(GameDataKey.Ray1MSDOSData).SelectedGameMode;
        return $"ver={gameMode}";
    }

    #endregion
}