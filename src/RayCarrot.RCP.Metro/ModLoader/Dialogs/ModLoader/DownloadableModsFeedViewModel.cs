using System.Net.Http;
using RayCarrot.RCP.Metro.ModLoader.Sources;

namespace RayCarrot.RCP.Metro.ModLoader.Dialogs.ModLoader;

public class DownloadableModsFeedViewModel : BaseViewModel, IDisposable
{
    #region Constructor

    public DownloadableModsFeedViewModel(
        ModLoaderViewModel modLoaderViewModel, 
        GameInstallation gameInstallation, 
        HttpClient httpClient, 
        IReadOnlyList<DownloadableModsSource> downloadableModsSources)
    {
        _modLoaderViewModel = modLoaderViewModel;
        GameInstallation = gameInstallation;
        _httpClient = httpClient;
        _downloadableModsSources = downloadableModsSources;
    }

    #endregion

    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Constants

    private const int ChunkSize = 10;

    #endregion

    #region Private Fields

    private readonly ModLoaderViewModel _modLoaderViewModel;
    private readonly HttpClient _httpClient;
    private readonly IReadOnlyList<DownloadableModsSource> _downloadableModsSources;
    private int _pageCount;
    private int _currentPage;

    #endregion

    #region Public Properties

    public GameInstallation GameInstallation { get; }

    public ObservableCollection<DownloadableModViewModel> Mods { get; } = new();
    
    public bool IsEmpty { get; set; }
    public string? ErrorMessage { get; set; }
    public bool CanLoadChunk { get; set; }

    #endregion

    #region Private Methods

    private async Task LoadCurrentPageAsync()
    {
        Logger.Info("Loading page {0}", _currentPage);

        foreach (DownloadableModsSource modsSource in _downloadableModsSources)
        {
            DownloadableModsFeedPage feedPage = await modsSource.LoadDownloadableModsAsync(_modLoaderViewModel, Mods, _httpClient, GameInstallation, _currentPage);
            Mods.AddRange(feedPage.DownloadableMods);
            _pageCount = feedPage.PageCount;
        }

        Logger.Info("Loaded page {0} out of {1} total pages", _currentPage, _pageCount);
    }

    #endregion

    #region Public Methods

    public void Initialize()
    {
        IsEmpty = false;
        ErrorMessage = null;
        Mods.DisposeAll();
        Mods.Clear();
        _currentPage = -1;
        _pageCount = -1;
        CanLoadChunk = false;
    }

    public async Task LoadNextChunkAsync()
    {
        try
        {
            do
            {
                _currentPage++;
                await LoadCurrentPageAsync();
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
    }

    public void Dispose()
    {
        Mods.DisposeAll();
    }

    #endregion
}