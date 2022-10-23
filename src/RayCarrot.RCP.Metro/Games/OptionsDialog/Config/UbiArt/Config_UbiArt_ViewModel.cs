using Microsoft.Win32;
using NLog;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// View model for the Rayman Origins and Rayman Legends configuration
/// </summary>
public class Config_UbiArt_ViewModel : GameOptionsDialog_ConfigPageViewModel
{
    #region Constructor

    public Config_UbiArt_ViewModel(GameInstallation gameInstallation, string registryKey)
    {
        // Get the game
        GameInstallation = gameInstallation;
        RegistryKey = registryKey;
    }

    #endregion

    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Private Fields

    private bool _fullscreenMode;

    #endregion

    #region Private Constants

    private const string ScreenHeightKey = "ScreenHeight";

    private const string ScreenWidthKey = "ScreenWidth";

    private const string FullScreenKey = "FullScreen";

    #endregion

    #region Public Properties

    /// <summary>
    /// The game installation
    /// </summary>
    public GameInstallation GameInstallation { get; }

    /// <summary>
    /// The registry key for the game
    /// </summary>
    public string RegistryKey { get; }

    /// <summary>
    /// True if the game should run in fullscreen,
    /// false if it should run in windowed mode
    /// </summary>
    public bool FullscreenMode
    {
        get => _fullscreenMode;
        set
        {
            _fullscreenMode = value;
            UnsavedChanges = true;
        }
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Gets the registry key for the game properties
    /// </summary>
    /// <param name="writable">True if the key should be writable</param>
    /// <returns>The key</returns>
    private RegistryKey? GetKey(bool writable)
    {
        // Get the key path
        string keyPath = RegistryHelpers.CombinePaths(RegistryKey, "Settings");

        // Create the key if it doesn't exist and should be written to
        if (!RegistryHelpers.KeyExists(keyPath) && writable)
        {
            RegistryKey? key = RegistryHelpers.CreateRegistryKey(keyPath, RegistryView.Default, true);

            Logger.Info("The Registry key {0} has been created", key?.Name);

            return key;
        }

        return RegistryHelpers.GetKeyFromFullPath(keyPath, RegistryView.Default, writable);
    }

    #endregion

    #region Protected Methods

    protected override object GetPageUI() => new Config_UbiArt_Control()
    {
        DataContext = this
    };

    /// <summary>
    /// Loads and sets up the current configuration properties
    /// </summary>
    /// <returns>The task</returns>
    protected override Task LoadAsync()
    {
        Logger.Info("{0} config is being set up", GameInstallation.ID);

        using (RegistryKey? key = GetKey(false))
        {
            if (key != null)
                Logger.Info("The key {0} has been opened", key.Name);
            else
                Logger.Info("The key for {0} does not exist. Default values will be used.", GameInstallation.ID);

            GraphicsMode.GetAvailableResolutions();
            GraphicsMode.SelectedGraphicsMode = new GraphicsMode(
                width: GetInt(ScreenWidthKey, (int)SystemParameters.PrimaryScreenWidth), 
                height: GetInt(ScreenHeightKey, (int)SystemParameters.PrimaryScreenHeight));
            FullscreenMode = GetInt(FullScreenKey, 1) == 1;

            // Helper methods for getting values
            int GetInt(string valueName, int defaultValue) => Int32.TryParse(key?.GetValue(valueName, defaultValue).ToString().KeepFirstDigitsOnly(), out int result) ? result : defaultValue;
        }

        UnsavedChanges = false;

        Logger.Info("All values have been loaded");

        return Task.CompletedTask;
    }

    /// <summary>
    /// Saves the changes
    /// </summary>
    /// <returns>The task</returns>
    protected override async Task<bool> SaveAsync()
    {
        Logger.Info("{0} configuration is saving...", GameInstallation.ID);

        try
        {
            using (RegistryKey? key = GetKey(true))
            {
                if (key == null)
                    throw new Exception("The Registry key could not be created");

                Logger.Info("The key {0} has been opened", key.Name);

                key.SetValue(ScreenWidthKey, GraphicsMode.Width.ToString());
                key.SetValue(ScreenHeightKey, GraphicsMode.Height.ToString());
                key.SetValue(FullScreenKey, FullscreenMode ? 1 : 0);
            }

            Logger.Info("{0} configuration has been saved", GameInstallation.ID);

            return true;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Saving {0} registry data", GameInstallation.ID);
            await Services.MessageUI.DisplayExceptionMessageAsync(ex, String.Format(Resources.Config_SaveError, GameInstallation.GameInfo.DisplayName), Resources.Config_SaveErrorHeader);
            return false;
        }
    }

    #endregion
}