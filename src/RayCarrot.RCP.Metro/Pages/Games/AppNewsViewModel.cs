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

        NewsEntries = new ObservableCollection<AppNewsEntry>(AppUserData.App_CachedNews);

        ShowVersionHistoryCommand = new AsyncRelayCommand(ShowVersionHistoryAsync);
    }

    #endregion

    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Commands

    public ICommand ShowVersionHistoryCommand { get; }

    #endregion

    #region Services

    private AppUserData AppUserData { get; }
    private AppUIManager UI { get; }

    #endregion

    #region Public Properties

    public ObservableCollection<AppNewsEntry> NewsEntries { get; set; }

    #endregion

    #region Public Methods

    public Task ShowVersionHistoryAsync() => UI.ShowVersionHistoryAsync();

    public async Task LoadAsync()
    {
        try
        {
            // Do Task.Run since if it has trouble establishing a connection, like if you
            // have airplane mode on, then this takes a long time and blocks the thread
            List<AppNewsEntry> entries = await Task.Run(() => JsonHelpers.DeserializeFromURLAsync<List<AppNewsEntry>>(AppURLs.AppNewsUrl));

            entries.RemoveAll(x => (x.MinAppVersion != null && AppViewModel.AppVersion < x.MinAppVersion) ||
                                   (x.MaxAppVersion != null && AppViewModel.AppVersion >= x.MaxAppVersion));

            NewsEntries = new ObservableCollection<AppNewsEntry>(entries);
            AppUserData.App_CachedNews = entries;
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