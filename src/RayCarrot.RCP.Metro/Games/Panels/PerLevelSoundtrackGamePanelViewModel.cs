using System.Windows.Input;
using RayCarrot.RCP.Metro.Games.Components;
using RayCarrot.RCP.Metro.Games.Tools.PerLevelSoundtrack;
using RayCarrot.RCP.Metro.ModLoader.Dialogs.ModLoader;
using RayCarrot.RCP.Metro.ModLoader.Sources.GameBanana;

namespace RayCarrot.RCP.Metro.Games.Panels;

public class PerLevelSoundtrackGamePanelViewModel : GamePanelViewModel, IRecipient<ModifiedGameModsMessage>
{
    #region Constructor

    public PerLevelSoundtrackGamePanelViewModel(GameInstallation gameInstallation) : base(gameInstallation)
    {
        InstallCommand = new AsyncRelayCommand(InstallAsync);
        OpenGitHubCommand = new RelayCommand(OpenGitHub);

        Services.Messenger.RegisterAll(this);
    }

    #endregion

    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Constant Fields

    private const long GameBananaModId = 0; // TODO-UPDATE: Define

    #endregion

    #region Commands

    public ICommand InstallCommand { get; }
    public ICommand OpenGitHubCommand { get; }

    #endregion

    #region Public Properties

    public override GenericIconKind Icon => GenericIconKind.GamePanel_PerLevelSoundtrack;
    public override LocalizedString Header => new ResourceLocString(nameof(Resources.GameTool_PerLevelSoundtrack));

    public string GitHubUrl => "https://github.com/PluMGMK/rayman-tpls-tsr";

    public bool IsInstalled { get; set; }
    public bool IsEnabled
    {
        get => GameInstallation.GetObject<PerLevelSoundtrackData>(GameDataKey.R1_PerLevelSoundtrackData) is { IsEnabled: true };
        set
        {
            GameInstallation.ModifyObject<PerLevelSoundtrackData>(GameDataKey.R1_PerLevelSoundtrackData, x => x.IsEnabled = value);
            Services.Messenger.Send(new ModifiedGamesMessage(GameInstallation));
        }
    }

    // Commented out for now both here and in the UI
    //public bool ExpandedMemory
    //{
    //    get => GameInstallation.GetObject<PerLevelSoundtrackData>(GameDataKey.R1_PerLevelSoundtrackData) is { ExpandedMemory: true };
    //    set => GameInstallation.ModifyObject<PerLevelSoundtrackData>(GameDataKey.R1_PerLevelSoundtrackData, x => x.ExpandedMemory = value);
    //}
    public bool DisableClearAndDeathMusic
    {
        get => GameInstallation.GetObject<PerLevelSoundtrackData>(GameDataKey.R1_PerLevelSoundtrackData) is { DisableClearAndDeathMusic: true };
        set
        {
            GameInstallation.ModifyObject<PerLevelSoundtrackData>(GameDataKey.R1_PerLevelSoundtrackData, x => x.DisableClearAndDeathMusic = value);
            Services.Messenger.Send(new ModifiedGamesMessage(GameInstallation));
        }
    }
    public bool CdAudioOnly
    {
        get => GameInstallation.GetObject<PerLevelSoundtrackData>(GameDataKey.R1_PerLevelSoundtrackData) is { CdAudioOnly: true };
        set
        {
            GameInstallation.ModifyObject<PerLevelSoundtrackData>(GameDataKey.R1_PerLevelSoundtrackData, x => x.CdAudioOnly = value);
            Services.Messenger.Send(new ModifiedGamesMessage(GameInstallation));
        }
    }
    public bool MusicOnly
    {
        get => GameInstallation.GetObject<PerLevelSoundtrackData>(GameDataKey.R1_PerLevelSoundtrackData) is { MusicOnly: true };
        set
        {
            GameInstallation.ModifyObject<PerLevelSoundtrackData>(GameDataKey.R1_PerLevelSoundtrackData, x => x.MusicOnly = value);
            Services.Messenger.Send(new ModifiedGamesMessage(GameInstallation));
        }
    }
    public bool FistKills
    {
        get => GameInstallation.GetObject<PerLevelSoundtrackData>(GameDataKey.R1_PerLevelSoundtrackData) is { FistKills: true };
        set
        {
            GameInstallation.ModifyObject<PerLevelSoundtrackData>(GameDataKey.R1_PerLevelSoundtrackData, x => x.FistKills = value);
            Services.Messenger.Send(new ModifiedGamesMessage(GameInstallation));
        }
    }

    #endregion

    #region Private Methods

    private bool CheckIsModInstalled()
    {
        try
        {
            // NOTE: We don't check if the mod itself is installed, but rather if the expected file exists, so that
            //       the user may add the mod manually or through a custom mod if they want to
            PerLevelSoundtrackDosBoxLaunchCommandsComponent c = GameInstallation.GetRequiredComponent<DosBoxLaunchCommandsComponent, PerLevelSoundtrackDosBoxLaunchCommandsComponent>();
            return c.GetCueFilePath().FileExists;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Checking if mod is installed");
            return false;
        }
    }

    #endregion

    #region Public Methods

    protected override Task LoadAsyncImpl()
    {
        IsInstalled = CheckIsModInstalled();
        return Task.CompletedTask;
    }

    public override Task UnloadAsync()
    {
        Services.Messenger.UnregisterAll(this);
        return Task.CompletedTask;
    }

    public async Task InstallAsync()
    {
        await Services.UI.ShowModLoaderAsync(GameInstallation, _ =>
        {
            Services.Messenger.Send(new OpenModDownloadPageMessage(GameInstallation, GameBananaModsSource.SourceId, new GameBananaInstallData(GameBananaModId, -1)));
            return Task.CompletedTask;
        });
    }

    public void OpenGitHub()
    {
        Services.App.OpenUrl(GitHubUrl);
    }

    #endregion

    #region Message Receivers

    void IRecipient<ModifiedGameModsMessage>.Receive(ModifiedGameModsMessage message)
    {
        if (message.GameInstallation == GameInstallation)
            IsInstalled = CheckIsModInstalled();
    }

    #endregion
}