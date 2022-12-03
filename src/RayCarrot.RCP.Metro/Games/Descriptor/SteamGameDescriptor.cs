using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using NLog;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// A game descriptor for a Steam game
/// </summary>
public abstract class SteamGameDescriptor : GameDescriptor
{
    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Public Properties

    public override GamePlatform Platform => GamePlatform.Steam;
    public override bool SupportsGameLaunchMode => false;

    /// <summary>
    /// The Steam product id
    /// </summary>
    public abstract string SteamID { get; }

    #endregion

    #region Protected Methods

    protected override async Task<bool> LaunchAsync(GameInstallation gameInstallation, bool forceRunAsAdmin)
    {
        Logger.Trace("The game {0} is launching with Steam ID {1}", GameId, SteamID);

        // TODO-14: Does this return the Steam/game process or just explorer.exe?
        // Launch the game
        Process? process = await Services.File.LaunchFileAsync(SteamHelpers.GetStorePageURL(SteamID));

        Logger.Info("The game {0} has been launched", GameId);

        return process != null;
    }

    #endregion

    #region Public Methods

    public override IEnumerable<GameAddAction> GetAddActions() => new GameAddAction[]
    {
        new LocateGameAddAction(this, singleInstallationOnly: true),
        new FindSteamGameAddAction(this),
    };

    public override IEnumerable<GameUriLink> GetExternalUriLinks(GameInstallation gameInstallation) => new[]
    {
        new GameUriLink(
            Header: new ResourceLocString(nameof(Resources.GameDisplay_OpenSteamStore)),
            Uri: SteamHelpers.GetStorePageURL(SteamID),
            Icon: GenericIconKind.GameDisplay_Steam),
        new GameUriLink(
            Header: new ResourceLocString(nameof(Resources.GameDisplay_OpenSteamCommunity)),
            Uri: SteamHelpers.GetCommunityPageURL(SteamID),
            Icon: GenericIconKind.GameDisplay_Steam)
    };

    public override IEnumerable<GamePurchaseLink> GetPurchaseLinks() => new GamePurchaseLink[]
    {
        new(new ResourceLocString(nameof(Resources.GameDisplay_Steam)), SteamHelpers.GetStorePageURL(SteamID)),
    };

    public override GameFinder_GameItem GetGameFinderItem() => new(SteamID);

    public override IEnumerable<DuoGridItemViewModel> GetGameInfoItems(GameInstallation gameInstallation) =>
        base.GetGameInfoItems(gameInstallation).Concat(new[]
        {
            new DuoGridItemViewModel(
                header: new ResourceLocString(nameof(Resources.GameInfo_SteamID)),
                text: SteamID,
                minUserLevel: UserLevel.Advanced)
        });

    public override void CreateGameShortcut(GameInstallation gameInstallation, FileSystemPath shortcutName, FileSystemPath destinationDirectory)
    {
        Services.File.CreateURLShortcut(shortcutName, destinationDirectory, SteamHelpers.GetGameLaunchURI(SteamID));

        Logger.Trace("An URL shortcut was created for {0} under {1}", GameId, destinationDirectory);
    }

    public override IEnumerable<JumpListItemViewModel> GetJumpListItems(GameInstallation gameInstallation) => new[]
    {
        new JumpListItemViewModel(
            name: gameInstallation.GameDescriptor.DisplayName,
            iconSource: gameInstallation.InstallLocation + gameInstallation.GameDescriptor.DefaultFileName,
            launchPath: SteamHelpers.GetGameLaunchURI(SteamID),
            workingDirectory: null,
            launchArguments: null, 
            // TODO-14: Use game ID instead
            id: gameInstallation.LegacyGame.ToString())
    };

    #endregion
}