using System.Net.Http;
using System.Windows.Input;
using RayCarrot.RCP.Metro.ModLoader.Sources;

namespace RayCarrot.RCP.Metro.ModLoader.Dialogs.ModLoader;

public class DownloadableModsViewModel : BaseViewModel, IDisposable
{
    #region Constructor

    public DownloadableModsViewModel(
        ModLoaderViewModel modLoaderViewModel, 
        GameInstallation gameInstallation, 
        HttpClient httpClient, 
        IReadOnlyList<DownloadableModsSource> downloadableModsSources)
    {
        GameInstallation = gameInstallation;
        DownloadableModsSources = new ObservableCollection<DownloadableModsSourceViewModel>(
            downloadableModsSources.Select(x => new DownloadableModsSourceViewModel(x)));

        ModsFeed = new DownloadableModsFeedViewModel(modLoaderViewModel, gameInstallation, httpClient, downloadableModsSources);

        RefreshCommand = new AsyncRelayCommand(InitializeAsync);
        SearchCommand = new AsyncRelayCommand(SearchAsync);
    }

    #endregion

    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Private Fields

    private DownloadableModViewModel? _selectedMod;

    #endregion

    #region Commands

    public ICommand RefreshCommand { get; }
    public ICommand SearchCommand { get; }

    #endregion

    #region Public Properties

    public GameInstallation GameInstallation { get; }
    public ObservableCollection<DownloadableModsSourceViewModel> DownloadableModsSources { get; }
    public DownloadableModsFeedViewModel ModsFeed { get; }

    public DownloadableModViewModel? SelectedMod
    {
        get => _selectedMod;
        set
        {
            _selectedMod = value;
            _selectedMod?.OnSelected();
        }
    }

    public bool IsLoading { get; set; }

    public string SearchText { get; set; } = String.Empty;
    public string? CurrentSearchedText { get; set; }

    #endregion

    #region Public Methods

    public async Task InitializeAsync()
    {
        if (IsLoading)
            return;

        Logger.Info("Loading downloadable mods from {0} sources", DownloadableModsSources.Count);

        SearchText = String.Empty;
        CurrentSearchedText = null;

        ModsFeed.Initialize(null);

        await LoadNextChunkAsync();
    }

    public async Task SearchAsync()
    {
        if (IsLoading)
            return;

        // Search text has to be either empty or at least 2 characters
        if (SearchText.Length is not (0 or >= 2))
        {
            // TODO-LOC
            await Services.MessageUI.DisplayMessageAsync("The search text has to have at least 2 characters", MessageType.Error);
            return;
        }

        Logger.Info("Searching for downloadable mods with the text {0}", SearchText);

        if (SearchText == String.Empty)
        {
            CurrentSearchedText = null;
            ModsFeed.Initialize(null);
        }
        else
        {
            CurrentSearchedText = SearchText;
            ModsFeed.Initialize(new DownloadableModsFeedFilter(SearchText));
        }

        await LoadNextChunkAsync();
    }

    public async Task LoadNextChunkAsync()
    {
        if (IsLoading)
            return;

        IsLoading = true;

        Logger.Info("Loading chunk of downloadable mods");

        try
        {
            await ModsFeed.LoadNextChunkAsync();
        }
        finally
        {
            IsLoading = false;
        }
    }

    public void Dispose()
    {
        ModsFeed.Dispose();
    }

    #endregion
}