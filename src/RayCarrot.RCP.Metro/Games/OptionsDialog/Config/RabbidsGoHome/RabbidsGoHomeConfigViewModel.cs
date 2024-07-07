using RayCarrot.RCP.Metro.Games.Data;

namespace RayCarrot.RCP.Metro.Games.OptionsDialog;

/// <summary>
/// View model for the Rabbids Go Home config
/// </summary>
public class RabbidsGoHomeConfigViewModel : ConfigPageViewModel
{
    #region Constructor

    public RabbidsGoHomeConfigViewModel(GameInstallation gameInstallation)
    {
        GameInstallation = gameInstallation;
    }

    #endregion

    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Private Fields

    private bool _enableCustomSettings;

    #endregion

    #region Public Properties

    public GameInstallation GameInstallation { get; }

    public bool EnableCustomSettings
    {
        get => _enableCustomSettings;
        set
        {
            _enableCustomSettings = value;
            if (value)
                LoadData(new RabbidsGoHomeLaunchData());
        }
    }

    public RabbidsGoHomeLanguage Language { get; set; }
    public bool Fullscreen { get; set; }
    public bool VSync { get; set; }
    public int VersionIndex { get; set; }
    public string BigFile { get; set; } = String.Empty;
    public string CustomCommands { get; set; } = String.Empty;

    #endregion

    #region Private Methods

    private static string GetLanguageString(RabbidsGoHomeLanguage language)
    {
        return language switch
        {
            RabbidsGoHomeLanguage.English => "en",
            RabbidsGoHomeLanguage.French => "fr",
            RabbidsGoHomeLanguage.German => "de",
            RabbidsGoHomeLanguage.Italian => "it",
            RabbidsGoHomeLanguage.Spanish => "es",
            RabbidsGoHomeLanguage.Dutch => "nl",
            _ => throw new ArgumentOutOfRangeException(nameof(language), language, null)
        };
    }

    private static RabbidsGoHomeLanguage GetLanguageValue(string languageString)
    {
        return languageString switch
        {
            "en" => RabbidsGoHomeLanguage.English,
            "fr" => RabbidsGoHomeLanguage.French,
            "de" => RabbidsGoHomeLanguage.German,
            "it" => RabbidsGoHomeLanguage.Italian,
            "es" => RabbidsGoHomeLanguage.Spanish,
            "nl" => RabbidsGoHomeLanguage.Dutch,
            _ => throw new ArgumentOutOfRangeException(nameof(languageString), languageString, null)
        };
    }

    private void LoadData(RabbidsGoHomeLaunchData data)
    {
        GraphicsMode.GetAvailableResolutions();
        GraphicsMode.SelectedGraphicsMode = new GraphicsMode(data.ResolutionX, data.ResolutionY);
        Language = GetLanguageValue(data.Language);
        Fullscreen = data.IsFullscreen;
        VSync = data.IsVSyncEnabled;
        VersionIndex = data.VersionIndex;
        BigFile = data.BigFile;
        CustomCommands = data.OptionalCommands.JoinItems(Environment.NewLine);
    }

    #endregion

    #region Protected Methods

    protected override Task LoadAsync()
    {
        Logger.Info("{0} config is being set up", GameInstallation.FullId);

        // Get the current launch data
        RabbidsGoHomeLaunchData? launchData = GameInstallation.GetObject<RabbidsGoHomeLaunchData>(GameDataKey.RGH_LaunchData);

        if (launchData != null)
        {
            _enableCustomSettings = true;
            OnPropertyChanged(nameof(EnableCustomSettings));

            LoadData(launchData);
        }
        else
        {
            EnableCustomSettings = false;
        }

        UnsavedChanges = false;

        Logger.Info("All config properties have been loaded");

        return Task.CompletedTask;
    }

    protected override Task<bool> SaveAsync()
    {
        Logger.Info("Rabbids Go Home configuration is saving...");

        // Set the launch data
        RabbidsGoHomeLaunchData? launchData = null;

        if (EnableCustomSettings)
            launchData = new RabbidsGoHomeLaunchData(
                bigFile: BigFile, 
                language: GetLanguageString(Language), 
                resolutionX: GraphicsMode.Width,
                resolutionY: GraphicsMode.Height, 
                isVSyncEnabled: VSync, 
                isFullscreen: Fullscreen, 
                versionIndex: VersionIndex,
                optionalCommands: CustomCommands.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries));

        GameInstallation.SetObject(GameDataKey.RGH_LaunchData, launchData);

        // Refresh
        Services.Messenger.Send(new ModifiedGamesMessage(GameInstallation));

        Logger.Info("Rabbids Go Home configuration has been saved");

        return Task.FromResult(true);
    }

    protected override void ConfigPropertyChanged(string propertyName)
    {
        if (propertyName is
            nameof(EnableCustomSettings) or
            nameof(Language) or
            nameof(Fullscreen) or
            nameof(VSync) or
            nameof(VersionIndex) or
            nameof(BigFile) or
            nameof(CustomCommands))
        {
            UnsavedChanges = true;
        }
    }

    #endregion

    #region Enums

    public enum RabbidsGoHomeLanguage
    {
        English,
        French,
        German,
        Italian,
        Spanish,
        Dutch
    }

    #endregion
}