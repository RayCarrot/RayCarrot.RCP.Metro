using Microsoft.Win32;
using NLog;
using RayCarrot.Windows.Registry;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// View model for the Rayman Origins and Rayman Legends configuration
    /// </summary>
    public class Config_UbiArt_ViewModel : GameOptionsDialog_ConfigPageViewModel
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public Config_UbiArt_ViewModel(Games game)
        {
            // Check the game
            if (game != Games.RaymanOrigins && game != Games.RaymanLegends)
                throw new ArgumentOutOfRangeException(nameof(game), game, $"{nameof(Config_UbiArt_ViewModel)} only supports Rayman Origins or Rayman Legends");

            // Get the game
            Game = game;
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
        /// The game
        /// </summary>
        public Games Game { get; }

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
        private RegistryKey GetKey(bool writable)
        {
            // Get the key path
            var keyPath = RegistryHelpers.CombinePaths(Game == Games.RaymanOrigins ? AppFilePaths.RaymanOriginsRegistryKey : AppFilePaths.RaymanLegendsRegistryKey, "Settings");

            // Create the key if it doesn't exist and should be written to
            if (!RegistryHelpers.KeyExists(keyPath) && writable)
            {
                var key = RegistryHelpers.CreateRegistryKey(keyPath, RegistryView.Default, true);

                Logger.Info("The Registry key {0} has been created", key?.Name);

                return key;
            }

            return RegistryHelpers.GetKeyFromFullPath(keyPath, RegistryView.Default, writable);
        }

        #endregion

        #region Protected Methods

        protected override object GetPageUI() => new Config_UbiArt_UI()
        {
            DataContext = this
        };

        /// <summary>
        /// Loads and sets up the current configuration properties
        /// </summary>
        /// <returns>The task</returns>
        protected override Task LoadAsync()
        {
            Logger.Info("{0} config is being set up", Game);

            using (var key = GetKey(false))
            {
                Logger.Info(key != null
                    ? $"The key {key.Name} has been opened"
                    : $"The key for {Game} does not exist. Default values will be used.");

                Resolution.GetAvailableResolutions(800, 600);
                Resolution.Width = GetInt(ScreenWidthKey, (int)SystemParameters.PrimaryScreenWidth);
                Resolution.Height = GetInt(ScreenHeightKey, (int)SystemParameters.PrimaryScreenHeight);
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
            Logger.Info("{0} configuration is saving...", Game);

            try
            {
                using (var key = GetKey(true))
                {
                    if (key == null)
                        throw new Exception("The Registry key could not be created");

                    Logger.Info("The key {0} has been opened", key.Name);

                    key.SetValue(ScreenWidthKey, Resolution.Width.ToString());
                    key.SetValue(ScreenHeightKey, Resolution.Height.ToString());
                    key.SetValue(FullScreenKey, FullscreenMode ? 1 : 0);
                }

                Logger.Info("{0} configuration has been saved", Game);

                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Saving {0} registry data", Game);
                await Services.MessageUI.DisplayExceptionMessageAsync(ex, String.Format(Resources.Config_SaveError, Game.GetGameInfo().DisplayName), Resources.Config_SaveErrorHeader);
                return false;
            }
        }

        #endregion
    }
}