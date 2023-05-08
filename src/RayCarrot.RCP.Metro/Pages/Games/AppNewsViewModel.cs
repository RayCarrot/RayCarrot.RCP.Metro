using System.Windows.Input;

namespace RayCarrot.RCP.Metro.Pages.Games;

public class AppNewsViewModel : BaseViewModel
{
    #region Constructor

    public AppNewsViewModel(
        AppUserData appUserData, 
        AppUIManager ui)
    {
        AppUserData = appUserData ?? throw new ArgumentNullException(nameof(appUserData));
        UI = ui ?? throw new ArgumentNullException(nameof(ui));

        ShowMore(true);

        ShowVersionHistoryCommand = new AsyncRelayCommand(ShowVersionHistoryAsync);
        ShowMoreCommand = new RelayCommand(() => ShowMore(false));
    }

    #endregion

    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Commands

    public ICommand ShowVersionHistoryCommand { get; }
    public ICommand ShowMoreCommand { get; }

    #endregion

    #region Services

    private AppUserData AppUserData { get; }
    private AppUIManager UI { get; }

    #endregion

    #region Public Properties

    public ObservableCollection<AppNewsEntry>? NewsEntries { get; set; }
    public bool CanShowMore { get; set; }

    #endregion

    #region Public Methods

    public Task ShowVersionHistoryAsync() => UI.ShowVersionHistoryAsync();

    public void ShowMore(bool reset)
    {
        int prevCount = reset ? 0 : NewsEntries?.Count ?? 0;
        NewsEntries = new ObservableCollection<AppNewsEntry>(AppUserData.App_CachedNews.Take(prevCount + 8));
        CanShowMore = AppUserData.App_CachedNews.Count > NewsEntries.Count;
    }

    public async Task LoadAsync()
    {
        try
        {
            Logger.Trace("Downloading latest app news");

            // Do Task.Run since if it has trouble establishing a connection, like if you
            // have airplane mode on, then this takes a long time and blocks the thread
            List<AppNewsEntry> entries = await Task.Run(() => JsonHelpers.DeserializeFromURLAsync<List<AppNewsEntry>>(AppURLs.AppNewsUrl));

            entries.RemoveAll(x => (x.MinAppVersion != null && AppViewModel.AppVersion < x.MinAppVersion) ||
                                   (x.MaxAppVersion != null && AppViewModel.AppVersion >= x.MaxAppVersion));

            Logger.Info("Downloaded latest app news");

            AppUserData.App_CachedNews = entries;
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