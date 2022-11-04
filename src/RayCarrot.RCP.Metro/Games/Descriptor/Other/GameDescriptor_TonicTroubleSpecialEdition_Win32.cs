using System.Collections.Generic;
using BinarySerializer.OpenSpace;
using RayCarrot.RCP.Metro.Archive;
using RayCarrot.RCP.Metro.Archive.CPA;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Tonic Trouble Special Edition (Win32) game descriptor
/// </summary>
public sealed class GameDescriptor_TonicTroubleSpecialEdition_Win32 : Win32GameDescriptor
{
    #region Public Override Properties

    public override string Id => "TonicTroubleSpecialEdition_Win32";
    public override Game Game => Game.TonicTroubleSpecialEdition;
    public override GameCategory Category => GameCategory.Other;
    public override Games LegacyGame => Games.TonicTroubleSpecialEdition;

    public override string DisplayName => "Tonic Trouble Special Edition";
    public override string BackupName => "Tonic Trouble Special Edition";
    public override string DefaultFileName => "MaiD3Dvr.exe";

    public override string RayMapURL => AppURLs.GetRayMapGameURL("ttse_pc", "ttse_pc");

    public override bool HasArchives => true;

    #endregion

    #region Protected Methods

    protected override string GetLaunchArgs(GameInstallation gameInstallation) => "-cdrom:";

    #endregion

    #region Public Methods

    public override IEnumerable<ProgressionGameViewModel> GetProgressionGameViewModels(GameInstallation gameInstallation) =>
        new ProgressionGameViewModel_TonicTrouble(gameInstallation).Yield();

    public override IEnumerable<GameFileLink> GetGameFileLinks(GameInstallation gameInstallation) => new GameFileLink[]
    {
        new(Resources.GameLink_R2dgVoodoo, gameInstallation.InstallLocation + "dgVoodooCpl.exe"),
    };

    public override IArchiveDataManager GetArchiveDataManager(GameInstallation? gameInstallation) =>
        new CPACntArchiveDataManager(
            settings: new OpenSpaceSettings(EngineVersion.TonicTroubleSpecialEdition, BinarySerializer.OpenSpace.Platform.PC), 
            gameInstallation: gameInstallation, 
            cpaTextureSyncData: CPATextureSyncData.FromGameMode(CPAGameMode.TonicTrouble_SE_PC));

    public override FileSystemPath[] GetArchiveFilePaths(FileSystemPath installDir) => new[]
    {
        installDir + "GameData" + "Textures.cnt",
        installDir + "GameData" + "Vignette.cnt",
    };

    public override IEnumerable<Utility> GetUtilities(GameInstallation gameInstallation) => new Utility[]
    {
        new Utility_CPATextureSync(gameInstallation, CPATextureSyncData.FromGameMode(CPAGameMode.TonicTrouble_SE_PC)),
    };

    public override GameFinder_GameItem GetGameFinderItem() => new("TONICT", "Tonic Trouble", new[]
    {
        "Tonic Trouble",
    });

    #endregion
}