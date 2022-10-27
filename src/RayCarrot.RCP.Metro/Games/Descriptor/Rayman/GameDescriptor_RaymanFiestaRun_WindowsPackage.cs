#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace RayCarrot.RCP.Metro;

// TODO-14: Make a descriptor for each edition

/// <summary>
/// The Rayman Fiesta Run game descriptor
/// </summary>
public sealed class GameDescriptor_RaymanFiestaRun_WindowsPackage : WindowsPackageGameDescriptor
{
    #region Public Override Properties

    public override string Id => "RaymanFiestaRun_WindowsPackage";
    public override Game Game => Game.RaymanFiestaRun;

    /// <summary>
    /// The game
    /// </summary>
    public override Games LegacyGame => Games.RaymanFiestaRun;

    /// <summary>
    /// The category for the game
    /// </summary>
    public override GameCategory Category => GameCategory.Rayman;

    /// <summary>
    /// The game display name
    /// </summary>
    public override string DisplayName => "Rayman Fiesta Run";

    /// <summary>
    /// The game backup name
    /// </summary>
    public override string BackupName => $"Rayman Fiesta Run ({Services.Data.Game_FiestaRunVersion})";

    /// <summary>
    /// Gets the default file name for launching the game, if available
    /// </summary>
    public override string DefaultFileName => GetFiestaRunFileName(Services.Data.Game_FiestaRunVersion);

    public override FrameworkElement GetOptionsUI(GameInstallation gameInstallation) => new GameOptions_FiestaRun_Control();

    public override GameOptionsDialog_ConfigPageViewModel GetConfigPageViewModel(GameInstallation gameInstallation) => 
        new Config_RaymanFiestaRun_ViewModel(gameInstallation);

    public override IEnumerable<ProgressionGameViewModel> GetProgressionGameViewModels(GameInstallation gameInstallation)
    {
        var manager = this.GetLegacyManager<GameManager_RaymanFiestaRun_WinStore>();

        // Get every installed version
        IEnumerable<UserData_FiestaRunEdition> versions = EnumHelpers.GetValues<UserData_FiestaRunEdition>().Where(x => manager.GetGamePackage(manager.GetFiestaRunPackageName(x)) != null);

        return versions.Select(x => new ProgressionGameViewModel_RaymanFiestaRun(gameInstallation, x, $"{Games.RaymanFiestaRun.GetGameDescriptor().DisplayName} {manager.GetFiestaRunEditionDisplayName(x)}"));
    }

    public override bool AllowPatching => false;

    public override IEnumerable<OverflowButtonItemViewModel> GetAdditionalOverflowButtonItems() => new OverflowButtonItemViewModel[]
    {
        new(Resources.GameDisplay_OpenInWinStore, GenericIconKind.GameDisplay_Microsoft, new AsyncRelayCommand(async () =>
        {
            await Services.File.LaunchURIAsync(MicrosoftStoreHelpers.GetStorePageURI(MicrosoftStoreID));
        })),
    };

    #endregion

    #region Platform

    public override string PackageName => "Ubisoft.RaymanFiestaRun";
    public override string FullPackageName => "Ubisoft.RaymanFiestaRun_ngz4m417e0mpw";

    #endregion

    #region Public Override Methods

    public override IEnumerable<Utility> GetUtilities(GameInstallation gameInstallation) => new Utility[]
    {
        new Utility_RaymanFiestaRun_SaveFix(gameInstallation),
    };

    #endregion

    #region Public Methods

    /// <summary>
    /// Gets the Fiesta Run default file name based on version
    /// </summary>
    /// <param name="version">The version</param>
    /// <returns>The file name</returns>
    public string GetFiestaRunFileName(UserData_FiestaRunEdition version)
    {
        return version switch
        {
            UserData_FiestaRunEdition.Default => "RFR_WinRT.exe",
            UserData_FiestaRunEdition.Preload => "RFR_WinRT_OEM.exe",
            UserData_FiestaRunEdition.Win10 => "RFRXAML.exe",

            _ => throw new ArgumentOutOfRangeException()
        };
    }

    #endregion

    #region Other

    private const string MicrosoftStoreID = "9wzdncrdds0c";

    #endregion
}