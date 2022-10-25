using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Nito.AsyncEx;
using NLog;
using RayCarrot.RCP.Metro.Archive;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// View model for the games page
/// </summary>
public class Page_Games_ViewModel : BasePageViewModel, IDisposable
{
    #region Constructor

    public Page_Games_ViewModel(
        AppViewModel app, 
        AppUserData data, 
        IMessageUIManager messageUi,
        IAppInstanceData instanceData, 
        AppUIManager ui, 
        IFileManager file) : base(app)
    {
        // Set services
        Data = data ?? throw new ArgumentNullException(nameof(data));
        MessageUI = messageUi ?? throw new ArgumentNullException(nameof(messageUi));
        UI = ui ?? throw new ArgumentNullException(nameof(ui));
        File = file ?? throw new ArgumentNullException(nameof(file));

        // Get categorized games
        var games = App.GetCategorizedGames;
            
        // Create properties
        RefreshingGames = false;
        AsyncLock = new AsyncLock();

        GameCategories = new ObservableCollection<Page_Games_CategoryViewModel>()
        {
            // Create the master category
            new Page_Games_CategoryViewModel(App.GetGames), 
                
            // Create the categories
            new Page_Games_CategoryViewModel(games[GameCategory.Rayman], new ResourceLocString(nameof(Resources.GamesPage_Category_Rayman)), GenericIconKind.Games_Rayman), 
            new Page_Games_CategoryViewModel(games[GameCategory.Rabbids], new ResourceLocString(nameof(Resources.GamesPage_Category_Rabbids)), GenericIconKind.Games_Rabbids), 
            new Page_Games_CategoryViewModel(games[GameCategory.Demo], new ResourceLocString(nameof(Resources.GamesPage_Category_Demos)), GenericIconKind.Games_Demos),
            new Page_Games_CategoryViewModel(games[GameCategory.Other], new ResourceLocString(nameof(Resources.GamesPage_Category_Other)), GenericIconKind.Games_Other), 
            new Page_Games_CategoryViewModel(games[GameCategory.Fan], new ResourceLocString(nameof(Resources.GamesPage_Category_Fan)), GenericIconKind.Games_FanGames),
        };

        // Create commands
        RunGameFinderCommand = new AsyncRelayCommand(RunGameFinderAsync);
        RefreshGamesCommand = new AsyncRelayCommand(RefreshAsync);

        // Refresh on app refresh
        App.RefreshRequired += async (_, e) =>
        {
            if (e.LaunchInfoModified && e.ModifiedGames.Any())
                foreach (Games game in e.ModifiedGames)
                    await RefreshGameAsync(game);

            else if (e.LaunchInfoModified || e.GameCollectionModified)
                await RefreshAsync();
        };

        // Refresh category visibility
        RefreshCategorizedVisibility();

        // Refresh on culture changed
        instanceData.CultureChanged += async (_, _) => await Task.Run(async () => await RefreshAsync());

        // Refresh visibility on setting change
        Data.PropertyChanged += Data_PropertyChanged;
    }

    #endregion

    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Commands

    public ICommand RunGameFinderCommand { get; }
    public ICommand RefreshGamesCommand { get; }

    #endregion

    #region Private Properties

    /// <summary>
    /// The async lock to use for the game page
    /// </summary>
    private AsyncLock AsyncLock { get; }

    #endregion

    #region Services

    private AppUserData Data { get; }
    private IMessageUIManager MessageUI { get; }
    private AppUIManager UI { get; }
    private IFileManager File { get; }

    #endregion

    #region Public Properties

    public override AppPage Page => AppPage.Games;

    /// <summary>
    /// The selected game category index
    /// </summary>
    public int SelectedCategoryIndex { get; set; }

    /// <summary>
    /// The games category view models
    /// </summary>
    public ObservableCollection<Page_Games_CategoryViewModel> GameCategories { get; }

    /// <summary>
    /// Indicates if the games are being refreshed
    /// </summary>
    public bool RefreshingGames { get; set; }

    public bool CanRunGameFinder { get; set; }

    #endregion

    #region Private Methods

