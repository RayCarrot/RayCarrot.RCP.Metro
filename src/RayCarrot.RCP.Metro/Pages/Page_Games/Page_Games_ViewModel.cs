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
        FileManager file, 
        GamesManager gamesManager) : base(app)
    {
        // Set services
        Data = data ?? throw new ArgumentNullException(nameof(data));
        MessageUI = messageUi ?? throw new ArgumentNullException(nameof(messageUi));
        UI = ui ?? throw new ArgumentNullException(nameof(ui));
        File = file ?? throw new ArgumentNullException(nameof(file));
        GamesManager = gamesManager ?? throw new ArgumentNullException(nameof(gamesManager));

        // Get categorized games
        var groupedGames = gamesManager.EnumerateGameDescriptors()
            // Group the games by the category
            .GroupBy(x => x.Category).
            // Create a dictionary
            ToDictionary(x => x.Key, y => y.ToArray());
            
        // Create properties
        RefreshingGames = false;
        AsyncLock = new AsyncLock();

        GameCategories = new ObservableCollection<Page_Games_CategoryViewModel>()
        {
            // Create the master category
            new(gamesManager.EnumerateGameDescriptors().ToArray()), 
                
            // Create the categories
            new(groupedGames[GameCategory.Rayman], new ResourceLocString(nameof(Resources.GamesPage_Category_Rayman)), GenericIconKind.Games_Rayman), 
            new(groupedGames[GameCategory.Rabbids], new ResourceLocString(nameof(Resources.GamesPage_Category_Rabbids)), GenericIconKind.Games_Rabbids), 
            new(groupedGames[GameCategory.Demo], new ResourceLocString(nameof(Resources.GamesPage_Category_Demos)), GenericIconKind.Games_Demos),
            new(groupedGames[GameCategory.Other], new ResourceLocString(nameof(Resources.GamesPage_Category_Other)), GenericIconKind.Games_Other), 
            new(groupedGames[GameCategory.Fan], new ResourceLocString(nameof(Resources.GamesPage_Category_Fan)), GenericIconKind.Games_FanGames),
        };

        // Create commands
        RunGameFinderCommand = new AsyncRelayCommand(RunGameFinderAsync);
        RefreshGamesCommand = new AsyncRelayCommand(RefreshAsync);

        // Refresh on app refresh
        App.RefreshRequired += async (_, e) =>
        {
            if (e.LaunchInfoModified && e.ModifiedGames.Any())
                foreach (GameInstallation gameInstallation in e.ModifiedGames)
                    await RefreshGameAsync(gameInstallation.GameDescriptor);

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

    public AppUserData Data { get; } // Need to keep public for now due to binding
    private IMessageUIManager MessageUI { get; }
    private AppUIManager UI { get; }
    private FileManager File { get; }
    private GamesManager GamesManager { get; }

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
    /// Allows the user to locate the game and add it
    /// </summary>
    /// <returns>The task</returns>
    private async Task LocateGameAsync(GameDescriptor gameDescriptor)
    {
        try
        {
            // Locate the game and get the path
            FileSystemPath? path = await gameDescriptor.LocateAsync();

            if (path == null)
                return;

            // Add the game
            GameInstallation gameInstallation = await GamesManager.AddGameAsync(gameDescriptor, path.Value);

            // Refresh
            await App.OnRefreshRequiredAsync(new RefreshRequiredEventArgs(gameInstallation, RefreshFlags.GameCollection));

            Logger.Info("The game {0} has been added", gameInstallation.Id);
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
    private async Task DownloadGameAsync(GameDescriptor gameDescriptor)
    {
        try
        {
            Logger.Trace("The game {0} is being downloaded...", gameDescriptor.Id);

            // TODO-14: Change this. Use id? Make sure to not download multiple times then.
            // Get the game directory
            var gameDir = AppFilePaths.GamesBaseDir + gameDescriptor.LegacyGame.ToString();

            // Download the game
            var downloaded = await Services.App.DownloadAsync(gameDescriptor.DownloadURLs, true, gameDir, true);

            if (!downloaded)
                return;

            // Add the game
            GameInstallation gameInstallation = await GamesManager.AddGameAsync(gameDescriptor, gameDir);

            // Set the install info
            UserData_RCPGameInstallInfo installInfo = new(gameDir, UserData_RCPGameInstallInfo.RCPInstallMode.Download);
            gameInstallation.SetObject(GameDataKey.RCPGameInstallInfo, installInfo);

            // Refresh
            await Services.App.OnRefreshRequiredAsync(new RefreshRequiredEventArgs(gameInstallation, RefreshFlags.GameCollection));

            Logger.Trace("The game {0} has been downloaded", gameDescriptor.Id);

            await MessageUI.DisplaySuccessfulActionMessageAsync(String.Format(Resources.GameInstall_Success, gameDescriptor.DisplayName), Resources.GameInstall_SuccessHeader);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Downloading game");
            await MessageUI.DisplayExceptionMessageAsync(ex, String.Format(Resources.GameInstall_Error, gameDescriptor.DisplayName), Resources.GameInstall_ErrorHeader);
        }
    }

    private Page_Games_GameViewModel GetNotInstalledGameViewModel(GameDescriptor gameDescriptor)
    {
        var actions = new List<OverflowButtonItemViewModel>();

        OverflowButtonItemViewModel? downloadItem = null;

        if (gameDescriptor.CanBeDownloaded)
        {
            downloadItem = new OverflowButtonItemViewModel(Resources.GameDisplay_CloudInstall, GenericIconKind.GameDisplay_Download, new AsyncRelayCommand(async () => await DownloadGameAsync(gameDescriptor)));

            if (gameDescriptor.CanBeLocated)
            {
                actions.Add(downloadItem);
                actions.Add(new OverflowButtonItemViewModel());
            }
        }

        // Add purchase links
        actions.AddRange(gameDescriptor.GetGamePurchaseLinks().
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
        if (gameDescriptor.HasGameInstaller)
        {
            // Add separator if there are previous actions
            if (actions.Any())
                actions.Add(new OverflowButtonItemViewModel());

            // Add disc installer action
            actions.Add(new OverflowButtonItemViewModel(Resources.GameDisplay_DiscInstall, GenericIconKind.GameDisplay_DiscInstall, new AsyncRelayCommand(async () =>
            {
                // TODO-14: This should we called from the UI manager - same in debug page
                // Show and run the installer
                await Services.DialogBaseManager.ShowDialogWindowAsync(new GameInstallerDialog(gameDescriptor));
            })));
        }

        // If the last option is a separator, remove it
        if (actions.LastOrDefault()?.IsSeparator == true)
            actions.RemoveAt(actions.Count - 1);

        // Create the main action
        var mainAction = gameDescriptor.CanBeLocated
            ? new OverflowButtonItemViewModel(Resources.GameDisplay_Locate, GenericIconKind.GameDisplay_Location, new AsyncRelayCommand(async () => await LocateGameAsync(gameDescriptor)))
            : downloadItem;

        // Return the view model
        return new Page_Games_GameViewModel(gameDescriptor, gameDescriptor.DisplayName, gameDescriptor.IconSource, gameDescriptor.IsDemo, mainAction, null, actions);
    }

    private Page_Games_GameViewModel GetInstalledGameViewModel(GameInstallation gameInstallation)
    {
        GameDescriptor gameDescriptor = gameInstallation.GameDescriptor;

        var actions = new List<OverflowButtonItemViewModel>();

        // Add launch options if set to do so
        var launchMode = gameInstallation.GetValue<UserData_GameLaunchMode>(GameDataKey.Win32LaunchMode);
        if (launchMode == UserData_GameLaunchMode.AsAdminOption)
        {
            actions.Add(new OverflowButtonItemViewModel(Resources.GameDisplay_RunAsAdmin, GenericIconKind.GameDisplay_Admin, new AsyncRelayCommand(async () => await gameDescriptor.LaunchGameAsync(gameInstallation, true))));

            actions.Add(new OverflowButtonItemViewModel());
        }

        //// Get the Game links
        //var links = gameDescriptor.GetGameFileLinks(gameInstallation)?.Where(x => x.Path.FileExists).ToArray();

        //// Add links if there are any
        //if (links?.Any() ?? false)
        //{
        //    actions.AddRange(links.
        //        Select(x =>
        //        {
        //            // Get the path
        //            string path = x.Path;

        //            // Create the command
        //            var command = new AsyncRelayCommand(async () => (await File.LaunchFileAsync(path, arguments: x.Arguments))?.Dispose());

        //            if (x.Icon != GenericIconKind.None)
        //                return new OverflowButtonItemViewModel(x.Header, x.Icon, command);

        //            try
        //            {
        //                return new OverflowButtonItemViewModel(x.Header, WindowsHelpers.GetIconOrThumbnail(x.Path, ShellThumbnailSize.Small).ToImageSource(), command);
        //            }
        //            catch (Exception ex)
        //            {
        //                Logger.Error(ex, "Getting file icon for overflow button item");
        //                return new OverflowButtonItemViewModel(x.Header, x.Icon, command);
        //            }
        //        }));

        //    actions.Add(new OverflowButtonItemViewModel());
        //}

        //// Add additional items
        //int count = actions.Count;
        //actions.AddRange(gameDescriptor.GetGameLinks(gameInstallation));

        //if (actions.Count != count)
        //    actions.Add(new OverflowButtonItemViewModel());

        //// Add RayMap link
        //if (gameDescriptor.RayMapURL != null)
        //{
        //    actions.Add(new OverflowButtonItemViewModel(Resources.GameDisplay_Raymap, GenericIconKind.GameDisplay_Map, new AsyncRelayCommand(async () => (await File.LaunchFileAsync(gameDescriptor.RayMapURL))?.Dispose())));
        //    actions.Add(new OverflowButtonItemViewModel());
        //}

        // Add open archive
        if (gameDescriptor.HasArchives)
        {
            actions.Add(new OverflowButtonItemViewModel(Resources.GameDisplay_Archives, GenericIconKind.GameDisplay_Archive, new AsyncRelayCommand(async () =>
            {
                using IArchiveDataManager archiveDataManager = gameDescriptor.GetArchiveDataManager(gameInstallation);

                try
                {
                    // Show the archive explorer
                    await UI.ShowArchiveExplorerAsync(
                        manager: archiveDataManager,
                        filePaths: gameDescriptor.GetArchiveFilePaths(gameInstallation).
                            Select(x => gameInstallation.InstallLocation + x).
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
            if ((instDir + gameDescriptor.DefaultFileName).FileExists)
                instDir += gameDescriptor.DefaultFileName;

            // Open the location
            await File.OpenExplorerLocationAsync(instDir);

            Logger.Trace("The Game {0} install location was opened", gameInstallation.Id);
        }), UserLevel.Advanced));

        actions.Add(new OverflowButtonItemViewModel(UserLevel.Advanced));

        // Add patcher option
        if (gameDescriptor.AllowPatching)
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
            Logger.Trace("The game {0} options dialog is opening...", gameInstallation.Id);
            await UI.ShowGameOptionsAsync(gameInstallation);
        }));

        actions.Add(optionsAction);

        return new Page_Games_GameViewModel(
            gameDescriptor: gameDescriptor,
            displayName: gameDescriptor.DisplayName,
            iconSource: gameDescriptor.IconSource,
            isDemo: gameDescriptor.IsDemo,
            mainAction: new OverflowButtonItemViewModel(Resources.GameDisplay_Launch, GenericIconKind.GameDisplay_Play, new AsyncRelayCommand(async () => await gameDescriptor.LaunchGameAsync(gameInstallation, false))),
            secondaryAction: optionsAction,
            launchActions: actions);
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
    public async Task RefreshGameAsync(GameDescriptor gameDescriptor)
    {
        Logger.Info("The displayed game {0} is being refreshed...", gameDescriptor.Id);

        using (await AsyncLock.LockAsync())
        {
            try
            {
                // Make sure the game has been added
                if (!gameDescriptor.IsAdded())
                    throw new Exception("Only added games can be refreshed individually");

                // Get the display view model
                Page_Games_GameViewModel displayVM = gameDescriptor.IsAdded() 
                    ? GetInstalledGameViewModel(gameDescriptor.GetInstallation()) 
                    : GetNotInstalledGameViewModel(gameDescriptor);

                // Refresh the game in every category it's available in
                foreach (var category in GameCategories.Where(x => x.GameDescriptors.Contains(gameDescriptor)))
                {
                    Logger.Trace("The displayed game {0} in {1} is being refreshed...", gameDescriptor.Id, category.DisplayName);

                    // Get the collection containing the game
                    var collection = category.InstalledGames.Any(x => x.GameDescriptor == gameDescriptor) ? category.InstalledGames : category.NotInstalledGames;

                    // Get the game index
                    var index = collection.FindItemIndex(x => x.GameDescriptor == gameDescriptor);

                    // Make sure we got a valid index
                    if (index == -1)
                    {
                        Logger.Warn("The displayed game {0} in {1} could not be refreshed due to not existing in either game collection", gameDescriptor.Id, category.DisplayName);

                        return;
                    }

                    // Refresh the game
                    collection[index] = displayVM;

                    Logger.Trace("The displayed game {0} in {1} has been refreshed", gameDescriptor.Id, category.DisplayName);
                }
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex, "Refreshing game {0}", gameDescriptor.Id);
                throw;
            }
        }

        Logger.Info("The displayed game {0} has been refreshed", gameDescriptor.Id);
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
                var displayVMCache = new Dictionary<GameDescriptor, Page_Games_GameViewModel>();

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
                        foreach (GameDescriptor gameDescriptor in category.GameDescriptors)
                        {
                            // If cached, reuse the view model, otherwise create new and add to cache
                            Page_Games_GameViewModel displayVM = displayVMCache.ContainsKey(gameDescriptor)
                                ? displayVMCache[gameDescriptor]
                                : displayVMCache[gameDescriptor] = gameDescriptor.IsAdded()
                                    ? GetInstalledGameViewModel(gameDescriptor.GetInstallation())
                                    : GetNotInstalledGameViewModel(gameDescriptor);

                            // Check if it has been added
                            if (gameDescriptor.IsAdded())
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