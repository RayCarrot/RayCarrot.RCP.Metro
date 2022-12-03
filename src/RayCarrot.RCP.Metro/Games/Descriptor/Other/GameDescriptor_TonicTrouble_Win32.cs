using System.Collections.Generic;
using BinarySerializer.OpenSpace;
using RayCarrot.RCP.Metro.Archive;
using RayCarrot.RCP.Metro.Archive.CPA;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Tonic Trouble (Win32) game descriptor
/// </summary>
public sealed class GameDescriptor_TonicTrouble_Win32 : Win32GameDescriptor
{
    #region Public Override Properties

    public override string GameId => "TonicTrouble_Win32";
    public override Game Game => Game.TonicTrouble;
    public override GameCategory Category => GameCategory.Other;
    public override Games? LegacyGame => Games.TonicTrouble;

    public override string DisplayName => "Tonic Trouble";
    public override string DefaultFileName => "TonicTrouble.exe";

    public override GameIconAsset Icon => GameIconAsset.TonicTrouble;

    public override bool HasArchives => true;

    #endregion

    #region Protected Methods

    protected override string GetLaunchArgs(GameInstallation gameInstallation) => "-cdrom:";

    #endregion

    #region Public Methods

    public override IEnumerable<GameProgressionManager> GetGameProgressionManagers(GameInstallation gameInstallation) =>
        new GameProgressionManager_TonicTrouble(gameInstallation, "Tonic Trouble").Yield();

    public override IArchiveDataManager GetArchiveDataManager(GameInstallation? gameInstallation) =>
        new CPACntArchiveDataManager(
            settings: new OpenSpaceSettings(EngineVersion.TonicTrouble, BinarySerializer.OpenSpace.Platform.PC), 
            gameInstallation: gameInstallation, 
            cpaTextureSyncData: CPATextureSyncData.FromGameMode(CPAGameMode.TonicTrouble_PC));

    public override IEnumerable<GameUriLink> GetLocalUriLinks(GameInstallation gameInstallation) => new GameUriLink[]
    {
        new(new ResourceLocString(nameof(Resources.GameLink_R2dgVoodoo)), gameInstallation.InstallLocation + "dgVoodooCpl.exe"),
    };

    public override RayMapInfo GetRayMapInfo() => new(RayMapViewer.RayMap, "tt_pc", "tt_pc");

    public override IEnumerable<string> GetArchiveFilePaths(GameInstallation? gameInstallation) => new[]
    {
        @"gamedata\Textures.cnt",
        @"gamedata\Vignette.cnt",
    };

    public override IEnumerable<Utility> GetUtilities(GameInstallation gameInstallation) => new Utility[]
    {
        new Utility_CPATextureSync(gameInstallation, CPATextureSyncData.FromGameMode(CPAGameMode.TonicTrouble_PC)),
    };

    public override GameFinder_GameItem GetGameFinderItem() => new("TONICT", "Tonic Trouble", new[]
    {
        "Tonic Trouble",
    });

    #endregion
}