    /// <summary>
    /// Gets a type for the game, or null if the operation was canceled
    /// </summary>
    /// <returns>The type or null if the operation was canceled</returns>
    private async Task<GameTypeSelectionResult> GetGameTypeAsync(Games game)
    {
        // Get the available types
        GameType[] types = Services.App.GamesManager.GameManagers[game].Keys.ToArray();

        // If only one type, return that
        if (types.Length == 1)
            return new GameTypeSelectionResult()
            {
                CanceledByUser = false,
                SelectedType = types.First()
            };

        // Create the view model
        var vm = new GameTypeSelectionViewModel()
        {
            Title = Resources.App_SelectGameTypeHeader
        };

        // Enumerate the available types
        foreach (var type in types)
        {
            if (type == GameType.Win32)
                vm.AllowWin32 = true;
            else if (type == GameType.Steam)
                vm.AllowSteam = true;
            else if (type == GameType.WinStore)
                vm.AllowWinStore = true;
            else if (type == GameType.DosBox)
                vm.AllowDosBox = true;
            else if (type == GameType.EducationalDosBox)
                vm.AllowEducationalDosBox = true;
            else
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }

        // Create and show the dialog and return the result
        return await UI.SelectGameTypeAsync(vm);
    }

    /// <summary>
    /// Allows the user to locate the game and add it
    /// </summary>
    /// <returns>The task</returns>
    private async Task LocateGameAsync(Games game)
    {
        try
        {
            Logger.Trace("The game {0} is being located...", game);

            var typeResult = await GetGameTypeAsync(game);

            if (typeResult.CanceledByUser)
                return;

            Logger.Info("The game {0} type has been detected as {1}", game, typeResult.SelectedType);

            await game.GetManager(typeResult.SelectedType).LocateAddGameAsync();
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Locating game");
            await MessageUI.DisplayExceptionMessageAsync(ex, Resources.LocateGame_Error, Resources.LocateGame_ErrorHeader);
        }
    }

    /// <summary>
    /// Allows the user to download the game and add it
    /// </summary>
    /// <returns>The task</returns>
    private async Task DownloadGameAsync(GameInfo gameInfo)
    {
        try
        {
            Logger.Trace("The game {0} is being downloaded...", gameInfo.Game);

            // Get the game directory
            var gameDir = AppFilePaths.GamesBaseDir + gameInfo.Game.ToString();

            // Download the game
            var downloaded = await Services.App.DownloadAsync(gameInfo.DownloadURLs, true, gameDir, true);

            if (!downloaded)
                return;

            // Add the game
            await Services.Games.AddGameAsync(gameInfo.Game, gameInfo.DownloadType, gameDir, true);

            // Refresh
            await Services.App.OnRefreshRequiredAsync(new RefreshRequiredEventArgs(gameInfo.Game, RefreshFlags.GameCollection));

            Logger.Trace("The game {0} has been downloaded", gameInfo.Game);

            await MessageUI.DisplaySuccessfulActionMessageAsync(String.Format(Resources.GameInstall_Success, gameInfo.DisplayName), Resources.GameInstall_SuccessHeader);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Downloading game");
            await MessageUI.DisplayExceptionMessageAsync(ex, String.Format(Resources.GameInstall_Error, gameInfo.DisplayName), Resources.GameInstall_ErrorHeader);
        }
    }

