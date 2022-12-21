using System.Diagnostics.CodeAnalysis;
using Nito.AsyncEx;
using RayCarrot.RCP.Metro.Games.Components;

namespace RayCarrot.RCP.Metro.Games.OptionsDialog;

/// <summary>
/// View model for a game options dialog
/// </summary>
public class GameOptionsDialogViewModel : BaseRCPViewModel, IRecipient<ModifiedGamesMessage>, IDisposable
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

        Refresh();

        var pages = gameInstallation.GameDescriptor.GetComponents<GameOptionsDialogPageComponent>().
            Where(x => x.IsAvailable(gameInstallation)).
            Select(x => x.CreateObject(gameInstallation));
        Pages = new ObservableCollection<GameOptionsDialogPageViewModel>(pages);
        SelectedPage = Pages.First();

        Services.Messenger.RegisterAll(this);
    }

    #endregion

    #region Private Properties

    private AsyncLock PageLoadLock { get; }

    #endregion

    #region Public Properties

    /// <summary>
    /// The available options pages
    /// </summary>
    public ObservableCollection<GameOptionsDialogPageViewModel> Pages { get; }

    /// <summary>
    /// The currently selected page
    /// </summary>
    public GameOptionsDialogPageViewModel SelectedPage { get; set; }

    public GameInstallation GameInstallation { get; }
    public GameDescriptor GameDescriptor => GameInstallation.GameDescriptor;
    public LocalizedString DisplayName { get; set; }
    public GameIconAsset Icon => GameDescriptor.Icon;
    public bool IsDemo => GameDescriptor.IsDemo;

    public bool IsLoading { get; set; }

    #endregion

    #region Private Methods

    [MemberNotNull(nameof(DisplayName))]
    private void Refresh()
    {
        DisplayName = GameInstallation.GetDisplayName();
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
            GameOptionsDialogPageViewModel page = SelectedPage;

            // Ignore if already loaded
            if (page.IsLoaded)
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

    public void Receive(ModifiedGamesMessage message) => Refresh();

    /// <summary>
    /// Disposes the view model
    /// </summary>
    public void Dispose()
    {
        // Dispose
        Pages.DisposeAll();

        Services.Messenger.UnregisterAll(this);
    }

    #endregion
}