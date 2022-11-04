using System.Collections.Generic;
using System.Windows;
using BinarySerializer.Ray1;
using RayCarrot.RCP.Metro.Archive;
using RayCarrot.RCP.Metro.Archive.Ray1;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman Designer (MS-DOS) game descriptor
/// </summary>
public sealed class GameDescriptor_RaymanDesigner_MSDOS : MSDOSGameDescriptor
{
    #region Public Properties

    public override string Id => "RaymanDesigner_MSDOS";
    public override Game Game => Game.RaymanDesigner;
    public override GameCategory Category => GameCategory.Rayman;
    public override Games? LegacyGame => Games.RaymanDesigner;
    
    public override string DisplayName => "Rayman Designer";
    public override string BackupName => "Rayman Designer";
    public override string DefaultFileName => "RAYKIT.bat";

    public override string RayMapURL => AppURLs.GetRay1MapGameURL("RaymanDesignerPC", "r1/pc_kit");

    public override bool HasArchives => true;

    public override string ExecutableName => "RAYKIT.EXE";
    public override string RaymanForeverFolderName => "RayKit";

    #endregion

    #region Public Methods

    public override FrameworkElement GetOptionsUI(GameInstallation gameInstallation) =>
        new GameOptions_DOSBox_Control(gameInstallation);

    public override GameOptionsDialog_ConfigPageViewModel GetConfigPageViewModel(GameInstallation gameInstallation) => 
        new Config_RaymanDesigner_ViewModel(gameInstallation);

    public override IEnumerable<ProgressionGameViewModel> GetProgressionGameViewModels(GameInstallation gameInstallation) => 
        new ProgressionGameViewModel_RaymanDesigner(gameInstallation).Yield();

    public override IEnumerable<GameFileLink> GetGameFileLinks(GameInstallation gameInstallation) => new GameFileLink[]
    {
        new(Resources.GameLink_RDMapper, gameInstallation.InstallLocation + "MAPPER.EXE")
    };

    public override IArchiveDataManager GetArchiveDataManager(GameInstallation? gameInstallation) => 
        new Ray1PCArchiveDataManager(new Ray1Settings(Ray1EngineVersion.PC_Kit));

    public override FileSystemPath[] GetArchiveFilePaths(FileSystemPath installDir) => 
        Ray1PCArchiveDataManager.GetArchiveFiles(installDir);

    public override IEnumerable<Utility> GetUtilities(GameInstallation gameInstallation) => new Utility[]
    {
        new Utility_RaymanDesigner_ReplaceFiles(gameInstallation),
        new Utility_RaymanDesigner_CreateConfig(gameInstallation),
    };

    public override IEnumerable<GamePurchaseLink> GetGamePurchaseLinks() => new GamePurchaseLink[]
    {
        new(Resources.GameDisplay_PurchaseGOG, "https://www.gog.com/game/rayman_forever"),
        new(Resources.GameDisplay_PurchaseUplay, "https://store.ubi.com/eu/rayman--forever/5800d3fc4e016524248b4567.html")
    };

    #endregion
}