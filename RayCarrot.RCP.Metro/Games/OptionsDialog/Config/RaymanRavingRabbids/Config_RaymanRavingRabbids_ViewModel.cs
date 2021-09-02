using Microsoft.Win32;
using RayCarrot.Logging;
using RayCarrot.Windows.Registry;
using System;
using System.Threading.Tasks;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// View model for the Rayman Raving Rabbids configuration
    /// </summary>
    public class Config_RaymanRavingRabbids_ViewModel : GameOptionsDialog_ConfigPageViewModel
    {
        #region Private Fields

        private bool _fullscreenMode;

        private bool _useController;

        private int _screenModeIndex;

        #endregion

        #region Private Constants

        private const string WindowedModeKey = "WindowedMode";

        private const string DefaultControllerKey = "DefaultController";

        private const string ScreenModeKey = "ScreenMode";

        #endregion

        #region Public Properties

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

        #region Protected Methods

        protected override object GetPageUI() => new Config_RaymanRavingRabbids_UI()
        {
            DataContext = this
        };

        /// <summary>
        /// Loads and sets up the current configuration properties
        /// </summary>
        /// <returns>The task</returns>
        protected override Task LoadAsync()
        {
            RL.Logger?.LogInformationSource("Rayman Raving Rabbids config is being set up");

            using (var key = RegistryHelpers.GetKeyFromFullPath(RegistryHelpers.CombinePaths(AppFilePaths.RaymanRavingRabbidsRegistryKey, "Basic video"), RegistryView.Default))
            {
                RL.Logger?.LogInformationSource(key != null
                    ? $"The key {key.Name} has been opened"
                    : $"The key for {Games.RaymanRavingRabbids} does not exist. Default values will be used.");

                FullscreenMode = GetInt(WindowedModeKey, 0) != 1;
                UseController = GetInt(DefaultControllerKey, 0) == 1;
                ScreenModeIndex = GetInt(ScreenModeKey, 1) - 1;

                // Helper methods for getting values
                int GetInt(string valueName, int defaultValue) => Int32.TryParse(key?.GetValue(valueName, defaultValue).ToString(), out int result) ? result : defaultValue;
            }

            UnsavedChanges = false;

            RL.Logger?.LogInformationSource($"All values have been loaded");

            return Task.CompletedTask;
        }

        /// <summary>
        /// Saves the changes
        /// </summary>
        /// <returns>The task</returns>
        protected override async Task<bool> SaveAsync()
        {
            RL.Logger?.LogInformationSource($"Rayman Raving Rabbids configuration is saving...");

            try
            {
                // Get the key path
                var keyPath = RegistryHelpers.CombinePaths(AppFilePaths.RaymanRavingRabbidsRegistryKey, "Basic video");

                RegistryKey key;

                // Create the key if it doesn't exist
                if (!RegistryHelpers.KeyExists(keyPath))
                {
                    key = RegistryHelpers.CreateRegistryKey(keyPath, RegistryView.Default, true);

                    RL.Logger?.LogInformationSource($"The Registry key {key?.Name} has been created");
                }
                else
                {
                    key = RegistryHelpers.GetKeyFromFullPath(keyPath, RegistryView.Default, true);
                }

                using (key)
                {
                    if (key == null)
                        throw new Exception("The Registry key could not be created");

                    RL.Logger?.LogInformationSource($"The key {key.Name} has been opened");

                    key.SetValue(WindowedModeKey, FullscreenMode ? 0 : 1);
                    key.SetValue(DefaultControllerKey, UseController ? 1 : 0);
                    key.SetValue(ScreenModeKey, ScreenModeIndex + 1);
                }

                RL.Logger?.LogInformationSource($"Rayman Raving Rabbids configuration has been saved");

                return true;
            }
            catch (Exception ex)
            {
                ex.HandleError("Saving Rayman Raving Rabbids registry data");
                await WPF.Services.MessageUI.DisplayExceptionMessageAsync(ex, Resources.Config_SaveRRRError, Resources.Config_SaveErrorHeader);
                return false;
            }
        }

        #endregion
    }
}