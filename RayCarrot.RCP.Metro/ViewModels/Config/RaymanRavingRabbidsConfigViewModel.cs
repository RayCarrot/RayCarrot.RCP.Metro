using System;
using System.Threading.Tasks;
using Microsoft.Win32;
using Nito.AsyncEx;
using RayCarrot.CarrotFramework;
using RayCarrot.Windows.Registry;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// View model for the Rayman Raving Rabbids configuration
    /// </summary>
    public class RaymanRavingRabbidsConfigViewModel : GameConfigViewModel
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public RaymanRavingRabbidsConfigViewModel()
        {
            // Create commands
            SaveCommand = new AsyncRelayCommand(SaveAsync);

            // Create properties
            AsyncLock = new AsyncLock();
            AvailableResolutionValues = new string[]
            {
                "640 x 400", // 16/10
                "640 x 480", // 4/3
                "800 x 600", // 4/3
                "1280 x 600", // 32/15
                "1024 x 768", // 4/3
                "1280 x 720", // 16/9
                "1280 x 768", // 5/3
                "1152 x 864", // 4/3
                "1280 x 800", // 16/10
                "1360 x 768", // 85/48
                "1366 x 768", // 16/9
                "1280 x 960", // 4/3
                "1440 x 900", // 16/10
                "1280 x 1024", // 5/4
                "1600 x 900", // 16/9
                "1400 x 1050", // 4/3
                "1680 x 1050", // 16/10
                "1920 x 1080" // 16/9
            };
            AvailableScreenModeValues = new string[]
            {
                "4:3",
                "4:3 borders",
                "16:9"
            };
        }

        #endregion

        #region Private Fields

        private int _resolutionIndex;

        private bool _fullscreenMode;

        private bool _useController;

        private int _screenModeIndex;

        #endregion

        #region Private Constants

        private const string ResolutionKey = "Mode";

        private const string WindowedModeKey = "WindowedMode";

        private const string DefaultControllerKey = "DefaultController";

        private const string ScreenModeKey = "ScreenMode";

        #endregion

        #region Private Properties

        /// <summary>
        /// The async lock to use for saving the configuration
        /// </summary>
        protected AsyncLock AsyncLock { get; }

        #endregion

        #region Public Properties

        /// <summary>
        /// The available resolution values
        /// </summary>
        public string[] AvailableResolutionValues { get; }

        /// <summary>
        /// The selected resolution index
        /// </summary>
        public int ResolutionIndex
        {
            get => _resolutionIndex;
            set
            {
                _resolutionIndex = value;
                UnsavedChanges = true;
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

        /// <summary>
        /// Indicates if a controller should be used by default
        /// </summary>
        public bool UseController
        {
            get => _useController;
            set
            {
                _useController = value;
                UnsavedChanges = true;
            }
        }

        /// <summary>
        /// The available screen mode values
        /// </summary>
        public string[] AvailableScreenModeValues { get; }

        /// <summary>
        /// The selected screen mode index
        /// </summary>
        public int ScreenModeIndex
        {
            get => _screenModeIndex;
            set
            {
                _screenModeIndex = value;
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
            RCF.Logger.LogInformationSource("Rayman Raving Rabbids config is being set up");

            using (var key = RCFWinReg.RegistryManager.GetKeyFromFullPath(RCFWinReg.RegistryManager.CombinePaths(CommonPaths.RaymanRavingRabbidsRegistryKey, "Basic video"), RegistryView.Default))
            {
                RCF.Logger.LogInformationSource($"The key {key.Name} has been opened");

                ResolutionIndex = GetInt(ResolutionKey, 0);
                FullscreenMode = GetInt(WindowedModeKey, 0) != 1;
                UseController = GetInt(DefaultControllerKey, 0) == 1;
                ScreenModeIndex = GetInt(ScreenModeKey, 0) - 1;

                // Helper methods for getting values
                int GetInt(string valueName, int defaultValue) => Int32.TryParse(key.GetValue(valueName, defaultValue).ToString(), out int result) ? result : defaultValue;
            }

            UnsavedChanges = false;

            RCF.Logger.LogInformationSource($"All values have been loaded");

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
                RCF.Logger.LogInformationSource($"Rayman Raving Rabbids configuration is saving...");

                try
                {
                    using (var key = RCFWinReg.RegistryManager.GetKeyFromFullPath(RCFWinReg.RegistryManager.CombinePaths(CommonPaths.RaymanRavingRabbidsRegistryKey, "Basic video"), RegistryView.Default, true))
                    {
                        RCF.Logger.LogInformationSource($"The key {key.Name} has been opened");

                        key.SetValue(ResolutionKey, ResolutionIndex);
                        key.SetValue(WindowedModeKey, FullscreenMode ? 0 : 1);
                        key.SetValue(DefaultControllerKey, UseController ? 1 : 0);
                        key.SetValue(ScreenModeKey, ScreenModeIndex + 1);
                    }

                    RCF.Logger.LogInformationSource($"Rayman Raving Rabbids configuration has been saved");
                }
                catch (Exception ex)
                {
                    ex.HandleError("Saving Rayman Raving Rabbids registry data");
                    await RCF.MessageUI.DisplayMessageAsync($"An error occurred when saving your Rayman Raving Rabbids configuration", "Error saving", MessageType.Error);
                    return;
                }

                UnsavedChanges = false;

                await RCF.MessageUI.DisplaySuccessfulActionMessageAsync("Your changes have been saved");

                OnSave();
            }
        }

        #endregion
    }
}