using RayCarrot.RCP.Metro.Games.Emulators;

namespace RayCarrot.RCP.Metro;

public class EmulatorsSetupViewModel : BaseViewModel, IRecipient<AddedEmulatorsMessage>, IRecipient<RemovedEmulatorsMessage>
{
    public EmulatorsSetupViewModel()
    {
        InstalledEmulators = new ObservableCollection<InstalledEmulatorViewModel>();
        AvailableEmulators = new ObservableCollection<AvailableEmulatorViewModel>(
            Services.Emulators.GetEmulatorDescriptors().Select(x => new AvailableEmulatorViewModel(x)));

        Refresh();

        Services.Messenger.RegisterAll(this);
    }

    public ObservableCollection<InstalledEmulatorViewModel> InstalledEmulators { get; }
    public ObservableCollection<AvailableEmulatorViewModel> AvailableEmulators { get; }

    public InstalledEmulatorViewModel? SelectedEmulator { get; set; }

    public void Refresh(EmulatorInstallation? selectedEmulatorInstallation = null)
    {
        // TODO: Lock

        InstalledEmulators.Clear();

        foreach (EmulatorInstallation emulatorInstallation in Services.Emulators.GetInstalledEmulators())
        {
            InstalledEmulatorViewModel viewModel = new(emulatorInstallation);
            InstalledEmulators.Add(viewModel);

            if (selectedEmulatorInstallation == emulatorInstallation)
                SelectedEmulator = viewModel;

        }
    }

    public void Receive(AddedEmulatorsMessage message) => 
        Refresh(message.EmulatorInstallations.FirstOrDefault());
    public void Receive(RemovedEmulatorsMessage message) => 
        Refresh(SelectedEmulator?.EmulatorInstallation);
}