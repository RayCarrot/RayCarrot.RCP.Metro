using BinarySerializer.OpenSpace;
using RayCarrot.RCP.Metro.Archive;
using RayCarrot.RCP.Metro.Archive.CPA;
using RayCarrot.RCP.Metro.Games.Components;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Tonic Trouble Special Edition (Win32) game descriptor
/// </summary>
public sealed class GameDescriptor_TonicTroubleSpecialEdition_Win32 : Win32GameDescriptor
{
    #region Public Override Properties

    public override string GameId => "TonicTroubleSpecialEdition_Win32";
    public override Game Game => Game.TonicTroubleSpecialEdition;
    public override GameCategory Category => GameCategory.Other;
    public override LegacyGame? LegacyGame => Metro.LegacyGame.TonicTroubleSpecialEdition;

    public override LocalizedString DisplayName => "Tonic Trouble Special Edition";
    public override string DefaultFileName => "MaiD3Dvr.exe";
    public override DateTime ReleaseDate => new(1998, 01, 01); // Not exact

    public override GameIconAsset Icon => GameIconAsset.TonicTroubleSpecialEdition;

    public override bool HasArchives => true;

    #endregion

    #region Private Methods

    private static string GetLaunchArgs(GameInstallation gameInstallation) => "-cdrom:";

    private static IEnumerable<GameLinksComponent.GameUriLink> GetLocalGameLinks(GameInstallation gameInstallation) => new[]
    {
        new GameLinksComponent.GameUriLink(
            Header: new ResourceLocString(nameof(Resources.GameLink_R2dgVoodoo)), 
            Uri: gameInstallation.InstallLocation + "dgVoodooCpl.exe"),
    };

    #endregion

    #region Protected Methods

    protected override void RegisterComponents(IGameComponentBuilder builder)
    {
        base.RegisterComponents(builder);

        builder.Register(new ProgressionManagersComponent(x => new GameProgressionManager_TonicTrouble(x, "Tonic Trouble Special Edition")));
        builder.Register<OnGameAddedComponent, AddToJumpListOnGameAddedComponent>();
        builder.Register(new LaunchArgumentsComponent(GetLaunchArgs));
        builder.Register(new LocalGameLinksComponent(GetLocalGameLinks));

        builder.Register(new UtilityComponent(x => new Utility_CPATextureSync(x, CPATextureSyncData.FromGameMode(CPAGameMode.TonicTrouble_SE_PC))));
    }

    #endregion

    #region Public Methods

    public override RayMapInfo GetRayMapInfo() => new(RayMapViewer.RayMap, "ttse_pc", "ttse_pc");

    public override IArchiveDataManager GetArchiveDataManager(GameInstallation? gameInstallation) =>
        new CPACntArchiveDataManager(
            settings: new OpenSpaceSettings(EngineVersion.TonicTroubleSpecialEdition, BinarySerializer.OpenSpace.Platform.PC), 
            gameInstallation: gameInstallation, 
            cpaTextureSyncData: CPATextureSyncData.FromGameMode(CPAGameMode.TonicTrouble_SE_PC));

    public override IEnumerable<string> GetArchiveFilePaths(GameInstallation? gameInstallation) => new[]
    {
        @"GameData\Textures.cnt",
        @"GameData\Vignette.cnt",
    };

    public override GameFinder_GameItem GetGameFinderItem() => new("TONICT", "Tonic Trouble", new[]
    {
        "Tonic Trouble",
    });

    #endregion
}