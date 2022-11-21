#nullable disable
using NLog;
using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// View model for the Rabbids Go Home config
/// </summary>
public class Config_RabbidsGoHome_ViewModel : GameOptionsDialog_ConfigPageViewModel
{
    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Private Fields

    private bool _enableCustomSettings;

    private RabbidsGoHomeLanguage _language;

    private bool _fullscreen;

    private bool _vSync;

    private int _versionIndex;

    private string _bigFile;

    private string _customCommands;

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
                ImportConfig(new UserData_RabbidsGoHomeLaunchData());
        }
    }

    /// <summary>
    /// The selected language
    /// </summary>
    public RabbidsGoHomeLanguage Language
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

    #region Private Methods

    /// <summary>
    /// Import the specified data
    /// </summary>
    /// <param name="data">The data to import</param>
    private void ImportConfig(UserData_RabbidsGoHomeLaunchData data)
    {
        GraphicsMode.GetAvailableResolutions();
        GraphicsMode.SelectedGraphicsMode = new GraphicsMode(data.ResolutionX, data.ResolutionY);
        Language = GetLanguage(data.Language);
        Fullscreen = data.IsFullscreen;
        VSync = data.IsVSyncEnabled;
        VersionIndex = data.VersionIndex;
        BigFile = data.BigFile;
        CustomCommands = data.OptionalCommands.JoinItems(Environment.NewLine);
    }

    #endregion

    #region Protected Methods

    protected override object GetPageUI() => new Config_RabbidsGoHome_Control()
    {
        DataContext = this
    };

    /// <summary>
    /// Loads and sets up the current configuration properties
    /// </summary>
    /// <returns>The task</returns>
    protected override Task LoadAsync()
    {
        Logger.Info("Rabbids Go Home config is being set up");

        // Get the current launch data
        var launchData = Data.Game_RabbidsGoHomeLaunchData;

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

        Logger.Info("All values have been loaded");

        return Task.CompletedTask;
    }

    /// <summary>
    /// Saves the changes
    /// </summary>
    /// <returns>The task</returns>
    protected override async Task<bool> SaveAsync()
    {
        Logger.Info("Rabbids Go Home configuration is saving...");

        // Set the launch data
        Data.Game_RabbidsGoHomeLaunchData = EnableCustomSettings ?
            new UserData_RabbidsGoHomeLaunchData(BigFile, GetLanguageName(Language), GraphicsMode.Width, GraphicsMode.Height, VSync, Fullscreen, VersionIndex, CustomCommands.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)) :
            null;

        // Refresh
        Services.Messenger.Send(new ModifiedGamesMessage(Games.RabbidsGoHome.GetInstallation()));

        Logger.Info("Rabbids Go Home configuration has been saved");

        return true;
    }

    #endregion

    #region Private Static Methods

    /// <summary>
    /// Gets the language name for the specified language
    /// </summary>
    /// <param name="language">The language to get the name from</param>
    /// <returns>The language name</returns>
    private static string GetLanguageName(RabbidsGoHomeLanguage language)
    {
        switch (language)
        {
            case RabbidsGoHomeLanguage.English:
                return "en";

            case RabbidsGoHomeLanguage.French:
                return "fr";

            case RabbidsGoHomeLanguage.German:
                return "de";

            case RabbidsGoHomeLanguage.Italian:
                return "it";

            case RabbidsGoHomeLanguage.Spanish:
                return "es";

            case RabbidsGoHomeLanguage.Dutch:
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
    private static RabbidsGoHomeLanguage GetLanguage(string languageName)
    {
        switch (languageName)
        {
            case "en":
                return RabbidsGoHomeLanguage.English;

            case "fr":
                return RabbidsGoHomeLanguage.French;

            case "de":
                return RabbidsGoHomeLanguage.German;

            case "it":
                return RabbidsGoHomeLanguage.Italian;

            case "es":
                return RabbidsGoHomeLanguage.Spanish;

            case "nl":
                return RabbidsGoHomeLanguage.Dutch;

            default:
                throw new ArgumentOutOfRangeException(nameof(languageName), languageName, null);
        }
    }

    #endregion

    #region Enums

    /// <summary>
    /// The available languages for Rabbids Go Home
    /// </summary>
    public enum RabbidsGoHomeLanguage
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