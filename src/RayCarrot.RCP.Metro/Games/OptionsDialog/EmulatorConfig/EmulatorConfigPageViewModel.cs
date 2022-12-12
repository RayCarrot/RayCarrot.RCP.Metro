using System.Windows.Input;
using RayCarrot.RCP.Metro.Games.Emulators;

namespace RayCarrot.RCP.Metro.Games.OptionsDialog;

public class EmulatorConfigPageViewModel : GameOptionsDialogPageViewModel, 
    IRecipient<AddedEmulatorsMessage>, IRecipient<RemovedEmulatorsMessage>, IRecipient<ModifiedGamesMessage>
{
    public EmulatorConfigPageViewModel(GameInstallation gameInstallation, EmulatedGameDescriptor gameDescriptor)
    {
        GameInstallation = gameInstallation;
        GameDescriptor = gameDescriptor;

        ConfigureEmulatorsCommand = new AsyncRelayCommand(ConfigureEmulatorsAsync);

        // TODO-14: Only do this once page is first loaded!
        // Register for messages
        Services.Messenger.RegisterAll(this);
    }

    private EmulatorViewModel? _selectedEmulator;

    public ICommand ConfigureEmulatorsCommand { get; }

    public override LocalizedString PageName => "Emulator"; // TODO-UPDATE: Localize
    public override GenericIconKind PageIcon => GenericIconKind.GameOptions_Emulator;
    public override bool CanSave => true; // TODO-14: Depends on if selected emulator has config

    public GameInstallation GameInstallation { get; }
    public EmulatedGameDescriptor GameDescriptor { get; }

    public ObservableCollection<EmulatorViewModel>? Emulators { get; set; }

    public EmulatorViewModel? SelectedEmulator
    {
        get => _selectedEmulator;
        set
        {
            _selectedEmulator = value;

            Invoke();
            async void Invoke() => await GameDescriptor.SetEmulatorAsync(GameInstallation, value?.EmulatorInstallation);
        }
    }

    // TODO-14: Selected emulator might have config for this game

    protected override Task LoadAsync()
    {
        _selectedEmulator = null;
        OnPropertyChanged(nameof(SelectedEmulator));

        var emulators = Services.Emulators.GetInstalledEmulators().
            Where(x => x.EmulatorDescriptor.SupportedPlatforms.Contains(GameDescriptor.Platform)).
            Select(x => new EmulatorViewModel(x));
        Emulators = new ObservableCollection<EmulatorViewModel>(emulators);

        string? emuId = GameInstallation.GetValue<string>(GameDataKey.Emu_InstallationId);

        if (emuId != null)
        {
            _selectedEmulator = Emulators.FirstOrDefault(x => x.EmulatorInstallation.InstallationId == emuId);
            OnPropertyChanged(nameof(SelectedEmulator));
        }

        return Task.CompletedTask;
    }

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
    public void Receive(ModifiedGamesMessage message)
    {
        if (message.GameInstallations.Contains(GameInstallation))
        {
            string? emuId = GameInstallation.GetValue<string>(GameDataKey.Emu_InstallationId);

            if (emuId != SelectedEmulator?.EmulatorInstallation.InstallationId)
            {
                _selectedEmulator = Emulators?.FirstOrDefault(x => x.EmulatorInstallation.InstallationId == emuId);
                OnPropertyChanged(nameof(SelectedEmulator));
            }
        }
    }

    public override void Dispose()
    {
        base.Dispose();

        // Unregister for messages
        Services.Messenger.UnregisterAll(this);
    }

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
}