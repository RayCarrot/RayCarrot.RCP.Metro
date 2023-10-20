using System.Windows.Input;

namespace RayCarrot.RCP.Metro.Pages.Games;

public class AppNewsFeedViewModel : BaseViewModel
{
    #region Constructor

    public AppNewsFeedViewModel(AppUserData appUserData)
    {
        Data = appUserData ?? throw new ArgumentNullException(nameof(appUserData));

        // Load cached news first
        ShowMore(true);

        ShowMoreCommand = new RelayCommand(() => ShowMore(false));
    }

    #endregion

    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Commands

    public ICommand ShowMoreCommand { get; }

    #endregion

    #region Services

    private AppUserData Data { get; }

    #endregion

    #region Public Properties

    public ObservableCollection<AppNewsEntry>? NewsEntries { get; set; }
    public bool CanShowMore { get; set; }

    #endregion

    #region Private Methods

    private async Task LoadAppNews()
    {
        Logger.Trace("Downloading latest app news");

        // Do Task.Run since if it has trouble establishing a connection, like if you
        // have airplane mode on, then this takes a long time and blocks the thread
        List<AppNewsEntry> entries = await Task.Run(() => JsonHelpers.DeserializeFromURLAsync<List<AppNewsEntry>>(AppURLs.AppNewsUrl));

        entries.RemoveAll(x => (x.MinAppVersion != null && AppViewModel.AppVersion < x.MinAppVersion) ||
                               (x.MaxAppVersion != null && AppViewModel.AppVersion >= x.MaxAppVersion));

        Logger.Info("Downloaded latest app news");

        Data.App_CachedNews = entries;
    }

    #endregion

    #region Public Methods

    public void ShowMore(bool reset)
    {
        int prevCount = reset ? 0 : NewsEntries?.Count ?? 0;
        NewsEntries = new ObservableCollection<AppNewsEntry>(Data.App_CachedNews.Take(prevCount + 8));
        CanShowMore = Data.App_CachedNews.Count > NewsEntries.Count;
    }

    public async Task InitializeAsync()
    {
        try
        {
            if (Data.App_LoadNews)
                await LoadAppNews();

            ShowMore(true);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Loading app news");

            // We don't show a message to the user here since the cached news will
            // be shown instead. They'll probably also get an error from the updater.
        }
    }

    #endregion
}