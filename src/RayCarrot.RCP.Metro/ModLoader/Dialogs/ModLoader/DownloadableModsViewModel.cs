using System.Net.Http;
using System.Windows.Input;
using RayCarrot.RCP.Metro.ModLoader.Sources;

namespace RayCarrot.RCP.Metro.ModLoader.Dialogs.ModLoader;

public class DownloadableModsViewModel : BaseViewModel
{
    #region Constructor

    public DownloadableModsViewModel(ModLoaderViewModel modLoaderViewModel, GameInstallation gameInstallation, HttpClient httpClient, IEnumerable<DownloadableModsSource> downloadableModsSources)
    {
        _modLoaderViewModel = modLoaderViewModel;
        GameInstallation = gameInstallation;
        _httpClient = httpClient;
        DownloadableModsSources = new ObservableCollection<DownloadableModsSourceViewModel>(downloadableModsSources.Select(x =>
            new DownloadableModsSourceViewModel(x)));

        Mods = new ObservableCollection<DownloadableModViewModel>();

        RefreshCommand = new AsyncRelayCommand(LoadModsAsync);
        OpenModDocsCommand = new RelayCommand(OpenModDocs);
        LoadChunkCommand = new AsyncRelayCommand(LoadChunkAsync);
    }

    #endregion

    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Constants

    private const int ChunkSize = 10;

    #endregion

    #region Private Fields

    private readonly HttpClient _httpClient;
    private readonly ModLoaderViewModel _modLoaderViewModel;
    private int _pageCount;
    private int _currentPage;

    #endregion

    #region Commands

    public ICommand RefreshCommand { get; }
    public ICommand OpenModDocsCommand { get; }
    public ICommand LoadChunkCommand { get; }

    #endregion

    #region Public Properties

    public GameInstallation GameInstallation { get; }

    public ObservableCollection<DownloadableModsSourceViewModel> DownloadableModsSources { get; }

    public DownloadableModViewModel? SelectedMod { get; set; }
    public ObservableCollection<DownloadableModViewModel> Mods { get; }
    public bool IsEmpty { get; set; }
    public bool IsLoading { get; set; }
    public string? ErrorMessage { get; set; }
    public bool CanLoadChunk { get; set; }

    #endregion

    #region Private Methods

    private async Task LoadPageAsync()
    {
        Logger.Info("Loading page {0}", _currentPage);

        foreach (DownloadableModsSourceViewModel modsSource in DownloadableModsSources)
        {
            DownloadableModsFeed feed = await modsSource.Source.LoadDownloadableModsAsync(_modLoaderViewModel, _httpClient, GameInstallation, _currentPage);
            Mods.AddRange(feed.DownloadableMods);
            _pageCount = feed.PageCount;
        }
        
        Logger.Info("Loaded page {0} out of {1} total pages", _currentPage, _pageCount);
    }

    private async Task LoadChunkAsync()
    {
        if (IsLoading)
            return;

        IsLoading = true;

        Logger.Info("Loaded chunk of downloadable mods");

        try
        {
            do
            {
                _currentPage++;
                await LoadPageAsync();
            } while (Mods.Count < ChunkSize && _currentPage < _pageCount - 1);

            CanLoadChunk = _pageCount != -1 && _currentPage < _pageCount - 1;

            IsEmpty = !Mods.Any();
            Logger.Info("Loaded chunk");
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Loading chunk of downloadable mods");

            ErrorMessage = ex.Message;
            IsEmpty = false;
            CanLoadChunk = false;
        }
        finally
        {
            IsLoading = false;
        }
    }

    #endregion

    #region Public Methods

    public async Task LoadModsAsync()
    {
        if (IsLoading)
            return;

        Logger.Info("Loading downloadable mods from {0} sources", DownloadableModsSources.Count);

        IsEmpty = false;
        ErrorMessage = null;
        Mods.Clear();
        _currentPage = -1;
        _pageCount = -1;
        CanLoadChunk = false;

        await LoadChunkAsync();
    }

    public void OpenModDocs()
    {
        Services.App.OpenUrl("https://github.com/RayCarrot/RayCarrot.RCP.Metro/wiki/Mod-Loader");
    }

    #endregion
}