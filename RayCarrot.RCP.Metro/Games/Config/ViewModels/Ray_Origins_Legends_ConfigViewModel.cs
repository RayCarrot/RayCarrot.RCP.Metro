using System;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;
using Nito.AsyncEx;
using RayCarrot.CarrotFramework.Abstractions;
using RayCarrot.Logging;
using RayCarrot.UI;
using RayCarrot.Windows.Registry;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// View model for the Rayman Origins and Rayman Legends configuration
    /// </summary>
    public class Ray_Origins_Legends_ConfigViewModel : GameConfigViewModel
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public Ray_Origins_Legends_ConfigViewModel(Games game)
        {
            // Check the game
            if (game != Games.RaymanOrigins && game != Games.RaymanLegends)
                throw new ArgumentOutOfRangeException(nameof(game), game, $"{nameof(Ray_Origins_Legends_ConfigViewModel)} only supports Rayman Origins or Rayman Legends");

            // Get the game
            Game = game;

            // Create commands
            SaveCommand = new AsyncRelayCommand(SaveAsync);

            // Create properties
            AsyncLock = new AsyncLock();
        }

        #endregion

        #region Private Fields

        private int _screenHeight;

        private int _screenWidth;

        private bool _lockToScreenRes;

        private bool _fullscreenMode;

        #endregion

        #region Private Constants

        private const string ScreenHeightKey = "ScreenHeight";

        private const string ScreenWidthKey = "ScreenWidth";

        private const string FullScreenKey = "FullScreen";

        #endregion

        #region Private Properties

        /// <summary>
        /// The async lock to use for saving the configuration
        /// </summary>
        protected AsyncLock AsyncLock { get; }

        #endregion

        #region Public Properties

        /// <summary>
        /// The game
        /// </summary>
        public Games Game { get; }

        /// <summary>
        /// The screen height
        /// </summary>
        public int ScreenHeight
        {
            get => _screenHeight;
            set
            {
                _screenHeight = value;
                UnsavedChanges = true;
            }
        }

        /// <summary>
        /// The screen width
        /// </summary>
        public int ScreenWidth
        {
            get => _screenWidth;
            set
            {
                _screenWidth = value;
                UnsavedChanges = true;
            }
        }

        /// <summary>
        /// Indicates if the resolution is locked to the current screen resolution
        /// </summary>
        public bool LockToScreenRes
        {
            get => _lockToScreenRes;
            set
            {
                _lockToScreenRes = value;

                if (!value)
                    return;

                ScreenHeight = (int)SystemParameters.PrimaryScreenHeight;
                ScreenWidth = (int)SystemParameters.PrimaryScreenWidth;
            }
        }

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

        #region Commands

        public AsyncRelayCommand SaveCommand { get; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Loads and sets up the current configuration properties
        /// </summary>
        /// <returns>The task</returns>
        public override Task SetupAsync()
        {
            RL.Logger?.LogInformationSource($"{Game} config is being set up");

            using (var key = GetKey(false))
            {
                RL.Logger?.LogInformationSource(key != null
                    ? $"The key {key.Name} has been opened"
                    : $"The key for {Game} does not exist. Default values will be used.");

                ScreenHeight = GetInt(ScreenHeightKey, (int)SystemParameters.PrimaryScreenHeight);
                ScreenWidth = GetInt(ScreenWidthKey, (int)SystemParameters.PrimaryScreenWidth);
                FullscreenMode = GetInt(FullScreenKey, 1) == 1;

                // Helper methods for getting values
                int GetInt(string valueName, int defaultValue) => Int32.TryParse(key?.GetValue(valueName, defaultValue).ToString().KeepFirstDigitsOnly(), out int result) ? result : defaultValue;
            }

            UnsavedChanges = false;

            RL.Logger?.LogInformationSource($"All values have been loaded");

            return Task.CompletedTask;
        }

        /// <summary>
        /// Saves the changes
        /// </summary>
        /// <returns>The task</returns>
        public async Task SaveAsync()
        {
            using (await AsyncLock.LockAsync())
            {
                RL.Logger?.LogInformationSource($"{Game} configuration is saving...");

                try
                {
                    using (var key = GetKey(true))
                    {
                        if (key == null)
                            throw new Exception("The Registry key could not be created");

                        RL.Logger?.LogInformationSource($"The key {key.Name} has been opened");

                        key.SetValue(ScreenHeightKey, ScreenHeight.ToString());
                        key.SetValue(ScreenWidthKey, ScreenWidth.ToString());
                        key.SetValue(FullScreenKey, FullscreenMode ? 1 : 0);
                    }

                    RL.Logger?.LogInformationSource($"{Game} configuration has been saved");
                }
                catch (Exception ex)
                {
                    ex.HandleError($"Saving {Game} registry data");
                    await RCFUI.MessageUI.DisplayExceptionMessageAsync(ex, String.Format(Resources.Config_SaveError, Game.GetGameInfo().DisplayName), Resources.Config_SaveErrorHeader);
                    return;
                }

                UnsavedChanges = false;

                await RCFUI.MessageUI.DisplaySuccessfulActionMessageAsync(Resources.Config_SaveSuccess);

                OnSave();
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
            var keyPath = RCFWinReg.RegistryManager.CombinePaths(Game == Games.RaymanOrigins ? CommonPaths.RaymanOriginsRegistryKey : CommonPaths.RaymanLegendsRegistryKey, "Settings");

            // Create the key if it doesn't exist and should be written to
            if (!RCFWinReg.RegistryManager.KeyExists(keyPath) && writable)
            {
                var key = RCFWinReg.RegistryManager.CreateRegistryKey(keyPath, RegistryView.Default, true);

                RL.Logger?.LogInformationSource($"The Registry key {key?.Name} has been created");

                return key;
            }

            return RCFWinReg.RegistryManager.GetKeyFromFullPath(keyPath, RegistryView.Default, writable);
        }

        #endregion
    }
}