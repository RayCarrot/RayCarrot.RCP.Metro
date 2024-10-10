using System.Windows.Input;
using RayCarrot.RCP.Metro.Games.Tools;
using RayCarrot.RCP.Metro.Games.Tools.PerLevelSoundtrack;

namespace RayCarrot.RCP.Metro.Games.Panels;

public class PerLevelSoundtrackGamePanelViewModel : GamePanelViewModel, 
    IRecipient<ToolInstalledMessage>, IRecipient<ToolUninstalledMessage>
{
    #region Constructor

    public PerLevelSoundtrackGamePanelViewModel(GameInstallation gameInstallation) : base(gameInstallation)
    {
        InstallableTool = new PerLevelSoundtrackInstallableTool();

        InstallCommand = new AsyncRelayCommand(InstallAsync);
        UninstallCommand = new AsyncRelayCommand(UninstallAsync);
        OpenGitHubCommand = new RelayCommand(OpenGitHub);

        Services.Messenger.RegisterAll(this);
    }

    #endregion

    #region Commands

    public ICommand InstallCommand { get; }
    public ICommand UninstallCommand { get; }
    public ICommand OpenGitHubCommand { get; }

    #endregion

    #region Public Properties

    public override GenericIconKind Icon => GenericIconKind.GamePanel_PerLevelSoundtrack;
    public override LocalizedString Header => "Per-level Soundtrack"; // TODO-LOC

    public string GitHubUrl => "https://github.com/PluMGMK/rayman-tpls-tsr";

    public InstallableTool InstallableTool { get; }

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

    #region Public Methods

    protected override Task LoadAsyncImpl()
    {
        IsInstalled = Services.InstallableTools.CheckIsInstalled(InstallableTool);
        OnPropertyChanged(nameof(IsEnabled));

        return Task.CompletedTask;
    }

    public override Task UnloadAsync()
    {
        Services.Messenger.UnregisterAll(this);
     
        return Task.CompletedTask;
    }

    public async Task InstallAsync()
    {
        await Services.InstallableTools.InstallAsync(InstallableTool);
    }

    public async Task UninstallAsync()
    {
        // TODO-LOC
        if (!await Services.MessageUI.DisplayMessageAsync("Are you sure you want to uninstall the Per-level Soundtrack tool? This will uninstall for all games where it is used.", "Confirm uninstall", MessageType.Question, true))
            return;

        await Services.InstallableTools.UninstallAsync(InstallableTool);
    }

    public void OpenGitHub()
    {
        Services.App.OpenUrl(GitHubUrl);
    }

    #endregion

    #region Message Receivers

    void IRecipient<ToolInstalledMessage>.Receive(ToolInstalledMessage message)
    {
        if (message.ToolId == InstallableTool.ToolId)
        {
            IsInstalled = true;
            IsEnabled = true;
        }
    }

    void IRecipient<ToolUninstalledMessage>.Receive(ToolUninstalledMessage message)
    {
        if (message.ToolId == InstallableTool.ToolId)
        {
            IsInstalled = false;
        }
    }

    #endregion
}