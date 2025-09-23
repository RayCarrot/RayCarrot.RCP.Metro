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

        Categories = new ObservableCollectionEx<DownloadableModsCategoryViewModel>();

        // Add main category for all mods. No need to localize since the rest of the category names aren't localized.
        Categories.Insert(0, new DownloadableModsCategoryViewModel("All", null, null));
        SelectedCategory = Categories[0];

        ClearSelectionCommand = new RelayCommand(ClearSelection);
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

    public ICommand ClearSelectionCommand { get; }
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

    public bool HasCategories { get; set; }
    public ObservableCollectionEx<DownloadableModsCategoryViewModel> Categories { get; }
    public DownloadableModsCategoryViewModel SelectedCategory { get; set; }

    public DownloadableModViewModel? SelectedMod
    {
        get => _selectedMod;
        set
        {
            _selectedMod = value;

            // TODO-UPDATE: Await this (probably call from ICommand instead)
            _selectedMod?.LoadAsync();
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
            // Clear all except the first item
            Categories.ModifyCollection(x => x.RemoveRange(1, x.Count - 1));

            foreach (DownloadableModsSourceViewModel source in DownloadableModsSources)
            {
                foreach (DownloadableModsCategoryViewModel cat in await source.Source.LoadDownloadableModsCategoriesAsync(_httpClient, GameInstallation))
                {
                    Categories.Add(cat);
                }
            }

            HasCategories = Categories.Count > 1;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Loading categories");
            HasCategories = false;
        }
    }

    public void ClearSelection()
    {
        SelectedMod = null;
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
            await Services.MessageUI.DisplayMessageAsync(Resources.ModLoader_TooShortSearchText, MessageType.Error);
            return;
        }

        Logger.Info("Loading downloadable mods from the search text {0}", SearchText);

        SelectedCategory = Categories[0];
        CurrentFeedType = FeedType.Search;
        FeedInfoText = new ResourceLocString(nameof(Resources.ModLoader_SearchFeedInfo), SearchText);

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
        FeedInfoText = new ResourceLocString(nameof(Resources.ModLoader_CategoryFeedInfo), SelectedCategory.Name);

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