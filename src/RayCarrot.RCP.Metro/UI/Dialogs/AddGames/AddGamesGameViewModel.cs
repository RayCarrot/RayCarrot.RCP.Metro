using System.Windows.Input;
using RayCarrot.RCP.Metro.Games.Finder;

namespace RayCarrot.RCP.Metro;

public class AddGamesGameViewModel : BaseViewModel
{
    #region Constructor

    public AddGamesGameViewModel(GameDescriptor gameDescriptor)
    {
        GameDescriptor = gameDescriptor;
        DisplayName = gameDescriptor.DisplayName;
        AddActions = new ObservableCollection<GameAddActionViewModel>(gameDescriptor.GetAddActions().
            // Reverse the order so the common actions are aligned in the ui
            Reverse().Select(x => new GameAddActionViewModel(x)));
        PurchaseLinks = new ObservableCollection<GamePurchaseLinkViewModel>(gameDescriptor.GetPurchaseLinks().
            Select(x => new GamePurchaseLinkViewModel(x.Header, x.Path, x.Icon)));
        
        // Get and set platform info
        GamePlatformInfoAttribute platformInfo = gameDescriptor.Platform.GetInfo();
        PlatformDisplayName = platformInfo.DisplayName;
        PlatformIcon = platformInfo.Icon;

        // Do a first refresh
        Refresh(true);

        AddGameCommand = new AsyncRelayCommand(x => AddGameAsync(((GameAddActionViewModel)x!).AddAction));
        FindGameCommand = new AsyncRelayCommand(FindGameAsync);
    }

    #endregion

    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Commands

    public ICommand AddGameCommand { get; }
    public ICommand FindGameCommand { get; }

    #endregion

    #region Public Properties

    public GameDescriptor GameDescriptor { get; }
    public LocalizedString DisplayName { get; }
    public ObservableCollection<GameAddActionViewModel> AddActions { get; }
    public GameFinderItem? FinderItem { get; set; }
    public ObservableCollection<GamePurchaseLinkViewModel> PurchaseLinks { get; }
    public bool HasPurchaseLinks => PurchaseLinks.Any();

    public LocalizedString PlatformDisplayName { get; }
    public GamePlatformIconAsset PlatformIcon { get; }

    public bool IsAdded { get; set; }

    #endregion

    #region Private Methods

    private void Refresh(bool firstRefresh)
    {
        // Check if there any added games with this game descriptor
        IsAdded = Services.Games.AnyInstalledGames(x => x.GameDescriptor == GameDescriptor);

        // Only get the finder item if there are no added games with this game descriptor
        FinderItem = IsAdded ? null : GameDescriptor.GetFinderItem();

        if (firstRefresh)
            return;

        // Re-evaluate if each action is available
        foreach (GameAddActionViewModel addAction in AddActions)
            addAction.OnPropertyChanged(nameof(GameAddActionViewModel.IsAvailable));
    }

    #endregion

    #region Public Methods

    public void Refresh() => Refresh(false);

    public async Task AddGameAsync(GameAddAction addAction)
    {
        try
        {
            // Call the add action
            await addAction.AddGameAsync();
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Adding game from add action");
            await Services.MessageUI.DisplayExceptionMessageAsync(ex, Resources.AddGames_AddError, Resources.AddGames_AddErrorHeader);
        }
    }

    public async Task FindGameAsync()
    {
        if (FinderItem == null)
            return;

        try
        {
            // Create a finder
            Finder finder = new(Finder.DefaultOperations, FinderItem.YieldToArray<FinderItem>());

            // Run the finder
            finder.Run();
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Running finder for a game");
            await Services.MessageUI.DisplayExceptionMessageAsync(ex, Resources.Finder_Error);
            return;
        }

        if (FinderItem.HasBeenFound)
        {
            // Have to get the location here since FinderItem is null after the game gets added
            InstallLocation foundLocation = FinderItem.FoundLocation.Value;

            ConfigureGameInstallation? configureGameInstallation = null;

            if (FinderItem.FoundQuery.ConfigureInstallation != null)
                configureGameInstallation = new ConfigureGameInstallation(FinderItem.FoundQuery.ConfigureInstallation);

            // Add the found games
            await Services.Games.AddGameAsync(FinderItem.GameDescriptor, foundLocation, configureGameInstallation);

            await Services.MessageUI.DisplayMessageAsync(String.Format(Resources.AddGames_FindSuccessResult, foundLocation), 
                Resources.AddGames_FindResultHeader, MessageType.Success);
        }
        else
        {
            await Services.MessageUI.DisplayMessageAsync(Resources.AddGames_FindFailedResult, 
                Resources.AddGames_FindResultHeader, MessageType.Information);
        }
    }

    #endregion

    #region Classes

    public class GameAddActionViewModel : BaseViewModel
    {
        public GameAddActionViewModel(GameAddAction addAction)
        {
            AddAction = addAction;
        }

        public GameAddAction AddAction { get; }

        public LocalizedString Header => AddAction.Header;
        public GenericIconKind Icon => AddAction.Icon;
        public bool IsAvailable => AddAction.IsAvailable;
    }

    public class GamePurchaseLinkViewModel : BaseViewModel
    {
        public GamePurchaseLinkViewModel(LocalizedString header, string path, GenericIconKind icon)
        {
            Header = header;
            Path = path;
            Icon = icon;

            OpenLinkCommand = new AsyncRelayCommand(OpenLinkAsync);
        }

        public ICommand OpenLinkCommand { get; }

        public LocalizedString Header { get; }
        public string Path { get; }
        public GenericIconKind Icon { get; }

        public Task OpenLinkAsync() => Services.File.LaunchFileAsync(Path);
    }

    #endregion
}