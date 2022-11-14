using System.Collections.Generic;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman Jungle Run (Windows Package) game descriptor
/// </summary>
public sealed class GameDescriptor_RaymanJungleRun_WindowsPackage : WindowsPackageGameDescriptor
{
    #region Private Constant Fields

    private const string MicrosoftStoreID = "9WZDNCRFJ13P";

    #endregion

    #region Public Properties

    public override string Id => "RaymanJungleRun_WindowsPackage";
    public override Game Game => Game.RaymanJungleRun;
    public override GameCategory Category => GameCategory.Rayman;
    public override Games? LegacyGame => Games.RaymanJungleRun;

    public override string DisplayName => "Rayman Jungle Run";
    public override string BackupName => "Rayman Jungle Run";
    public override string DefaultFileName => "RO1Mobile.exe";

    public override string PackageName => "UbisoftEntertainment.RaymanJungleRun";
    public override string FullPackageName => "UbisoftEntertainment.RaymanJungleRun_dbgk1hhpxymar";

    #endregion

    #region Public Methods

    public override GameOptionsDialog_ConfigPageViewModel GetConfigPageViewModel(GameInstallation gameInstallation) =>
        new Config_RaymanJungleRun_ViewModel(this);

    public override GameProgressionManager GetGameProgressionManager(GameInstallation gameInstallation) =>
        new GameProgressionManager_RaymanJungleRun(this, gameInstallation);

    public override IEnumerable<GameUriLink> GetExternalUriLinks(GameInstallation gameInstallation) => new[]
    {
        new GameUriLink(
            Header: new ResourceLocString(nameof(Resources.GameDisplay_OpenInWinStore)),
            Uri: MicrosoftStoreHelpers.GetStorePageURI(MicrosoftStoreID),
            Icon: GenericIconKind.GameDisplay_Microsoft)
    };

    #endregion
}