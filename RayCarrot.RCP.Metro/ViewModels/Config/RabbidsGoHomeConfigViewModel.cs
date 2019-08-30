using System;
using System.Threading.Tasks;
using System.Windows;
using Nito.AsyncEx;
using RayCarrot.CarrotFramework.Abstractions;
using RayCarrot.Extensions;
using RayCarrot.UI;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// View model for the Rabbids Go Home config
    /// </summary>
    public class RabbidsGoHomeConfigViewModel : GameConfigViewModel
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public RabbidsGoHomeConfigViewModel()
        {
            // Create commands
            SaveCommand = new AsyncRelayCommand(SaveAsync);

            // Create properties
            AsyncLock = new AsyncLock();
        }

        #endregion

        #region Private Fields

        private bool _enableCustomSettings;

        private int _resX;

        private int _resY;

        private bool _lockToScreenRes;

        private RabbidsGoHomeLanguages _language;

        private bool _fullscreen;

        private bool _vSync;

        private int _versionIndex;

        private string _bigFile;

        private string _customCommands;

        #endregion

        #region Private Properties

        /// <summary>
        /// The async lock to use for saving the configuration
        /// </summary>
        protected AsyncLock AsyncLock { get; }

        #endregion

        #region Public Properties

        /// <summary>
        /// Indicates if custom settings are enabled
        /// </summary>
        public bool EnableCustomSettings
        {
            get => _enableCustomSettings;
            set
            {
                _enableCustomSettings = value;
                UnsavedChanges = true;

                if (EnableCustomSettings)
                    ImportConfig(new RabbidsGoHomeLaunchData());
            }
        }

        /// <summary>
        /// The current horizontal resolution
        /// </summary>
        public int ResX
        {
            get => _resX;
            set
            {
                _resX = value;
                UnsavedChanges = true;
            }
        }

        /// <summary>
        /// The current vertical resolution
        /// </summary>
        public int ResY
        {
            get => _resY;
            set
            {
                _resY = value;
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

                ResY = (int)SystemParameters.PrimaryScreenHeight;
                ResX = (int)SystemParameters.PrimaryScreenWidth;
            }
        }

        /// <summary>
        /// The selected language
        /// </summary>
        public RabbidsGoHomeLanguages Language
        {
            get => _language;
            set
            {
                _language = value;
                UnsavedChanges = true;
            }
        }

        /// <summary>
        /// Indicates if fullscreen mode is enabled
        /// </summary>
        public bool Fullscreen
        {
            get => _fullscreen;
            set
            {
                _fullscreen = value;
                UnsavedChanges = true;
            }
        }

        /// <summary>
        /// Indicates if V-Sync is enabled
        /// </summary>
        public bool VSync
        {
            get => _vSync;
            set
            {
                _vSync = value;
                UnsavedChanges = true;
            }
        }

        /// <summary>
        /// The selected version index
        /// </summary>
        public int VersionIndex
        {
            get => _versionIndex;
            set
            {
                _versionIndex = value;
                UnsavedChanges = true;
            }
        }

        /// <summary>
        /// The big file
        /// </summary>
        public string BigFile
        {
            get => _bigFile;
            set
            {
                _bigFile = value;
                UnsavedChanges = true;
            }
        }

        /// <summary>
        /// Custom commands
        /// </summary>
        public string CustomCommands
        {
            get => _customCommands;
            set
            {
                _customCommands = value;
                UnsavedChanges = true;
            }
        }

        #endregion

        #region Commands

        public AsyncRelayCommand SaveCommand { get; }

        #endregion

        #region Private Methods

        /// <summary>
        /// Import the specified data
        /// </summary>
        /// <param name="data">The data to import</param>
        private void ImportConfig(RabbidsGoHomeLaunchData data)
        {
            ResX = data.ResolutionX;
            ResY = data.ResolutionY;
            Language = GetLanguage(data.Language);
            Fullscreen = data.IsFullscreen;
            VSync = data.IsVSyncEnabled;
            VersionIndex = data.VersionIndex;
            BigFile = data.BigFile;
            CustomCommands = data.OptionalCommands.JoinItems(Environment.NewLine);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Loads and sets up the current configuration properties
        /// </summary>
        /// <returns>The task</returns>
        public override Task SetupAsync()
        {
            RCFCore.Logger?.LogInformationSource("Rabbids Go Home config is being set up");

            // Get the current launch data
            var launchData = Data.RabbidsGoHomeLaunchData;

            if (launchData != null)
            {
                _enableCustomSettings = true;
                OnPropertyChanged(nameof(EnableCustomSettings));

                ImportConfig(launchData);
            }
            else
            {
                EnableCustomSettings = false;
            }

            UnsavedChanges = false;

            RCFCore.Logger?.LogInformationSource($"All values have been loaded");

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
                RCFCore.Logger?.LogInformationSource($"Rabbids Go Home configuration is saving...");

                // Set the launch data
                Data.RabbidsGoHomeLaunchData = EnableCustomSettings ? 
                    new RabbidsGoHomeLaunchData(BigFile, GetLanguageName(Language), ResX, ResY, VSync, Fullscreen, VersionIndex, CustomCommands.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)) :
                    null;

                // Refresh
                await App.OnRefreshRequiredAsync(new RefreshRequiredEventArgs(Games.RabbidsGoHome, false, false, false, true));

                RCFCore.Logger?.LogInformationSource($"Rabbids Go Home configuration has been saved");

                UnsavedChanges = false;

                await RCFUI.MessageUI.DisplaySuccessfulActionMessageAsync(Resources.Config_SaveSuccess);

                OnSave();
            }
        }

        #endregion

        #region Private Static Methods

        /// <summary>
        /// Gets the language name for the specified language
        /// </summary>
        /// <param name="language">The language to get the name from</param>
        /// <returns>The language name</returns>
        private static string GetLanguageName(RabbidsGoHomeLanguages language)
        {
            switch (language)
            {
                case RabbidsGoHomeLanguages.English:
                    return "en";

                case RabbidsGoHomeLanguages.French:
                    return "fr";

                case RabbidsGoHomeLanguages.German:
                    return "de";

                case RabbidsGoHomeLanguages.Italian:
                    return "it";

                case RabbidsGoHomeLanguages.Spanish:
                    return "es";

                case RabbidsGoHomeLanguages.Dutch:
                    return "nl";

                default:
                    throw new ArgumentOutOfRangeException(nameof(language), language, null);
            }
        }

        /// <summary>
        /// Gets the language for the specified name
        /// </summary>
        /// <param name="languageName">The language name to get the language from</param>
        /// <returns>The language</returns>
        private static RabbidsGoHomeLanguages GetLanguage(string languageName)
        {
            switch (languageName)
            {
                case "en":
                    return RabbidsGoHomeLanguages.English;

                case "fr":
                    return RabbidsGoHomeLanguages.French;

                case "de":
                    return RabbidsGoHomeLanguages.German;

                case "it":
                    return RabbidsGoHomeLanguages.Italian;

                case "es":
                    return RabbidsGoHomeLanguages.Spanish;

                case "nl":
                    return RabbidsGoHomeLanguages.Dutch;

                default:
                    throw new ArgumentOutOfRangeException(nameof(languageName), languageName, null);
            }
        }

        #endregion

        #region Enums

        /// <summary>
        /// The available languages for Rabbids Go Home
        /// </summary>
        public enum RabbidsGoHomeLanguages
        {
            /// <summary>
            /// English
            /// </summary>
            English,

            /// <summary>
            /// French
            /// </summary>
            French,

            /// <summary>
            /// German
            /// </summary>
            German,

            /// <summary>
            /// Italian
            /// </summary>
            Italian,

            /// <summary>
            /// Spanish
            /// </summary>
            Spanish,

            /// <summary>
            /// Dutch
            /// </summary>
            Dutch
        }

        #endregion
    }
}