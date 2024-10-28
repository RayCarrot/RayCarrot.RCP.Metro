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
        _httpClient = httpClient;

        DownloadableModsSources = new ObservableCollection<DownloadableModsSourceViewModel>(
            downloadableModsSources.Select(x => new DownloadableModsSourceViewModel(x)));

        ModsFeed = new DownloadableModsFeedViewModel(modLoaderViewModel, gameInstallation, httpClient, downloadableModsSources);

        Categories = new ObservableCollection<DownloadableModsCategoryViewModel>();

        // Add main category for all mods. No need to localize since the rest of the category names aren't localized.
        Categories.Insert(0, new DownloadableModsCategoryViewModel("All", null, null));
        SelectedCategory = Categories[0];

        RefreshCommand = new AsyncRelayCommand(LoadDefaultFeedAsync);
        SearchCommand = new AsyncRelayCommand(LoadSearchFeedAsync);
        SelectCategoryCommand = new AsyncRelayCommand(LoadCategoryFeedAsync);
    }

    #endregion

    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Private Fields

    private readonly HttpClient _httpClient;
    private DownloadableModViewModel? _selectedMod;

    #endregion

    #region Commands

    public ICommand RefreshCommand { get; }
    public ICommand SearchCommand { get; }
    public ICommand SelectCategoryCommand { get; }

    #endregion

    #region Public Properties

    public GameInstallation GameInstallation { get; }
    public ObservableCollection<DownloadableModsSourceViewModel> DownloadableModsSources { get; }
    public DownloadableModsFeedViewModel ModsFeed { get; }
    public FeedType CurrentFeedType { get; set; }
    public LocalizedString? FeedInfoText { get; set; }

    public ObservableCollection<DownloadableModsCategoryViewModel> Categories { get; }
    public DownloadableModsCategoryViewModel SelectedCategory { get; set; }

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

    #endregion

    #region Public Methods

    public async Task InitializeAsync()
    {
        Logger.Info("Initializing downloadable mods from {0} sources", DownloadableModsSources.Count);

        await Task.WhenAll(
            LoadDefaultFeedAsync(), 
            LoadCategoriesAsync());
    }

    public async Task LoadCategoriesAsync()
    {
        try
        {
            foreach (DownloadableModsSourceViewModel source in DownloadableModsSources)
            {
                foreach (DownloadableModsCategoryViewModel cat in await source.Source.LoadDownloadableModsCategoriesAsync(_httpClient, GameInstallation))
                {
                    Categories.Add(cat);
                }
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Loading categories");
        }
    }

    public async Task LoadDefaultFeedAsync()
    {
        if (IsLoading)
            return;

        Logger.Info("Loading full feed of downloadable mods");

        SearchText = String.Empty;
        SelectedCategory = Categories[0];
        CurrentFeedType = FeedType.Default;
        FeedInfoText = null;

        ModsFeed.Initialize(null);
        await LoadNextChunkAsync();
    }

    public async Task LoadSearchFeedAsync()
    {
        if (IsLoading)
            return;

        if (SearchText.Length == 0)
        {
            await LoadDefaultFeedAsync();
            return;
        }

        // Search text has to be at least 2 characters
        if (SearchText.Length < 2)
        {
            // TODO-LOC
            await Services.MessageUI.DisplayMessageAsync("The search text has to have at least 2 characters", MessageType.Error);
            return;
        }

        Logger.Info("Loading downloadable mods from the search text {0}", SearchText);

        SelectedCategory = Categories[0];
        CurrentFeedType = FeedType.Search;
        FeedInfoText = $"Showing search results for \"{SearchText}\""; // TODO-LOC

        ModsFeed.Initialize(new DownloadableModsFeedSearchTextFilter(SearchText));
        await LoadNextChunkAsync();
    }

    public async Task LoadCategoryFeedAsync()
    {
        if (IsLoading)
            return;

        if (SelectedCategory == Categories[0])
        {
            await LoadDefaultFeedAsync();
            return;
        }

        SearchText = String.Empty;
        CurrentFeedType = FeedType.Category;
        FeedInfoText = $"Showing mods in the category \"{SelectedCategory.Name}\""; // TODO-LOC

        ModsFeed.Initialize(SelectedCategory.Filter);
        await LoadNextChunkAsync();
    }

    public async Task LoadNextChunkAsync()
    {
        if (IsLoading)
            return;

        IsLoading = true;

        Logger.Info("Loading chunk of downloadable mods for feed type {0}", CurrentFeedType);

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

    #region Data Types

    public enum FeedType
    {
        Default,
        Search,
        Category,
    }

    #endregion
}