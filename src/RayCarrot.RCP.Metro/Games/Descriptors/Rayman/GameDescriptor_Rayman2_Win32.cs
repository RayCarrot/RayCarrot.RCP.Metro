﻿using BinarySerializer.OpenSpace;
using RayCarrot.RCP.Metro.Archive;
using RayCarrot.RCP.Metro.Archive.CPA;
using RayCarrot.RCP.Metro.Games.Components;
using RayCarrot.RCP.Metro.Games.OptionsDialog;
using RayCarrot.RCP.Metro.Ini;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman 2 (Win32) game descriptor
/// </summary>
public sealed class GameDescriptor_Rayman2_Win32 : Win32GameDescriptor
{
    #region Public Properties

    public override string GameId => "Rayman2_Win32";
    public override Game Game => Game.Rayman2;
    public override GameCategory Category => GameCategory.Rayman;
    public override LegacyGame? LegacyGame => Metro.LegacyGame.Rayman2;

    public override LocalizedString DisplayName => "Rayman 2";
    public override string DefaultFileName => "Rayman2.exe";
    public override DateTime ReleaseDate => new(1999, 11, 05);

    public override GameIconAsset Icon => GameIconAsset.Rayman2;
    public override GameBannerAsset Banner => GameBannerAsset.Rayman2;

    public override IEnumerable<string> DialogGroupNames => new string[]
    {
        UbiIniFileGroupName
    };

    public override bool HasArchives => true;

    #endregion

    #region Protected Methods

    protected override void RegisterComponents(IGameComponentBuilder builder)
    {
        base.RegisterComponents(builder);

        builder.Register(new ProgressionManagersComponent(x => new GameProgressionManager_Rayman2(x, "Rayman 2")));
        builder.Register(new GameConfigComponent(x => new Rayman2ConfigViewModel(x)));
        builder.Register<OnGameAddedComponent, AddToJumpListOnGameAddedComponent>();

        builder.Register(new UtilityComponent(x => new Utility_CPATextureSync(x, CPATextureSyncData.FromGameMode(CPAGameMode.Rayman2_PC))));
    }

    #endregion

    #region Public Methods

    public override IEnumerable<GameAddAction> GetAddActions() => base.GetAddActions().Concat(new GameAddAction[]
    {
        new DiscInstallGameAddAction(this, new GameInstallerInfo(
            discFilesListFileName: "Rayman2",
            gameLogo: GameLogoAsset.Rayman2,
            gifFileNames: new[] { "ASTRO.gif", "CASK.gif", "CHASE.gif", "GLOB.gif", "RODEO.gif", },
            installFolderName: "Rayman 2"))
    });

    public override IEnumerable<GameUriLink> GetLocalUriLinks(GameInstallation gameInstallation) => new GameUriLink[]
    {
        new(new ResourceLocString(nameof(Resources.GameLink_Setup)), gameInstallation.InstallLocation + "GXSetup.exe"),
        new(new ResourceLocString(nameof(Resources.GameLink_R2nGlide)), gameInstallation.InstallLocation + "nglide_config.exe"),
        new(new ResourceLocString(nameof(Resources.GameLink_R2dgVoodoo)), gameInstallation.InstallLocation + "dgVoodooCpl.exe"),
        new(new ResourceLocString(nameof(Resources.GameLink_R2Fix)), gameInstallation.InstallLocation + "R2FixCfg.exe"),
    };

    public override RayMapInfo GetRayMapInfo() => new(RayMapViewer.RayMap, "r2_pc", "r2_pc");

    public override IArchiveDataManager GetArchiveDataManager(GameInstallation? gameInstallation) =>
        new CPACntArchiveDataManager(
            settings: new OpenSpaceSettings(EngineVersion.Rayman2, BinarySerializer.OpenSpace.Platform.PC),
            gameInstallation: gameInstallation,
            cpaTextureSyncData: CPATextureSyncData.FromGameMode(CPAGameMode.Rayman2_PC));

    public override IEnumerable<string> GetArchiveFilePaths(GameInstallation? gameInstallation) => new[]
    {
        @"Data\Textures.cnt",
        @"Data\Vignette.cnt",
    };

    public override IEnumerable<GamePurchaseLink> GetPurchaseLinks() => new GamePurchaseLink[]
    {
        new(new ResourceLocString(nameof(Resources.GameDisplay_PurchaseGOG)), "https://www.gog.com/game/rayman_2_the_great_escape"),
        new(new ResourceLocString(nameof(Resources.GameDisplay_PurchaseUplay)), "https://store.ubi.com/eu/rayman-2--the-great-escape/56c4947e88a7e300458b465c.html")
    };

    public override async Task<IList<string>> GetAppliedUtilitiesAsync(GameInstallation gameInstallation)
    {
        // TODO-14: These are not utilities - these are game modifications caused by the config

        // Create the output
        var output = new List<string>();

        if (await Rayman2ConfigViewModel.GetIsWidescreenHackAppliedAsync(gameInstallation) == true)
            output.Add(Resources.Config_WidescreenSupport);

        var dinput = Rayman2ConfigViewModel.GetCurrentDinput(gameInstallation);

        if (dinput == Rayman2ConfigViewModel.R2Dinput.Controller)
            output.Add(Resources.Config_UseController);

        if (dinput == Rayman2ConfigViewModel.R2Dinput.Mapping)
            output.Add(Resources.Config_ButtonMapping);

        // Get other utilities
        output.AddRange(await base.GetAppliedUtilitiesAsync(gameInstallation));

        return output;
    }

    public override GameFinder_GameItem GetGameFinderItem() => new(UbiIniData_Rayman2.SectionName, "Rayman 2", new[]
    {
        "Rayman 2",
        "Rayman: 2",
        "Rayman 2 - The Great Escape",
        "Rayman: 2 - The Great Escape",
        "GOG.com Rayman 2",
    });

    #endregion
}