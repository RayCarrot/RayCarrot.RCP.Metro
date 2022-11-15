using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinarySerializer.OpenSpace;
using RayCarrot.RCP.Metro.Archive;
using RayCarrot.RCP.Metro.Archive.CPA;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman 2 (Steam) game descriptor
/// </summary>
public sealed class GameDescriptor_Rayman2_Steam : SteamGameDescriptor
{
    #region Public Properties

    public override string Id => "Rayman2_Steam";
    public override Game Game => Game.Rayman2;
    public override GameCategory Category => GameCategory.Rayman;
    public override Games? LegacyGame => Games.Rayman2;

    public override string DisplayName => "Rayman 2";
    public override string BackupName => "Rayman 2";
    public override string DefaultFileName => "Rayman2.exe";

    public override GameBanner Banner => GameBanner.Rayman2;

    public override IEnumerable<string> DialogGroupNames => new[] { UbiIniFileGroupName };

    public override bool HasArchives => true;

    public override string SteamID => "15060";

    #endregion

    #region Public Methods

    public override GameOptionsDialog_ConfigPageViewModel GetConfigPageViewModel(GameInstallation gameInstallation) =>
        new Config_Rayman2_ViewModel(gameInstallation);

    public override GameProgressionManager GetGameProgressionManager(GameInstallation gameInstallation) =>
        new GameProgressionManager_Rayman2(gameInstallation);

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

    public override IEnumerable<GamePurchaseLink> GetGamePurchaseLinks() =>
        // No longer available for purchase
        Enumerable.Empty<GamePurchaseLink>();

    public override IEnumerable<Utility> GetUtilities(GameInstallation gameInstallation) => new Utility[]
    {
        new Utility_CPATextureSync(gameInstallation, CPATextureSyncData.FromGameMode(CPAGameMode.Rayman2_PC)),
    };

    public override async Task<IList<string>> GetAppliedUtilitiesAsync(GameInstallation gameInstallation)
    {
        // TODO-14: These are not utilities - these are game modifications caused by the config

        // Create the output
        var output = new List<string>();

        if (await Config_Rayman2_ViewModel.GetIsWidescreenHackAppliedAsync(gameInstallation) == true)
            output.Add(Resources.Config_WidescreenSupport);

        var dinput = Config_Rayman2_ViewModel.GetCurrentDinput(gameInstallation);

        if (dinput == Config_Rayman2_ViewModel.R2Dinput.Controller)
            output.Add(Resources.Config_UseController);

        if (dinput == Config_Rayman2_ViewModel.R2Dinput.Mapping)
            output.Add(Resources.Config_ButtonMapping);

        // Get other utilities
        output.AddRange(await base.GetAppliedUtilitiesAsync(gameInstallation));

        return output;
    }

    #endregion
}