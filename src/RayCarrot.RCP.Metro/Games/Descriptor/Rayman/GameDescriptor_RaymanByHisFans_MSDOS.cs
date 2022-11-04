using System.Collections.Generic;
using System.Windows;
using BinarySerializer.Ray1;
using RayCarrot.RCP.Metro.Archive;
using RayCarrot.RCP.Metro.Archive.Ray1;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman by his Fans (MS-DOS) game descriptor
/// </summary>
public sealed class GameDescriptor_RaymanByHisFans_MSDOS : MSDOSGameDescriptor
{
    #region Public Properties

    public override string Id => "RaymanByHisFans_MSDOS";
    public override Game Game => Game.RaymanByHisFans;
    public override GameCategory Category => GameCategory.Rayman;
    public override Games? LegacyGame => Games.RaymanByHisFans;

    public override string DisplayName => "Rayman by his Fans";
    public override string BackupName => "Rayman by his Fans";
    public override string DefaultFileName => "rayfan.bat";

    public override string RayMapURL => AppURLs.GetRay1MapGameURL("RaymanByHisFansPC", "r1/pc_fan");

    public override bool HasArchives => true;

    public override string ExecutableName => "RAYFAN.EXE";
    public override string RaymanForeverFolderName => "RayFan";

    #endregion

    #region Public Methods

    public override FrameworkElement GetOptionsUI(GameInstallation gameInstallation) =>
        new GameOptions_DOSBox_Control(gameInstallation);

    public override GameOptionsDialog_ConfigPageViewModel GetConfigPageViewModel(GameInstallation gameInstallation) => 
        new Config_RaymanByHisFans_ViewModel(gameInstallation);

    public override IEnumerable<ProgressionGameViewModel> GetProgressionGameViewModels(GameInstallation gameInstallation) => 
        new ProgressionGameViewModel_RaymanByHisFans(gameInstallation).Yield();

    public override IArchiveDataManager GetArchiveDataManager(GameInstallation? gameInstallation) => 
        new Ray1PCArchiveDataManager(new Ray1Settings(Ray1EngineVersion.PC_Fan));

    public override FileSystemPath[] GetArchiveFilePaths(FileSystemPath installDir) => 
        Ray1PCArchiveDataManager.GetArchiveFiles(installDir);

    public override IEnumerable<GamePurchaseLink> GetGamePurchaseLinks() => new GamePurchaseLink[]
    {
        new(Resources.GameDisplay_PurchaseGOG, "https://www.gog.com/game/rayman_forever"),
        new(Resources.GameDisplay_PurchaseUplay, "https://store.ubi.com/eu/rayman--forever/5800d3fc4e016524248b4567.html")
    };

    #endregion
}