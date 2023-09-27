using System.Windows.Input;

namespace RayCarrot.RCP.Metro.Pages.Settings.Sections;

public class DebugSettingsSectionViewModel : SettingsSectionViewModel
{
    public DebugSettingsSectionViewModel(AppUserData data, FileManager fileManager) : base(data)
    {
        FileManager = fileManager ?? throw new ArgumentNullException(nameof(fileManager));

        OpenFileCommand = new AsyncRelayCommand(x => OpenFileAsync((FileSystemPath)x!));
        OpenDirectoryCommand = new AsyncRelayCommand(x => OpenDirectoryAsync((FileSystemPath)x!));
        OpenRegistryKeyCommand = new AsyncRelayCommand(x => OpenRegistryKeyAsync((string)x!));
    }

    private FileManager FileManager { get; }

    public ICommand OpenFileCommand { get; }
    public ICommand OpenDirectoryCommand { get; }
    public ICommand OpenRegistryKeyCommand { get; }

    public override LocalizedString Header => new ResourceLocString(nameof(Resources.Settings_DebugHeader));
    public override GenericIconKind Icon => GenericIconKind.Settings_Debug;
    public override UserLevel UserLevel => UserLevel.Debug;

    public Task OpenFileAsync(FileSystemPath filePath) => FileManager.LaunchFileAsync(filePath);
    public Task OpenDirectoryAsync(FileSystemPath dirPath) => FileManager.OpenExplorerLocationAsync(dirPath);
    public Task OpenRegistryKeyAsync(string registryKey) => FileManager.OpenRegistryKeyAsync(registryKey);
}