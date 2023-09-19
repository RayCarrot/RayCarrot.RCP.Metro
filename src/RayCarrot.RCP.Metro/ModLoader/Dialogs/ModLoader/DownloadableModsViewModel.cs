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
    }

    #endregion

    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Private Fields

    private readonly HttpClient _httpClient;
    private readonly ModLoaderViewModel _modLoaderViewModel;

    #endregion

    #region Commands

    public ICommand RefreshCommand { get; }
    public ICommand OpenModDocsCommand { get; }

    #endregion

    #region Public Properties

    public GameInstallation GameInstallation { get; }

    public ObservableCollection<DownloadableModsSourceViewModel> DownloadableModsSources { get; }

    public DownloadableModViewModel? SelectedMod { get; set; }
    public ObservableCollection<DownloadableModViewModel> Mods { get; }
    public bool IsEmpty { get; set; }
    public bool IsLoading { get; set; }
    public string? ErrorMessage { get; set; }

    #endregion

    #region Public Methods

    public async Task LoadModsAsync()
    {
        if (IsLoading)
            return;

        Logger.Info("Loading downloadable mods from {0} sources", DownloadableModsSources.Count);

        IsLoading = true;
        IsEmpty = false;
        ErrorMessage = null;
        Mods.Clear();

        try
        {
            foreach (DownloadableModsSourceViewModel modsSource in DownloadableModsSources)
                await modsSource.Source.LoadDownloadableModsAsync(_modLoaderViewModel, _httpClient, GameInstallation, Mods);

            IsEmpty = !Mods.Any();
            Logger.Info("Loaded {0} downloadable mods", Mods.Count);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Loaded downloadable mods");

            ErrorMessage = ex.Message;
            IsEmpty = false;
        }
        finally
        {
            IsLoading = false;
        }
    }

    public void OpenModDocs()
    {
        Services.App.OpenUrl("https://github.com/RayCarrot/RayCarrot.RCP.Metro/wiki/Mod-Loader");
    }

    #endregion
}