    /// <summary>
    /// Gets a display view model for the game
    /// </summary>
    /// <param name="game">The game to get the view model for</param>
    /// <returns>A new display view model</returns>
    private Page_Games_GameViewModel GetDisplayViewModel(Games game)
    {
        try
        {
            GameInfo gameInfo = game.GetGameInfo();

            if (game.IsAdded())
            {
                var actions = new List<OverflowButtonItemViewModel>();

                // Get the installation
                GameInstallation gameInstallation = game.GetInstallation();

                // Get the manager
                var manager = game.GetManager();

                // Add launch options if set to do so
                var launchMode = gameInstallation.GetValue<UserData_GameLaunchMode>(GameDataKey.Win32LaunchMode);
                if (launchMode == UserData_GameLaunchMode.AsAdminOption)
                {
                    actions.Add(new OverflowButtonItemViewModel(Resources.GameDisplay_RunAsAdmin, GenericIconKind.GameDisplay_Admin, new AsyncRelayCommand(async () => await game.GetManager().LaunchGameAsync(true))));

                    actions.Add(new OverflowButtonItemViewModel());
                }

                // Get the Game links
                var links = gameInfo.GetGameFileLinks(gameInstallation)?.Where(x => x.Path.FileExists).ToArray();

                // Add links if there are any
                if (links?.Any() ?? false)
                {
                    actions.AddRange(links.
                        Select(x =>
                        {
                            // Get the path
                            string path = x.Path;

                            // Create the command
                            var command = new AsyncRelayCommand(async () => (await File.LaunchFileAsync(path, arguments: x.Arguments))?.Dispose());

                            if (x.Icon != GenericIconKind.None)
                                return new OverflowButtonItemViewModel(x.Header, x.Icon, command);

                            try
                            {
                                return new OverflowButtonItemViewModel(x.Header, WindowsHelpers.GetIconOrThumbnail(x.Path, ShellThumbnailSize.Small).ToImageSource(), command);
                            }
                            catch (Exception ex)
                            {
                                Logger.Error(ex, "Getting file icon for overflow button item");
                                return new OverflowButtonItemViewModel(x.Header, x.Icon, command);
                            }
                        }));

                    actions.Add(new OverflowButtonItemViewModel());
                }

                // Get additional items
                var additionalItems = manager.GetAdditionalOverflowButtonItems;

                // Add the items if there are any
                if (additionalItems.Any())
                {
                    actions.AddRange(additionalItems);

                    actions.Add(new OverflowButtonItemViewModel());
                }

                // Add RayMap link
                if (gameInfo.RayMapURL != null)
                {
                    actions.Add(new OverflowButtonItemViewModel(Resources.GameDisplay_Raymap, GenericIconKind.GameDisplay_Map, new AsyncRelayCommand(async () => (await File.LaunchFileAsync(gameInfo.RayMapURL))?.Dispose())));
                    actions.Add(new OverflowButtonItemViewModel());
                }

                // Add open archive
                if (gameInfo.HasArchives)
                {
                    actions.Add(new OverflowButtonItemViewModel(Resources.GameDisplay_Archives, GenericIconKind.GameDisplay_Archive, new AsyncRelayCommand(async () =>
                    {
                        using IArchiveDataManager archiveDataManager = gameInfo.GetArchiveDataManager(gameInstallation);

                        try
                        {
                            // Show the archive explorer
                            await UI.ShowArchiveExplorerAsync(
                                manager: archiveDataManager,
                                filePaths: gameInfo.GetArchiveFilePaths(gameInstallation.InstallLocation).
                                    Where(x => x.FileExists).
                                    ToArray());
                        }
                        catch (Exception ex)
                        {
                            Logger.Error(ex, "Running Archive Explorer");

                            await MessageUI.DisplayExceptionMessageAsync(ex, Resources.Archive_CriticalError);
                        }
                    }), UserLevel.Advanced));
                }

                // Add open location
                actions.Add(new OverflowButtonItemViewModel(Resources.GameDisplay_OpenLocation, GenericIconKind.GameDisplay_Location, new AsyncRelayCommand(async () =>
                {
                    // Get the install directory
                    FileSystemPath instDir = gameInstallation.InstallLocation;

                    // Select the file in Explorer if it exists
                    if ((instDir + gameInfo.DefaultFileName).FileExists)
                        instDir += gameInfo.DefaultFileName;

                    // Open the location
                    await File.OpenExplorerLocationAsync(instDir);

                    Logger.Trace("The Game {0} install location was opened", gameInstallation.ID);
                }), UserLevel.Advanced));

                actions.Add(new OverflowButtonItemViewModel(UserLevel.Advanced));

                // Add patcher option
                if (gameInfo.AllowPatching)
                {
                    actions.Add(new OverflowButtonItemViewModel(Resources.GameDisplay_Patcher, GenericIconKind.GameDisplay_Patcher, new AsyncRelayCommand(async () =>
                    {
                        try
                        {
                            // Show the Patcher
                            await UI.ShowPatcherAsync(gameInstallation);
                        }
                        catch (Exception ex)
                        {
                            Logger.Error(ex, "Runing Patcher");

                            await MessageUI.DisplayExceptionMessageAsync(ex, Resources.Patcher_CriticalError);
                        }
                    }), UserLevel.Advanced));
                    actions.Add(new OverflowButtonItemViewModel(UserLevel.Advanced));
                }

                // Add Game options
                var optionsAction = new OverflowButtonItemViewModel(Resources.GameDisplay_Options, GenericIconKind.GameDisplay_Config, new AsyncRelayCommand(async () =>
                {
                    Logger.Trace("The game {0} options dialog is opening...", gameInstallation.ID);
                    await UI.ShowGameOptionsAsync(gameInstallation);
                }));

                actions.Add(optionsAction);

                return new Page_Games_GameViewModel(
                    game: game,
                    displayName: gameInfo.DisplayName,
                    iconSource: gameInfo.IconSource,
                    isDemo: gameInfo.IsDemo,
                    mainAction: new ActionItemViewModel(Resources.GameDisplay_Launch, GenericIconKind.GameDisplay_Play, new AsyncRelayCommand(async () => await game.GetManager().LaunchGameAsync(false))),
                    secondaryAction: optionsAction,
                    launchActions: actions);
            }
            else
            {
                var actions = new List<OverflowButtonItemViewModel>();

                OverflowButtonItemViewModel? downloadItem = null;

                if (gameInfo.CanBeDownloaded)
                {
                    downloadItem = new OverflowButtonItemViewModel(Resources.GameDisplay_CloudInstall, GenericIconKind.GameDisplay_Download, new AsyncRelayCommand(async () => await DownloadGameAsync(gameInfo)));

                    if (gameInfo.CanBeLocated)
                    {
                        actions.Add(downloadItem);
                        actions.Add(new OverflowButtonItemViewModel());
                    }
                }

                // Get the purchase links
                var links = game.
                    // Get all available managers
                    GetManagers().
                    // Get the purchase links
                    SelectMany(x => x.GetGamePurchaseLinks);

                // Add links
                actions.AddRange(links.
                    Select(x =>
                    {
                        // Get the path
                        string path = x.Path;

                        // Create the command
                        var command = new AsyncRelayCommand(async () => (await File.LaunchFileAsync(path))?.Dispose());

                        // Return the item
                        return new OverflowButtonItemViewModel(x.Header, x.Icon, command);
                    }));

                // Add disc installer options for specific Games
                if (gameInfo.CanBeInstalledFromDisc)
                {
                    // Add separator if there are previous actions
                    if (actions.Any())
                        actions.Add(new OverflowButtonItemViewModel());

                    // Add disc installer action
                    actions.Add(new OverflowButtonItemViewModel(Resources.GameDisplay_DiscInstall, GenericIconKind.GameDisplay_DiscInstall, new AsyncRelayCommand(async () =>
                    {
                        // Show and run the installer
                        await Services.DialogBaseManager.ShowDialogWindowAsync(new GameInstallerDialog(game));
                    })));
                }

                // If the last option is a separator, remove it
                if (actions.LastOrDefault()?.IsSeparator == true)
                    actions.RemoveAt(actions.Count - 1);

                // Create the main action
                var mainAction = gameInfo.CanBeLocated
                    ? new ActionItemViewModel(Resources.GameDisplay_Locate, GenericIconKind.GameDisplay_Location, new AsyncRelayCommand(async () => await LocateGameAsync(game)))
                    : downloadItem;

                // Return the view model
                return new Page_Games_GameViewModel(game, gameInfo.DisplayName, gameInfo.IconSource, gameInfo.IsDemo, mainAction, null, actions);
            }
        }
        catch (Exception ex)
        {
            Logger.Fatal(ex, "Getting game display view model");
            throw;
        }
    }

