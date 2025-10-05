using System.Net.Http;
using System.Windows.Input;
using Nito.AsyncEx;
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
        if (downloadableModsSources.Count > 1)
            throw new InvalidOperationException("Multiple mod sources is currently not supported");

        _asyncLock = new AsyncLock();
        GameInstallation = gameInstallation;
        _httpClient = httpClient;

        DownloadableModsSources = new ObservableCollection<DownloadableModsSourceViewModel>(
            downloadableModsSources.Select(x => new DownloadableModsSourceViewModel(x)));

        ModsFeed = new DownloadableModsFeedViewModel(modLoaderViewModel, gameInstallation, httpClient, downloadableModsSources[0]);

        Categories = new ObservableCollectionEx<DownloadableModsCategoryViewModel>();

        // Add main category for all mods. No need to localize since the rest of the category names aren't localized.
        Categories.Insert(0, new DownloadableModsCategoryViewModel("All", null, null));
        SelectedCategory = Categories[0];

        SelectModCommand = new AsyncRelayCommand(x => SelectModAsync((DownloadableModViewModel?)x));
        ClearSelectionCommand = new RelayCommand(ClearSelection);
        RefreshCommand = new AsyncRelayCommand(LoadDefaultFeedAsync);
        SearchCommand = new AsyncRelayCommand(LoadSearchFeedAsync);
        SelectCategoryCommand = new AsyncRelayCommand(LoadCategoryAndSortFeedAsync);
    }

    #endregion

    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Private Fields

    private readonly AsyncLock _asyncLock;
    private readonly HttpClient _httpClient;

    #endregion

    #region Events

    public event EventHandler? FeedInitialized;

    #endregion

    #region Commands

    public ICommand SelectModCommand { get; }
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

    public DownloadableModViewModel? SelectedMod { get; set; }

    public bool IsLoading { get; set; }

    public string SearchText { get; set; } = String.Empty;

    #endregion

    #region Private Methods

    private void OnFeedInitialized()
    {
        FeedInitialized?.Invoke(this, EventArgs.Empty);
    }

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

    public async Task SelectModAsync(DownloadableModViewModel? mod)
    {
        // Don't allow selecting placeholder mods
        if (mod is PlaceholderDownloadableModViewModel)
            return;

        SelectedMod = mod;

        if (mod != null)
            await mod.LoadAsync();
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

        await ModsFeed.InitializeAsync(null);
        OnFeedInitialized();
        await LoadNextPageAsync();
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

        await ModsFeed.InitializeAsync(new DownloadableModsFeedSearchTextFilter()
        {
            SearchText = SearchText,
        });
        OnFeedInitialized();
        await LoadNextPageAsync();
    }

    public async Task LoadCategoryAndSortFeedAsync()
    {
        if (IsLoading)
            return;

        if (SelectedCategory == Categories[0])
        {
            await LoadDefaultFeedAsync();
            return;
        }

        SearchText = String.Empty;
        CurrentFeedType = FeedType.CategoryAndSort;
        FeedInfoText = new ResourceLocString(nameof(Resources.ModLoader_CategoryFeedInfo), SelectedCategory.Name);

        await ModsFeed.InitializeAsync(new DownloadableModsFeedCategoryAndSortFilter()
        {
            Category = SelectedCategory.Id,
            Order = null,
        });
        OnFeedInitialized();
        await LoadNextPageAsync();
    }

    public async Task LoadNextPageAsync()
    {
        using (await _asyncLock.LockAsync())
        {
            IsLoading = true;

            Logger.Info("Loading chunk of downloadable mods for feed type {0}", CurrentFeedType);

            try
            {
                await ModsFeed.LoadNextPageAsync();
            }
            finally
            {
                IsLoading = false;
            }
        }
    }

    public async Task LoadNextPageFromPlaceholderAsync(PlaceholderDownloadableModViewModel placeholder)
    {
        using (await _asyncLock.LockAsync())
        {
            if (!ModsFeed.IsPlaceholderForNextPage(placeholder))
                return;

            IsLoading = true;

            Logger.Info("Loading chunk of downloadable mods for feed type {0}", CurrentFeedType);

            try
            {
                await ModsFeed.LoadNextPageAsync();
            }
            finally
            {
                IsLoading = false;
            }
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
        CategoryAndSort,
    }

    #endregion
}