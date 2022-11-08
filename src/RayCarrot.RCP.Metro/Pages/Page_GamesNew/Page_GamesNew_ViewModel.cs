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
        GamesManager gamesManager) : base(app)
    {
        // Set services
        GamesManager = gamesManager ?? throw new ArgumentNullException(nameof(gamesManager));

        // Set properties
        AsyncLock = new AsyncLock();

        GameCategories = new ObservableCollection<GameCategoryViewModel>();
        var source = CollectionViewSource.GetDefaultView(GameCategories);
        source.Filter = p => !((GameCategoryViewModel)p).FilteredGameGroups.IsEmpty;
        FilteredGameCategories = source;

        // Create commands
        RefreshGamesCommand = new AsyncRelayCommand(RefreshAsync);
    }

    #endregion

    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Commands

    public ICommand RefreshGamesCommand { get; }

    #endregion

    #region Private Fields

    private string _gameFilter = String.Empty;

    #endregion

    #region Private Properties

    private AsyncLock AsyncLock { get; }

    #endregion

    #region Services

    private GamesManager GamesManager { get; }

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

    public ObservableCollection<GameCategoryViewModel> GameCategories { get; }
    public ICollectionView FilteredGameCategories { get; }

    public InstalledGameViewModel? SelectedInstalledGame { get; set; }

    #endregion

    #region Public Methods

    public void UpdateFilteredCollections()
    {
        foreach (GameCategoryViewModel gameCategory in GameCategories)
            gameCategory.UpdateFilteredCollections();

        FilteredGameCategories.Refresh();
    }

    protected override Task InitializeAsync() => RefreshAsync();

    public async Task RefreshAsync()
    {
        using (await AsyncLock.LockAsync())
        {
            try
            {
                GameCategories.Clear();

                foreach (var categorizedGames in GamesManager.EnumerateInstalledGames().GroupBy(x => x.GameDescriptor.Category))
                {
                    var category = new GameCategoryViewModel(categorizedGames.Key.ToString(), () => GameFilter);
                    GameCategories.Add(category);

                    foreach (var gameInstallations in categorizedGames.
                                 OrderBy(x => x.GameDescriptor.Game).
                                 GroupBy(x => x.GameDescriptor.Game))
                    {
                        // TODO-UPDATE: Get most common icon?
                        string iconSource = gameInstallations.First().GameDescriptor.IconSource;
                        category.GameGroups.Add(new GameGroupViewModel(
                            iconSource: iconSource, 
                            installedGames: gameInstallations.Select(x => new InstalledGameViewModel(x))));
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

    #endregion
}