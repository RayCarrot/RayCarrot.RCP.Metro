using System;
using System.Collections.Generic;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Globox Moment (Win32) game descriptor
/// </summary>
public sealed class GameDescriptor_GloboxMoment_Win32 : Win32GameDescriptor
{
    #region Public Properties

    public override string Id => "GloboxMoment_Win32";
    public override Game Game => Game.GloboxMoment;
    public override GameCategory Category => GameCategory.Fan;
    public override Games? LegacyGame => Games.GloboxMoment;

    public override string DisplayName => "Globox Moment";
    public override string BackupName => "Globox Moment";
    public override string DefaultFileName => "Globox Moment.exe";

    public override GameIconAsset Icon => GameIconAsset.GloboxMoment;

    // TODO-14: Should we be removing this?
    public override IEnumerable<FileSystemPath> UninstallFiles => new[]
    {
        Environment.SpecialFolder.ApplicationData.GetFolderPath() + "MMFApplications" + "globoxmoment.ini"
    };

    #endregion

    #region Public Methods

    public override GameProgressionManager GetGameProgressionManager(GameInstallation gameInstallation) =>
        new GameProgressionManager_GloboxMoment(gameInstallation);

    public override IEnumerable<GameUriLink> GetExternalUriLinks(GameInstallation gameInstallation) => new[]
    {
        new GameUriLink(
            Header: new ResourceLocString(nameof(Resources.GameDisplay_OpenGameJoltPage)),
            Uri: "https://gamejolt.com/games/globoxmoment/428585",
            Icon: GenericIconKind.GameDisplay_Web)
    };

    public override IEnumerable<GamePurchaseLink> GetGamePurchaseLinks() => new GamePurchaseLink[]
    {
        new(Resources.GameDisplay_GameJolt, "https://gamejolt.com/games/globoxmoment/428585", GenericIconKind.GameDisplay_Web),
    };

    #endregion
}