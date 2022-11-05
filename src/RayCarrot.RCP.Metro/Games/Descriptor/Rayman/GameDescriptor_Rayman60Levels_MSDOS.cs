using System.Collections.Generic;
using System.Windows;
using BinarySerializer.Ray1;
using RayCarrot.RCP.Metro.Archive;
using RayCarrot.RCP.Metro.Archive.Ray1;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman 60 Levels (MS-DOS) game descriptor
/// </summary>
public sealed class GameDescriptor_Rayman60Levels_MSDOS : MSDOSGameDescriptor
{
    #region Public Properties

    public override string Id => "Rayman60Levels_MSDOS";
    public override Game Game => Game.Rayman60Levels;
    public override GameCategory Category => GameCategory.Rayman;
    public override Games? LegacyGame => Games.Rayman60Levels;

    public override string DisplayName => "Rayman 60 Levels";
    public override string BackupName => "Rayman 60 Levels";
    public override string DefaultFileName => "Rayman.bat";

    public override string RayMapURL => AppURLs.GetRay1MapGameURL("Rayman60LevelsPC", "r1/pc_60n");

    public override bool HasArchives => true;

    public override string ExecutableName => "RAYPLUS.EXE";

    #endregion

    #region Public Methods

    public override FrameworkElement GetOptionsUI(GameInstallation gameInstallation) =>
        new GameOptions_DOSBox_Control(gameInstallation);

    public override GameOptionsDialog_ConfigPageViewModel GetConfigPageViewModel(GameInstallation gameInstallation) => 
        new Config_RaymanByHisFans_ViewModel(gameInstallation);

    public override IEnumerable<ProgressionGameViewModel> GetProgressionGameViewModels(GameInstallation gameInstallation) => 
        new ProgressionGameViewModel_Rayman60Levels(gameInstallation).Yield();

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