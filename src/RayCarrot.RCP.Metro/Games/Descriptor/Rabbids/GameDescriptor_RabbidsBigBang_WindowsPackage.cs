using System.Collections.Generic;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rabbids Big Bang (Windows Package) game descriptor
/// </summary>
public sealed class GameDescriptor_RabbidsBigBang_WindowsPackage : WindowsPackageGameDescriptor
{
    #region Private Constant Fields

    private const string MicrosoftStoreID = "9WZDNCRFJCS3";

    #endregion

    #region Public Properties

    public override string Id => "RabbidsBigBang_WindowsPackage";
    public override Game Game => Game.RabbidsBigBang;
    public override GameCategory Category => GameCategory.Rabbids;
    public override Games? LegacyGame => Games.RabbidsBigBang;

    public override string DisplayName => "Rabbids Big Bang";
    public override string BackupName => "Rabbids Big Bang";
    public override string DefaultFileName => "Template.exe";

    public override string PackageName => "UbisoftEntertainment.RabbidsBigBang";
    public override string FullPackageName => "UbisoftEntertainment.RabbidsBigBang_dbgk1hhpxymar";

    #endregion

    #region Public Methods

    public override IEnumerable<ProgressionGameViewModel> GetProgressionGameViewModels(GameInstallation gameInstallation) => 
        new ProgressionGameViewModel_RabbidsBigBang(gameInstallation).Yield();

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