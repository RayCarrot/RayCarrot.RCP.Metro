using System.Windows.Input;

namespace RayCarrot.RCP.Metro.Pages.Games;

public class NewsViewModel : BaseViewModel
{
    #region Constructor

    public NewsViewModel(
        AppUserData appUserData, 
        AppUIManager ui,
        GamesManager games)
    {
        UI = ui ?? throw new ArgumentNullException(nameof(ui));

        AppNewsFeedViewModel = new AppNewsFeedViewModel(appUserData);
        ModNewsFeedViewModel = new ModNewsFeedViewModel(games);

        FeedViewModel = AppNewsFeedViewModel;

        ShowVersionHistoryCommand = new AsyncRelayCommand(ShowVersionHistoryAsync);
    }

    #endregion

    #region Private Fields

    private NewsFeed _selectedFeed;
    private bool _hasLoadedModNews;

    #endregion

    #region Commands

    public ICommand ShowVersionHistoryCommand { get; }

    #endregion

    #region Services

    private AppUIManager UI { get; }

    #endregion

    #region Public Properties

    public AppNewsFeedViewModel AppNewsFeedViewModel { get; }
    public ModNewsFeedViewModel ModNewsFeedViewModel { get; }

    public NewsFeed SelectedFeed
    {
        get => _selectedFeed;
        set
        {
            _selectedFeed = value;

            if (_selectedFeed == NewsFeed.Mods && !_hasLoadedModNews)
            {
                loadModNews();
                _hasLoadedModNews = true;
            }

            FeedViewModel = value switch
            {
                NewsFeed.App => AppNewsFeedViewModel,
                NewsFeed.Mods => ModNewsFeedViewModel,
                _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
            };

            async void loadModNews() => await ModNewsFeedViewModel.LoadAsync();
        }
    }
    public BaseViewModel FeedViewModel { get; set; }

    #endregion

    #region Public Methods

    public Task ShowVersionHistoryAsync() => UI.ShowVersionHistoryAsync();

    public async Task InitializeAsync()
    {
        await AppNewsFeedViewModel.InitializeAsync();
    }

    #endregion

    #region Enums

    public enum NewsFeed
    {
        App,
        Mods,
    }

    #endregion
}