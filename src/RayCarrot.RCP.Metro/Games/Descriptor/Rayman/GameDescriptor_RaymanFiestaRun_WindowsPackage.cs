using System.Collections.Generic;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman Fiesta Run (Windows Package) game descriptor
/// </summary>
public sealed class GameDescriptor_RaymanFiestaRun_WindowsPackage : WindowsPackageGameDescriptor
{
    #region Private Constant Fields

    private const string MicrosoftStoreID = "9wzdncrdds0c";

    #endregion

    #region Public Properties

    public override string Id => "RaymanFiestaRun_WindowsPackage";
    public override Game Game => Game.RaymanFiestaRun;
    public override GameCategory Category => GameCategory.Rayman;
    public override Games? LegacyGame => Games.RaymanFiestaRun;

    public override string DisplayName => "Rayman Fiesta Run";
    public override string BackupName => "Rayman Fiesta Run (Default)";
    public override string DefaultFileName => "RFR_WinRT.exe";

    public override string PackageName => "Ubisoft.RaymanFiestaRun";
    public override string FullPackageName => "Ubisoft.RaymanFiestaRun_ngz4m417e0mpw";

    #endregion

    #region Public Methods

    public override GameOptionsDialog_ConfigPageViewModel GetConfigPageViewModel(GameInstallation gameInstallation) => 
        new Config_RaymanFiestaRun_ViewModel(this);

    public override GameProgressionManager GetGameProgressionManager(GameInstallation gameInstallation) => 
        new GameProgressionManager_RaymanFiestaRun(this, gameInstallation, 1);

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