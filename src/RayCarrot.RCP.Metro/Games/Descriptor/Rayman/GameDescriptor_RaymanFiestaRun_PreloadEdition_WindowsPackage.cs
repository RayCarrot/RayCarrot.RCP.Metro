﻿using System.Collections.Generic;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman Fiesta Run Preload Edition (Windows Package) game descriptor
/// </summary>
public sealed class GameDescriptor_RaymanFiestaRun_PreloadEdition_WindowsPackage : WindowsPackageGameDescriptor
{
    #region Private Constant Fields

    private const string MicrosoftStoreID = "9wzdncrdcw9b";

    #endregion

    #region Public Properties

    public override string Id => "RaymanFiestaRunPreloadEdition_WindowsPackage";
    public override Game Game => Game.RaymanFiestaRun;
    public override GameCategory Category => GameCategory.Rayman;
    public override Games? LegacyGame => Games.RaymanFiestaRun;

    public override string DisplayName => "Rayman Fiesta Run Preload Edition";
    public override string BackupName => "Rayman Fiesta Run (Preload)";
    public override string DefaultFileName => "RFR_WinRT_OEM.exe";

    public override string PackageName => "UbisoftEntertainment.RaymanFiestaRunPreloadEdition";
    public override string FullPackageName => "UbisoftEntertainment.RaymanFiestaRunPreloadEdition_dbgk1hhpxymar";

    #endregion

    #region Public Methods

    public override GameOptionsDialog_ConfigPageViewModel GetConfigPageViewModel(GameInstallation gameInstallation) => 
        new Config_RaymanFiestaRun_ViewModel(this);

    public override IEnumerable<ProgressionGameViewModel> GetProgressionGameViewModels(GameInstallation gameInstallation) => 
        new ProgressionGameViewModel_RaymanFiestaRun(this, gameInstallation, 1).Yield();

    public override IEnumerable<Utility> GetUtilities(GameInstallation gameInstallation) => new Utility[]
    {
        new Utility_RaymanFiestaRun_SaveFix(this, gameInstallation, 1),
    };

    public override IEnumerable<OverflowButtonItemViewModel> GetAdditionalOverflowButtonItems(GameInstallation gameInstallation) => 
        new OverflowButtonItemViewModel[]
        {
            new(Resources.GameDisplay_OpenInWinStore, GenericIconKind.GameDisplay_Microsoft, new AsyncRelayCommand(async () =>
            {
                await Services.File.LaunchURIAsync(MicrosoftStoreHelpers.GetStorePageURI(MicrosoftStoreID));
            })),
        };

    #endregion
}