    #endregion

    #region Public Methods

    protected override Task InitializeAsync()
    {
        return RefreshAsync();
    }

    /// <summary>
    /// Refreshes the added game
    /// </summary>
    /// <returns>The task</returns>
    public async Task RefreshGameAsync(Games game)
    {
        Logger.Info("The displayed game {0} is being refreshed...", game);

        using (await AsyncLock.LockAsync())
        {
            try
            {
                // Make sure the game has been added
                if (!game.IsAdded())
                    throw new Exception("Only added games can be refreshed individually");

                // Get the display view model
                Page_Games_GameViewModel displayVM = GetDisplayViewModel(game);

                // Refresh the game in every category it's available in
                foreach (var category in GameCategories.Where(x => x.Games.Contains(game)))
                {
                    Logger.Trace("The displayed game {0} in {1} is being refreshed...", game, category.DisplayName);

                    // Get the collection containing the game
                    var collection = category.InstalledGames.Any(x => x.Game == game) ? category.InstalledGames : category.NotInstalledGames;

                    // Get the game index
                    var index = collection.FindItemIndex(x => x.Game == game);

                    // Make sure we got a valid index
                    if (index == -1)
                    {
                        Logger.Warn("The displayed game {0} in {1} could not be refreshed due to not existing in either game collection", game, category.DisplayName);

                        return;
                    }

                    // Refresh the game
                    collection[index] = displayVM;

                    Logger.Trace("The displayed game {0} in {1} has been refreshed", game, category.DisplayName);
                }
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex, "Refreshing game {0}", game);
                throw;
            }
        }

