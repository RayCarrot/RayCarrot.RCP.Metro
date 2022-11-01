using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Win32;
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

    protected override async Task<GameLaunchResult> LaunchAsync(GameInstallation gameInstallation, bool forceRunAsAdmin)
    {
        Logger.Trace("The game {0} is launching with Steam ID {1}", Id, SteamID);

        // TODO-14: Does this return the Steam/game process or just explorer.exe?
        // Launch the game
        Process? process = await Services.File.LaunchFileAsync(SteamHelpers.GetStorePageURL(SteamID));

        Logger.Info("The game {0} has been launched", Id);

        return new GameLaunchResult(process, process != null);
    }

    #endregion

    #region Public Methods

    public override IEnumerable<GamePurchaseLink> GetGamePurchaseLinks() => new GamePurchaseLink[]
    {
        new(Resources.GameDisplay_Steam, SteamHelpers.GetStorePageURL(SteamID)),
    };

    public override IEnumerable<OverflowButtonItemViewModel> GetAdditionalOverflowButtonItems() => new OverflowButtonItemViewModel[]
    {
        new(Resources.GameDisplay_OpenSteamStore, GenericIconKind.GameDisplay_Steam, new AsyncRelayCommand(async () =>
        {
            (await Services.File.LaunchFileAsync(SteamHelpers.GetStorePageURL(SteamID)))?.Dispose();
            Logger.Trace("The game {0} Steam store page was opened", Id);
        })),
        new(Resources.GameDisplay_OpenSteamCommunity, GenericIconKind.GameDisplay_Steam, new AsyncRelayCommand(async () =>
        {
            (await Services.File.LaunchFileAsync(SteamHelpers.GetCommunityPageURL(SteamID)))?.Dispose();
            Logger.Trace("The game {0} Steam community page was opened", Id);
        }))
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

        Logger.Trace("An URL shortcut was created for {0} under {1}", Id, destinationDirectory);
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
            id: LegacyGame.ToString())
    };

    public override async Task<FileSystemPath?> LocateAsync()
    {
        FileSystemPath installDir;

        try
        {
            // Get the key path
            var keyPath = RegistryHelpers.CombinePaths(AppFilePaths.UninstallRegistryKey, $"Steam App {SteamID}");

            using var key = RegistryHelpers.GetKeyFromFullPath(keyPath, RegistryView.Registry64);

            // Get the install directory
            if (key?.GetValue("InstallLocation") is not string dir)
            {
                Logger.Info("The {0} was not found under Steam Apps", Id);

                await Services.MessageUI.DisplayMessageAsync(Resources.LocateGame_InvalidSteamGame, Resources.LocateGame_InvalidSteamGameHeader, MessageType.Error);

                return null;
            }

            installDir = dir;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Getting Steam game install directory");

            await Services.MessageUI.DisplayMessageAsync(Resources.LocateGame_InvalidSteamGame, Resources.LocateGame_InvalidSteamGameHeader, MessageType.Error);

            return null;
        }

        // Make sure the game is valid
        if (!await IsValidAsync(installDir))
        {
            Logger.Info("The {0} install directory was not valid", Id);

            await Services.MessageUI.DisplayMessageAsync(Resources.LocateGame_InvalidSteamGame, Resources.LocateGame_InvalidSteamGameHeader, MessageType.Error);

            return null;
        }

        return installDir;
    }

    #endregion
}