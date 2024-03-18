using RayCarrot.RCP.Metro.Archive.CPA;
using RayCarrot.RCP.Metro.Games.Components;
using RayCarrot.RCP.Metro.Games.Finder;
using RayCarrot.RCP.Metro.Games.OptionsDialog;
using RayCarrot.RCP.Metro.Games.Structure;
using RayCarrot.RCP.Metro.Games.Tools.RuntimeModifications;
using RayCarrot.RCP.Metro.Ini;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman 3 (Win32) game descriptor
/// </summary>
public sealed class GameDescriptor_Rayman3_Win32 : Win32GameDescriptor
{
    #region Private Constant Fields

    private const string UbisoftConnectGameId = "360";
    private const string UbisoftConnectProductId = "5800b15eef3aa5ab3e8b4567";

    #endregion

    #region Public Properties

    public override string GameId => "Rayman3_Win32";
    public override string LegacyGameId => "Rayman3";
    public override Game Game => Game.Rayman3;
    public override GameCategory Category => GameCategory.Rayman;

    public override LocalizedString DisplayName => new ResourceLocString(nameof(Resources.Rayman3_Win32_Title));
    public override string[] SearchKeywords => new[] { "r3" };
    public override DateTime ReleaseDate => new(2003, 03, 18);

    public override GameIconAsset Icon => GameIconAsset.Rayman3;
    public override GameBannerAsset Banner => GameBannerAsset.Rayman3;

    #endregion

    #region Protected Methods

    protected override void RegisterComponents(IGameComponentBuilder builder)
    {
        base.RegisterComponents(builder);

        builder.Register(new UbisoftConnectGameClientComponent(UbisoftConnectGameId, UbisoftConnectProductId));

        builder.Register(new ProgressionManagersComponent(x => new GameProgressionManager_Rayman3_Win32(x, "Rayman 3")));
        builder.Register(new GameConfigComponent(x => new Rayman3ConfigViewModel(x)));
        builder.Register<OnGameAddedComponent, AddToJumpListOnGameAddedComponent>();
        builder.Register<LocalGameLinksComponent>(new Rayman3SetupLocalGameLinksComponent(false));
        builder.Register(new RayMapComponent(RayMapComponent.RayMapViewer.RayMap, "r3_pc", "r3_pc"));
        builder.Register<BinaryGameModeComponent>(new CPAGameModeComponent(CPAGameMode.Rayman3_PC));
        builder.Register<ArchiveComponent>(new CPAArchiveComponent(_ => new[]
        {
            @"Gamedatabin\tex32_1.cnt",
            @"Gamedatabin\tex32_2.cnt",
            @"Gamedatabin\vignette.cnt",
        }));
        builder.Register<GameOptionsDialogGroupNameComponent, UbiIniGameOptionsDialogGroupNameComponent>();
        builder.Register(new CPATextureSyncComponent(
            new CPATextureSyncDataItem(
                Name: "Gamedatabin",
                Archives: new[] { "tex32_1.cnt", "tex32_2.cnt", "vignette.cnt" })));

        builder.Register(new GameBananaGameComponent(6188));

        builder.Register(new RuntimeModificationsGameManagersComponent(EmulatedPlatform.None, _ =>
            new[]
            {
                new CpaGameManager(
                    displayName: new ResourceLocString(nameof(Resources.Mod_Mem_Game_R3_PC)),
                    getOffsetsFunc: () => CPAMemoryData.Offsets_R3_PC)
            }));

        builder.Register(new UtilityComponent(x => new Utility_Rayman3_DirectPlay(x)));
    }

    protected override ProgramInstallationStructure GetStructure() => new DirectoryProgramInstallationStructure(new GameInstallationPath[]
    {
        // Files
        new GameInstallationFilePath("Rayman3.exe", GameInstallationPathType.PrimaryExe, required: true),

        // Directories
        new GameInstallationDirectoryPath("Gamedatabin", GameInstallationPathType.Data, required: true),
        new GameInstallationDirectoryPath("GAMEDATA", GameInstallationPathType.Save),
    });

    #endregion

    #region Public Methods

    public override IEnumerable<GameAddAction> GetAddActions() => new GameAddAction[]
    {
        new LocateDirectoryGameAddAction(this),
    };

    public override IEnumerable<GamePurchaseLink> GetPurchaseLinks() => new GamePurchaseLink[]
    {
        new(new ResourceLocString(nameof(Resources.GameDisplay_PurchaseGOG)), "https://www.gog.com/game/rayman_3_hoodlum_havoc"),
        new(new ResourceLocString(nameof(Resources.GameDisplay_PurchaseUplay)), UbisoftConnectHelpers.GetStorePageURL(UbisoftConnectProductId)),
    };

    public override FinderQuery[] GetFinderQueries() => new FinderQuery[]
    {
        new UbiIniFinderQuery(UbiIniData_Rayman3.SectionName),

        new UninstallProgramFinderQuery("Rayman 3"),
        new UninstallProgramFinderQuery("Rayman: 3"),
        new UninstallProgramFinderQuery("Rayman 3 - Hoodlum Havoc"),
        new UninstallProgramFinderQuery("Rayman: 3 - Hoodlum Havoc"),

        new Win32ShortcutFinderQuery("Rayman 3"),
    };

    #endregion
}