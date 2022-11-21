using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using Nito.AsyncEx;
using NLog;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// View model for the games page
/// </summary>
public class Page_GamesNew_ViewModel : BasePageViewModel
{
    #region Constructor

    public Page_GamesNew_ViewModel(
        AppViewModel app, 
        GamesManager gamesManager, 
        AppUIManager ui) : base(app)
    {
        // Set services
        GamesManager = gamesManager ?? throw new ArgumentNullException(nameof(gamesManager));
        UI = ui ?? throw new ArgumentNullException(nameof(ui));

        // Set properties
        AsyncLock = new AsyncLock();

        GameCategories = new ObservableCollection<InstalledGameCategoryViewModel>();
        var source = CollectionViewSource.GetDefaultView(GameCategories);
        source.Filter = p => !((InstalledGameCategoryViewModel)p).FilteredGameGroups.IsEmpty;
        FilteredGameCategories = source;

        // Create commands
        RefreshGamesCommand = new AsyncRelayCommand(RefreshAsync);
        AddGamesCommand = new AsyncRelayCommand(AddGamesAsync);
    }

    #endregion

    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Commands

    public ICommand RefreshGamesCommand { get; }
    public ICommand AddGamesCommand { get; }

    #endregion

    #region Private Fields

    private string _gameFilter = String.Empty;

    #endregion

    #region Private Properties

    private AsyncLock AsyncLock { get; }

    #endregion

    #region Services

    private GamesManager GamesManager { get; }
    private AppUIManager UI { get; }

    #endregion

    #region Public Properties

    public override AppPage Page => AppPage.GamesNew;

    public string GameFilter
    {
        get => _gameFilter;
        set
        {
            _gameFilter = value;
            UpdateFilteredCollections();
        }
    }

    public ObservableCollection<InstalledGameCategoryViewModel> GameCategories { get; }
    public ICollectionView FilteredGameCategories { get; }

    public InstalledGameViewModel? SelectedInstalledGame { get; set; }

    #endregion

    #region Protected Methods

    protected override Task InitializeAsync() => RefreshAsync();

    #endregion

    #region Public Methods

    public void UpdateFilteredCollections()
    {
        foreach (InstalledGameCategoryViewModel gameCategory in GameCategories)
            gameCategory.UpdateFilteredCollections();

        FilteredGameCategories.Refresh();
    }

    public async Task RefreshAsync()
    {
        using (await AsyncLock.LockAsync())
        {
            try
            {
                // Clear the categories
                GameCategories.Clear();

                // Enumerate every category of installed games
                foreach (var categorizedGames in GamesManager.EnumerateInstalledGames().
                             OrderBy(x => x.GameDescriptor.Category). // TODO-14: Normalize games sorting
                             GroupBy(x => x.GameDescriptor.Category))
                {
                    // Create a view model
                    GameCategoryInfoAttribute categoryInfo = categorizedGames.Key.GetInfo();
                    InstalledGameCategoryViewModel category = new(categoryInfo.DisplayName, () => GameFilter);
                    GameCategories.Add(category);

                    // Enumerate every group of installed games
                    foreach (var gameInstallations in categorizedGames.
                                 OrderBy(x => x.GameDescriptor.Game). // TODO-14: Normalize games sorting
                                 GroupBy(x => x.GameDescriptor.Game))
                    {
                        // Get the game info
                        GameInfoAttribute gameInfo = gameInstallations.Key.GetInfo();

                        // Add the group of game installations
                        category.GameGroups.Add(new InstalledGameGroupViewModel(
                            // TODO-UPDATE: Normalize
                            iconSource: $"{AppViewModel.WPFApplicationBasePath}Img/GameIcons/{gameInfo.GameIcon.GetAttribute<ImageFileAttribute>()!.FileName}",
                            displayName: gameInfo.DisplayName,
                            gameInstallations: gameInstallations));
                    }
                }

                SelectedInstalledGame = null;
            }
            catch (Exception ex)
            {
                // TODO-UPDATE: Handle exception
                Logger.Fatal(ex, "Refreshing games");
                throw;
            }
            finally
            {
                UpdateFilteredCollections();
            }
        }
    }

    public Task AddGamesAsync()
    {
        return UI.ShowAddGamesAsync();
    }

    #endregion
}