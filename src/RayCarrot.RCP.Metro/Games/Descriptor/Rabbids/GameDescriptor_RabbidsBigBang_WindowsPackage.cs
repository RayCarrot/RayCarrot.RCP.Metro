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

    public override GameIconAsset Icon => GameIconAsset.RabbidsBigBang;

    public override string PackageName => "UbisoftEntertainment.RabbidsBigBang";
    public override string FullPackageName => "UbisoftEntertainment.RabbidsBigBang_dbgk1hhpxymar";

    #endregion

    #region Public Methods

    public override GameProgressionManager GetGameProgressionManager(GameInstallation gameInstallation) => 
        new GameProgressionManager_RabbidsBigBang(this, gameInstallation);

    public override IEnumerable<GameUriLink> GetExternalUriLinks(GameInstallation gameInstallation) => new[]
    {
        new GameUriLink(
            Header: new ResourceLocString(nameof(Resources.GameDisplay_OpenInWinStore)),
            Uri: MicrosoftStoreHelpers.GetStorePageURI(MicrosoftStoreID),
            Icon: GenericIconKind.GameDisplay_Microsoft)
    };

    #endregion
}