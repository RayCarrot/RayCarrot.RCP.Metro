using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Nito.AsyncEx;
using NLog;

namespace RayCarrot.RCP.Metro.Games.OptionsDialog;

/// <summary>
/// View model for a game options dialog
/// </summary>
public class GameOptionsDialogViewModel : BaseRCPViewModel, IDisposable
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
        Pages = new ObservableCollection<GameOptionsDialogPageViewModel>(GameDescriptor.GetGameOptionsDialogPages(gameInstallation));
        SelectedPage = Pages.First();
    }

    #endregion

    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

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
    public string DisplayName => GameDescriptor.DisplayName; // TODO: LocalizedString
    public GameIconAsset Icon => GameDescriptor.Icon;
    public bool IsDemo => GameDescriptor.IsDemo;

    public bool IsLoading { get; set; }

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