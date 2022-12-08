using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nito.AsyncEx;
using NLog;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// View model for a game options dialog
/// </summary>
public class GameOptionsDialog_ViewModel : BaseRCPViewModel, IDisposable
{
    #region Constructor

    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="gameInstallation">The game installation to show the options for</param>
    public GameOptionsDialog_ViewModel(GameInstallation gameInstallation)
    {
        // Get the info
        GameDescriptor gameDescriptor = gameInstallation.GameDescriptor;

        // Set properties
        GameInstallation = gameInstallation;
        DisplayName = gameDescriptor.DisplayName;
        Icon = gameDescriptor.Icon;
        IsDemo = gameDescriptor.IsDemo;
        PageLoadLock = new AsyncLock();

        // Create the page collection
        List<GameOptionsDialog_BasePageViewModel> pages = new();

        // Add the options page
        pages.Add(new GameOptionsDialog_OptionsPageViewModel(gameInstallation));

        // Add the config page
        GameOptionsDialog_ConfigPageViewModel? configViewModel = gameDescriptor.GetConfigPageViewModel(gameInstallation);

        if (configViewModel != null)
            pages.Add(configViewModel);

        // TODO-14: This has to be changed since the emulator selection can change after this has been set
        // Add the emulator config page
        //Emulator? emu = gameDescriptor.Emulator;
        //GameOptionsDialog_EmulatorConfigPageViewModel? emuConfigViewModel = emu?.GetGameConfigViewModel(gameInstallation);

        //if (emuConfigViewModel != null)
        //    pages.Add(emuConfigViewModel);

        // Add the utilities page
        UtilityViewModel[] utilities = gameDescriptor.GetUtilities(gameInstallation).Select(x => new UtilityViewModel(x)).ToArray();

        if (utilities.Any())
            pages.Add(new GameOptionsDialog_UtilitiesPageViewModel(utilities));

        Pages = pages.ToArray();

        SelectedPage = Pages.First();
    }

    #endregion

    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Public Properties

    /// <summary>
    /// The available options pages
    /// </summary>
    public GameOptionsDialog_BasePageViewModel[] Pages { get; }

    /// <summary>
    /// The currently selected page
    /// </summary>
    public GameOptionsDialog_BasePageViewModel SelectedPage { get; set; }

    /// <summary>
    /// The game installation
    /// </summary>
    public GameInstallation GameInstallation { get; }

    /// <summary>
    /// The display name
    /// </summary>
    public string DisplayName { get; } // TODO: LocalizedString

    /// <summary>
    /// The icon
    /// </summary>
    public GameIconAsset Icon { get; }

    /// <summary>
    /// Indicates if the game is a demo
    /// </summary>
    public bool IsDemo { get; }

    public AsyncLock PageLoadLock { get; }

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
            GameOptionsDialog_BasePageViewModel page = SelectedPage;

            // Ignore if already loaded
            if (page.IsLoaded)
                return;

            try
            {
                IsLoading = true;

                // Load the page
                await page.LoadPageAsync();

                Logger.Info("Loaded {0} page", page.PageName);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Loading page {0}", page);

                page.SetErrorState(ex);
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