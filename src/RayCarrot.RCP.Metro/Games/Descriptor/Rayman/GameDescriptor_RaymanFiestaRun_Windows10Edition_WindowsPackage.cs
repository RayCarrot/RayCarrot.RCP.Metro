using System.Collections.Generic;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman Fiesta Run Windows 10 Edition (Windows Package) game descriptor
/// </summary>
public sealed class GameDescriptor_RaymanFiestaRun_Windows10Edition_WindowsPackage : WindowsPackageGameDescriptor
{
    #region Private Constant Fields

    private const string MicrosoftStoreID = "9nblggh59m6b";

    #endregion

    #region Public Properties

    public override string Id => "RaymanFiestaRunWindows10Edition_WindowsPackage";
    public override Game Game => Game.RaymanFiestaRun;
    public override GameCategory Category => GameCategory.Rayman;
    public override Games? LegacyGame => Games.RaymanFiestaRun;

    public override string DisplayName => "Rayman Fiesta Run Windows 10 Edition";
    public override string BackupName => "Rayman Fiesta Run (Win10)";
    public override string DefaultFileName => "RFRXAML.exe";

    public override GameIconAsset Icon => GameIconAsset.RaymanFiestaRun;

    public override string PackageName => "Ubisoft.RaymanFiestaRunWindows10Edition";
    public override string FullPackageName => "Ubisoft.RaymanFiestaRunWindows10Edition_ngz4m417e0mpw";

    #endregion

    #region Public Methods

    public override GameOptionsDialog_ConfigPageViewModel GetConfigPageViewModel(GameInstallation gameInstallation) => 
        new Config_RaymanFiestaRun_ViewModel(this);

    public override GameProgressionManager GetGameProgressionManager(GameInstallation gameInstallation) => 
        new GameProgressionManager_RaymanFiestaRun(this, gameInstallation, 0);

    public override IEnumerable<GameUriLink> GetExternalUriLinks(GameInstallation gameInstallation) => new[]
    {
        new GameUriLink(Header: new ResourceLocString(nameof(Resources.GameDisplay_OpenInWinStore)),
            Uri: MicrosoftStoreHelpers.GetStorePageURI(MicrosoftStoreID),
            Icon: GenericIconKind.GameDisplay_Microsoft)
    };

    public override IEnumerable<Utility> GetUtilities(GameInstallation gameInstallation) => new Utility[]
    {
        new Utility_RaymanFiestaRun_SaveFix(this, gameInstallation, 0),
    };

    #endregion
}