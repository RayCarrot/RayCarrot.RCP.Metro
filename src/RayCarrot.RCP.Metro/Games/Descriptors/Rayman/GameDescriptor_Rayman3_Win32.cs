using BinarySerializer.OpenSpace;
using RayCarrot.RCP.Metro.Archive;
using RayCarrot.RCP.Metro.Archive.CPA;
using RayCarrot.RCP.Metro.Games.Components;
using RayCarrot.RCP.Metro.Games.OptionsDialog;
using RayCarrot.RCP.Metro.Ini;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman 3 (Win32) game descriptor
/// </summary>
public sealed class GameDescriptor_Rayman3_Win32 : Win32GameDescriptor
{
    #region Public Properties

    public override string GameId => "Rayman3_Win32";
    public override Game Game => Game.Rayman3;
    public override GameCategory Category => GameCategory.Rayman;
    public override LegacyGame? LegacyGame => Metro.LegacyGame.Rayman3;

    public override LocalizedString DisplayName => "Rayman 3";
    public override string DefaultFileName => "Rayman3.exe";
    public override DateTime ReleaseDate => new(2003, 03, 18);

    public override GameIconAsset Icon => GameIconAsset.Rayman3;

    public override IEnumerable<string> DialogGroupNames => new string[]
    {
        UbiIniFileGroupName
    };

    public override bool HasArchives => true;

    #endregion

    #region Protected Methods

    protected override void RegisterComponents(DescriptorComponentBuilder builder)
    {
        base.RegisterComponents(builder);

        builder.Register(new ProgressionManagersComponent(x => new GameProgressionManager_Rayman3(x, "Rayman 3")));
        builder.Register(new GameConfigComponent(x => new Rayman3ConfigViewModel(x)));

        builder.Register(new UtilityComponent(x => new Utility_CPATextureSync(x, CPATextureSyncData.FromGameMode(CPAGameMode.Rayman3_PC))));
        builder.Register(new UtilityComponent(x => new Utility_Rayman3_DirectPlay(x)));
    }

    #endregion

    #region Public Methods

    public override IEnumerable<GameUriLink> GetLocalUriLinks(GameInstallation gameInstallation) => new GameUriLink[]
    {
        new(new ResourceLocString(nameof(Resources.GameLink_Setup)), gameInstallation.InstallLocation + "R3_Setup_DX8.exe")
    };

    public override RayMapInfo GetRayMapInfo() => new(RayMapViewer.RayMap, "r3_pc", "r3_pc");

    public override IArchiveDataManager GetArchiveDataManager(GameInstallation? gameInstallation) => 
        new CPACntArchiveDataManager(
            settings: new OpenSpaceSettings(EngineVersion.Rayman3, BinarySerializer.OpenSpace.Platform.PC), 
            gameInstallation: gameInstallation, 
            cpaTextureSyncData: CPATextureSyncData.FromGameMode(CPAGameMode.Rayman3_PC));

    public override IEnumerable<string> GetArchiveFilePaths(GameInstallation? gameInstallation) => new[]
    {
        @"Gamedatabin\tex32_1.cnt",
        @"Gamedatabin\tex32_2.cnt",
        @"Gamedatabin\vignette.cnt",
    };

    public override IEnumerable<GamePurchaseLink> GetPurchaseLinks() => new GamePurchaseLink[]
    {
        new(new ResourceLocString(nameof(Resources.GameDisplay_PurchaseGOG)), "https://www.gog.com/game/rayman_3_hoodlum_havoc"),
        new(new ResourceLocString(nameof(Resources.GameDisplay_PurchaseUplay)), "https://store.ubi.com/eu/rayman--3--hoodlum-havoc/5800b15eef3aa5ab3e8b4567.html")
    };

    public override GameFinder_GameItem GetGameFinderItem() => new(UbiIniData_Rayman3.SectionName, "Rayman 3", new[]
    {
        "Rayman 3",
        "Rayman: 3",
        "Rayman 3 - Hoodlum Havoc",
        "Rayman: 3 - Hoodlum Havoc",
    });

    #endregion
}