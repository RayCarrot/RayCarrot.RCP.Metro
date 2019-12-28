using Newtonsoft.Json;
using RayCarrot.CarrotFramework.Abstractions;
using RayCarrot.IO;
using RayCarrot.UI;
using RayCarrot.UserData;
using System;
using System.Reflection;
using RayCarrot.RCP.Core;
using RayCarrot.WPF;

namespace RayCarrot.RCP.UI
{
    /// <summary>
    /// Base class for Rayman Control Panel user data
    /// </summary>
    public class RCPAppUserData : BaseViewModel, IUserData
    {
        #region Interface Implementation

        /// <summary>
        /// The path of the saved <see cref="RCPAppUserData"/> file
        /// </summary>
        [JsonIgnore]
        public FileSystemPath FilePath => RCFRCPA.Path.GetUserDataFile(GetType());

        /// <summary>
        /// The name of the <see cref="IUserData"/>
        /// </summary>
        [JsonIgnore]
        public string Name => "AppUserData";

        /// <summary>
        /// Resets all values to their defaults
        /// </summary>
        public virtual void Reset()
        {
            UserLevel = UserLevel.Advanced;
            LastVersion = new Version(0, 0, 0, 0);
            WindowState = null;
            DarkMode = true;
            ShowActionComplete = true;
            AutoUpdate = true;
            ShowProgressOnTaskBar = true;
            DisplayExceptionLevel = ExceptionLevel.Critical;
            EnableAnimations = true;
            CurrentCulture = RCFRCPUI.Localization.DefaultCulture.Name;
            ShowIncompleteTranslations = false;
            ApplicationPath = Assembly.GetExecutingAssembly().Location;
            ForceUpdate = false;
            GetBetaUpdates = false;
            DisableDowngradeWarning = false;
            IsUpdateAvailable = false;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// The current user level
        /// </summary>
        public UserLevel UserLevel
        {
            get => RCFCore.Data.CurrentUserLevel;
            set => RCFCore.Data.CurrentUserLevel = value;
        }

        /// <summary>
        /// The last used version of the program
        /// </summary>
        public Version LastVersion { get; set; }

        /// <summary>
        /// The previous Window session state
        /// </summary>
        public WindowSessionState WindowState { get; set; }

        /// <summary>
        /// Indicates if the dark mode is on
        /// </summary>
        public bool DarkMode { get; set; }

        /// <summary>
        /// Indicates if action complete messages should be shown
        /// </summary>
        public bool ShowActionComplete { get; set; }

        /// <summary>
        /// Indicates if updates should automatically be check for
        /// </summary>
        public bool AutoUpdate { get; set; }

        /// <summary>
        /// Indicates if progress should be shown on the task bar
        /// </summary>
        public bool ShowProgressOnTaskBar { get; set; }

        /// <summary>
        /// The minimum exception level to display
        /// </summary>
        public ExceptionLevel DisplayExceptionLevel { get; set; }

        /// <summary>
        /// Indicates if animations are enabled
        /// </summary>
        public bool EnableAnimations { get; set; }

        /// <summary>
        /// The current culture in the application
        /// </summary>
        public string CurrentCulture
        {
            get => RCFCore.Data.CurrentCulture?.Name ?? RCFRCPUI.Localization.DefaultCulture.Name;
            set => RCFRCPUI.Localization.SetCulture(value);
        }

        /// <summary>
        /// Indicates if languages with incomplete translations should be shown
        /// </summary>
        public bool ShowIncompleteTranslations { get; set; }

        /// <summary>
        /// The last recorded path of the application
        /// </summary>
        public FileSystemPath ApplicationPath { get; set; }

        /// <summary>
        /// Indicates if a manual update should be forced even if the installed version is the latest one
        /// </summary>
        public bool ForceUpdate { get; set; }

        /// <summary>
        /// Indicates if beta updates should be searched for through the updater
        /// </summary>
        public bool GetBetaUpdates { get; set; }

        /// <summary>
        /// Indicates if the downgrade warnings should be disabled
        /// </summary>
        public bool DisableDowngradeWarning { get; set; }

        /// <summary>
        /// Indicates if a new update is available
        /// </summary>
        public bool IsUpdateAvailable { get; set; }

        #endregion
    }
}
