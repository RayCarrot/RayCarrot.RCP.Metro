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
        _modLoaderViewModel = modLoaderViewModel;
        GameInstallation = gameInstallation;
        _httpClient = httpClient;
        DownloadableModsSources = new ObservableCollection<DownloadableModsSourceViewModel>(downloadableModsSources.Select(x =>
            new DownloadableModsSourceViewModel(x)));
        _primaryModsFeed = new DownloadableModsFeedViewModel(modLoaderViewModel, gameInstallation, httpClient, downloadableModsSources);
        CurrentModsFeed = _primaryModsFeed;

        RefreshCommand = new AsyncRelayCommand(InitializeAsync);
    }

    #endregion

    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Private Fields

    private readonly ModLoaderViewModel _modLoaderViewModel;
    private readonly HttpClient _httpClient;
    private readonly DownloadableModsFeedViewModel _primaryModsFeed;
    private DownloadableModViewModel? _selectedMod;

    #endregion

    #region Commands

    public ICommand RefreshCommand { get; }

    #endregion

    #region Public Properties

    public GameInstallation GameInstallation { get; }
    public ObservableCollection<DownloadableModsSourceViewModel> DownloadableModsSources { get; }

    public DownloadableModsFeedViewModel CurrentModsFeed { get; set; }

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

    #endregion

    #region Public Methods

    public async Task InitializeAsync()
    {
        if (IsLoading)
            return;

        Logger.Info("Loading downloadable mods from {0} sources", DownloadableModsSources.Count);

        CurrentModsFeed = _primaryModsFeed;
        CurrentModsFeed.Initialize();

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
            await CurrentModsFeed.LoadNextChunkAsync();
        }
        finally
        {
            IsLoading = false;
        }
    }

    public void Dispose()
    {
        _primaryModsFeed.Dispose();

        if (CurrentModsFeed != _primaryModsFeed)
            CurrentModsFeed.Dispose();
    }

    #endregion
}