using System.Diagnostics.CodeAnalysis;
using Nito.AsyncEx;
using RayCarrot.RCP.Metro.Games.Components;

namespace RayCarrot.RCP.Metro.Games.OptionsDialog;

/// <summary>
/// View model for a game options dialog
/// </summary>
public class GameOptionsDialogViewModel : BaseRCPViewModel, IInitializable, IDisposable, IRecipient<ModifiedGamesMessage>
{
    #region Constructor

    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="gameInstallation">The game installation to show the options for</param>
    public GameOptionsDialogViewModel(GameInstallation gameInstallation)
    {
        // Set properties
        GameInstallation = gameInstallation;
        PageLoadLock = new AsyncLock();
        CachedPages = new Dictionary<string, GameOptionsDialogPageViewModel>();

        CreatePages();
    }

    #endregion

    #region Private Properties

    private AsyncLock PageLoadLock { get; }
    private Dictionary<string, GameOptionsDialogPageViewModel> CachedPages { get; }

    #endregion

    #region Public Properties

    /// <summary>
    /// The available options pages
    /// </summary>
    public ObservableCollection<GameOptionsDialogPageViewModel> Pages { get; set; }

    /// <summary>
    /// The currently selected page
    /// </summary>
    public GameOptionsDialogPageViewModel? SelectedPage { get; set; }

    public GameInstallation GameInstallation { get; }
    public GameDescriptor GameDescriptor => GameInstallation.GameDescriptor;

    public bool IsLoading { get; set; }

    #endregion

    #region Private Methods

    [MemberNotNull(nameof(Pages))]
    private void CreatePages()
    {
        // Attempt to keep the selection
        int selectedIndex = -1;
        if (Pages != null && SelectedPage != null)
            selectedIndex = Pages.IndexOf(SelectedPage);

        // We have to recreate the pages on each refresh as they might have changed (if say a game client was attached/detached)
        var pages = GameInstallation.GetComponents<GameOptionsDialogPageComponent>().
            Where(x => x.IsAvailable()).
            Select(x =>
            {
                // Cache pages to avoid recreating the same page
                string instanceId = x.GetInstanceId();

                if (!CachedPages.TryGetValue(instanceId, out GameOptionsDialogPageViewModel page))
                {
                    page = x.CreateObject();
                    CachedPages.Add(instanceId, page);
                }

                return page;
            });
        Pages = new ObservableCollection<GameOptionsDialogPageViewModel>(pages);
        SelectedPage = Pages.ElementAtOrDefault(selectedIndex) ?? Pages.FirstOrDefault();
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Loads the current page
    /// </summary>
    public async Task LoadCurrentPageAsync()
    {
        using (await PageLoadLock.LockAsync())
        {
            // Get the selected page
            GameOptionsDialogPageViewModel? page = SelectedPage;

            // Ignore if already loaded or null
            if (page == null || page.IsLoaded)
                return;

            try
            {
                IsLoading = true;

                // Load the page
                await page.LoadPageAsync();
            }
            finally
            {
                // Set the page as loaded
                page.IsLoaded = true;

                IsLoading = false;
            }
        }
    }

    public void Initialize()
    {
        Services.Messenger.RegisterAll(this);
    }

    public void Deinitialize()
    {
        Services.Messenger.UnregisterAll(this);
    }

    public void Receive(ModifiedGamesMessage message)
    {
        // If the components were rebuilt we have to re-create the
        // pages too since they might have changed
        if (message.RebuiltComponents)
            CreatePages();
    }

    /// <summary>
    /// Disposes the view model
    /// </summary>
    public void Dispose()
    {
        // Dispose
        Pages.DisposeAll();
    }

    #endregion
}