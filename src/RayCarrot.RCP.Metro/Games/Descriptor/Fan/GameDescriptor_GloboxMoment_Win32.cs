#nullable disable
using System;
using System.Collections.Generic;
using NLog;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Globox Moment game descriptor
/// </summary>
public sealed class GameDescriptor_GloboxMoment_Win32 : Win32GameDescriptor
{
    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Public Override Properties

    public override string Id => "GloboxMoment_Win32";
    public override Game Game => Game.GloboxMoment;

    /// <summary>
    /// The game
    /// </summary>
    public override Games LegacyGame => Games.GloboxMoment;

    /// <summary>
    /// The category for the game
    /// </summary>
    public override GameCategory Category => GameCategory.Fan;

    /// <summary>
    /// The game display name
    /// </summary>
    public override string DisplayName => "Globox Moment";

    /// <summary>
    /// The game backup name
    /// </summary>
    public override string BackupName => "Globox Moment";

    /// <summary>
    /// Gets the launch name for the game
    /// </summary>
    public override string DefaultFileName => "Globox Moment.exe";

    /// <summary>
    /// Indicates if the game can be downloaded
    /// </summary>
    public override bool CanBeDownloaded => false;

    /// <summary>
    /// The files to remove when uninstalling
    /// </summary>
    public override IEnumerable<FileSystemPath> UninstallFiles => new FileSystemPath[]
    {
        Environment.SpecialFolder.ApplicationData.GetFolderPath() + "MMFApplications" + "globoxmoment.ini"
    };

    public override IEnumerable<ProgressionGameViewModel> GetProgressionGameViewModels(GameInstallation gameInstallation) => 
        new ProgressionGameViewModel_GloboxMoment(gameInstallation).Yield();

    /// <summary>
    /// Gets the purchase links for the game for this type
    /// </summary>
    public override IEnumerable<GamePurchaseLink> GetGamePurchaseLinks() => new GamePurchaseLink[]
    {
        new(Resources.GameDisplay_GameJolt, "https://gamejolt.com/games/globoxmoment/428585", GenericIconKind.GameDisplay_Web),
    };

    /// <summary>
    /// Gets the additional overflow button items for the game
    /// </summary>
    public override IEnumerable<OverflowButtonItemViewModel> GetAdditionalOverflowButtonItems() => new OverflowButtonItemViewModel[]
    {
        new(Resources.GameDisplay_OpenGameJoltPage, GenericIconKind.GameDisplay_Web, new AsyncRelayCommand(async () =>
        {
            (await Services.File.LaunchFileAsync("https://gamejolt.com/games/globoxmoment/428585"))?.Dispose();
            Logger.Trace("The game {0} GameJolt page was opened", Game);
        })),
    };

    #endregion
}