        Logger.Info("The displayed game {0} has been refreshed", game);
    }

    /// <summary>
    /// Refreshes the games
    /// </summary>
    /// <returns>The task</returns>
    public async Task RefreshAsync()
    {
        using (await AsyncLock.LockAsync())
        {
            try
            {
                RefreshingGames = true;

                // Cache the game view models
                var displayVMCache = new Dictionary<Games, Page_Games_GameViewModel>();

                Logger.Info("All displayed games are being refreshed...");

                // Refresh all categories
                foreach (var category in GameCategories)
                {
                    Logger.Info("The displayed games in {0} are being refreshed...", category.DisplayName.Value);

                    try
                    {
                        // Clear collections
                        category.InstalledGames.Clear();
                        category.NotInstalledGames.Clear();
                            
                        category.AnyInstalledGames = false;
                        category.AnyNotInstalledGames = false;

                        // Enumerate each game
                        foreach (Games game in category.Games)
                        {
                            // If cached, reuse the view model, otherwise create new and add to cache
                            Page_Games_GameViewModel displayVM = displayVMCache.ContainsKey(game)
                                ? displayVMCache[game]
                                : displayVMCache[game] = GetDisplayViewModel(game);

                            // Check if it has been added
                            if (game.IsAdded())
                            {
                                // Add the game to the collection
                               category.InstalledGames.Add(displayVM);
                                category.AnyInstalledGames = true;
                            }
                            else
                            {
                                category.NotInstalledGames.Add(displayVM);
                                category.AnyNotInstalledGames = true;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Fatal(ex, "Refreshing games in {0}", category.DisplayName);
                        throw;
                    }

                    Logger.Info("The displayed games in {0} have been refreshed with {1} installed and {2} not installed games", category.DisplayName, category.InstalledGames.Count, category.NotInstalledGames.Count);
                }

                // Allow game finder to run only if there are games which have not been found
                CanRunGameFinder = GameCategories.Any(x => x.AnyNotInstalledGames);
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex, "Refreshing games");
                throw;
            }
            finally
            {
                RefreshingGames = false;
            }
        }
    }

    /// <summary>
    /// Refreshes the visibility of the categories based on if the games should be categorized
    /// </summary>
    public void RefreshCategorizedVisibility()
    {
        lock (GameCategories)
        {
            try
            {
                // Get the master category
                var master = GameCategories.First(x => x.IsMaster);

                // Set the master category visibility
                master.IsVisible = false;

                // Set the categories visibility
                GameCategories.Where(x => !x.IsMaster).ForEach(x => x.IsVisible = Data.UI_CategorizeGames);

                // Set the selected index
                SelectedCategoryIndex = Data.UI_CategorizeGames ? GameCategories.FindItemIndex(x => !x.IsMaster) : GameCategories.FindItemIndex(x => x == master);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Refreshing game category visibility");

                throw;
            }
        }
    }

    /// <summary>
    /// Runs the game finder
    /// </summary>
    /// <returns>The task</returns>
    public async Task RunGameFinderAsync()
    {
        // Make sure the game finder is not running
        if (App.IsGameFinderRunning)
            return;

        // Run the game finder
        var result = await Task.Run(App.RunGameFinderAsync);

        // Check the result
        if (!result)
            await MessageUI.DisplayMessageAsync(Resources.GameFinder_NoResults, Resources.GameFinder_ResultHeader, MessageType.Information);
    }

    public void Dispose()
    {
        GameCategories.DisposeAll();
    }

    #endregion

    #region Event Handlers

    private void Data_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(Data.UI_CategorizeGames))
            return;

        RefreshCategorizedVisibility();
    }

    #endregion
}