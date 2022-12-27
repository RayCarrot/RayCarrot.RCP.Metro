using System.ComponentModel;
using System.Windows.Input;
using RayCarrot.RCP.Metro.Games.Emulators;

namespace RayCarrot.RCP.Metro.Games.OptionsDialog;

public class EmulatorGameConfigPageViewModel : GameOptionsDialogPageViewModel, 
    IRecipient<AddedEmulatorsMessage>, IRecipient<RemovedEmulatorsMessage>, IRecipient<ModifiedGamesMessage>
{
    #region Constructor

    public EmulatorGameConfigPageViewModel(GameInstallation gameInstallation)
    {
        GameInstallation = gameInstallation;

        ConfigureEmulatorsCommand = new AsyncRelayCommand(ConfigureEmulatorsAsync);

        // TODO-14: Only do this once page is first loaded!
        // Register for messages
        Services.Messenger.RegisterAll(this);
    }

    #endregion

    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Private Fields

    private EmulatorViewModel? _selectedEmulator;

    #endregion

    #region Commands

    public ICommand ConfigureEmulatorsCommand { get; }

    #endregion

    #region Public Properties

    public override LocalizedString PageName => "Emulator"; // TODO-UPDATE: Localize
    public override GenericIconKind PageIcon => GenericIconKind.GameOptions_Emulator;
    public override bool CanSave => EmulatorGameConfig?.CanSave ?? false;
    public override bool CanUseRecommended => EmulatorGameConfig?.CanUseRecommended ?? false;

    public GameInstallation GameInstallation { get; }

    public ObservableCollection<EmulatorViewModel>? Emulators { get; set; }

    public EmulatorViewModel? SelectedEmulator
    {
        get => _selectedEmulator;
        set
        {
            _selectedEmulator = value;

            Invoke();
            async void Invoke()
            {
                await GameInstallation.GameDescriptor.SetGameClientAsync(GameInstallation, value?.EmulatorInstallation);
                await SetSelectedEmulatorAsync(value);
            }
        }
    }

    public EmulatorGameConfigViewModel? EmulatorGameConfig { get; set; }

    #endregion

    #region Private Methods

    private async Task SetSelectedEmulatorAsync(EmulatorViewModel? emulator)
    {
        _selectedEmulator = emulator;
        OnPropertyChanged(nameof(SelectedEmulator));

        if (EmulatorGameConfig != null)
            EmulatorGameConfig.PropertyChanged -= EmulatorGameConfig_PropertyChanged;

        // Set the emulator config
        EmulatorGameConfig = emulator?.EmulatorInstallation.EmulatorDescriptor.GetGameConfigViewModel(GameInstallation, emulator.EmulatorInstallation);

        // Update page properties to match emulator config
        OnPropertyChanged(nameof(CanSave));
        OnPropertyChanged(nameof(CanUseRecommended));

        UnsavedChanges = EmulatorGameConfig?.UnsavedChanges ?? false;
        if (EmulatorGameConfig != null)
            EmulatorGameConfig.PropertyChanged += EmulatorGameConfig_PropertyChanged;

        try
        {
            EmulatorGameConfig?.Load();
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Loading emulator config");

            // TODO-14: Show an error message to the user

            EmulatorGameConfig = null;
        }
    }

    private void EmulatorGameConfig_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(EmulatorGameConfig.UnsavedChanges))
            UnsavedChanges = EmulatorGameConfig?.UnsavedChanges ?? false;
    }

    #endregion

    #region Protected Methods

    protected override async Task LoadAsync()
    {
        await SetSelectedEmulatorAsync(null);

        var emulators = Services.Emulators.GetInstalledEmulators().
            Where(x => x.EmulatorDescriptor.SupportedPlatforms.Contains(GameInstallation.GameDescriptor.Platform)).
            Select(x => new EmulatorViewModel(x));
        Emulators = new ObservableCollection<EmulatorViewModel>(emulators);

        string? emuId = GameInstallation.GetValue<string>(GameDataKey.Client_SelectedClient);

        if (emuId != null)
            await SetSelectedEmulatorAsync(Emulators.FirstOrDefault(x => x.EmulatorInstallation.InstallationId == emuId));
    }

    protected override async Task<bool> SaveAsync()
    {
        if (EmulatorGameConfig != null)
            return await EmulatorGameConfig.SaveAsync();
        else
            return true;
    }

    protected override void UseRecommended() => EmulatorGameConfig?.UseRecommended();

    #endregion

    #region Public Methods

    public Task ConfigureEmulatorsAsync() => Services.UI.ShowEmulatorsSetupAsync();

    public async void Receive(AddedEmulatorsMessage message)
    {
        // Refresh if any added emulators work on this game
        if (message.EmulatorInstallations.Any(x => 
                x.EmulatorDescriptor.SupportedPlatforms.Contains(GameInstallation.GameDescriptor.Platform)))
            await LoadPageAsync();
    }
    public async void Receive(RemovedEmulatorsMessage message)
    {
        // Refresh if any removed emulators work on this game
        if (message.EmulatorInstallations.Any(x =>
                x.EmulatorDescriptor.SupportedPlatforms.Contains(GameInstallation.GameDescriptor.Platform)))
            await LoadPageAsync();
    }
    public async void Receive(ModifiedGamesMessage message)
    {
        if (message.GameInstallations.Contains(GameInstallation))
        {
            string? emuId = GameInstallation.GetValue<string>(GameDataKey.Client_SelectedClient);

            if (emuId != SelectedEmulator?.EmulatorInstallation.InstallationId)
                await SetSelectedEmulatorAsync(Emulators?.FirstOrDefault(x => x.EmulatorInstallation.InstallationId == emuId));
        }
    }

    public override void Dispose()
    {
        base.Dispose();

        // Unregister for messages
        Services.Messenger.UnregisterAll(this);
    }

    #endregion

    #region Classes

    public class EmulatorViewModel : BaseViewModel
    {
        public EmulatorViewModel(EmulatorInstallation emulatorInstallation)
        {
            EmulatorInstallation = emulatorInstallation;
        }

        public EmulatorInstallation EmulatorInstallation { get; }
        public LocalizedString DisplayName => EmulatorInstallation.EmulatorDescriptor.DisplayName;
        public EmulatorIconAsset Icon => EmulatorInstallation.EmulatorDescriptor.Icon;
    }

    #endregion
}