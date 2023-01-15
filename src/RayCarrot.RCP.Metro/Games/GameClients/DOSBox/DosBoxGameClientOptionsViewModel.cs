using System.Windows.Input;
using RayCarrot.RCP.Metro.Games.Clients.Data;

namespace RayCarrot.RCP.Metro.Games.Clients.DosBox;

public class DosBoxGameClientOptionsViewModel : GameClientOptionsViewModel
{
    public DosBoxGameClientOptionsViewModel(GameClientInstallation gameClientInstallation, GameClientDescriptor gameClientDescriptor) 
        : base(gameClientInstallation)
    {
        GameClientDescriptor = gameClientDescriptor;

        var configFiles = GameClientInstallation.GetObject<DosBoxConfigFilePaths>(GameClientDataKey.DosBox_ConfigFilePaths);

        if (configFiles != null)
            ConfigFiles = new ObservableCollection<FileSystemPath>(configFiles.FilePaths);
        else
            ConfigFiles = new ObservableCollection<FileSystemPath>();

        AddConfigFileCommand = new AsyncRelayCommand(AddConfigFileAsync);
    }

    public ICommand AddConfigFileCommand { get; }

    public GameClientDescriptor GameClientDescriptor { get; }

    public ObservableCollection<FileSystemPath> ConfigFiles { get; }

    public async Task AddConfigFileAsync()
    {
        FileBrowserResult browseResult = await Services.BrowseUI.BrowseFileAsync(new FileBrowserViewModel
        {
            // TODO-UPDATE: Localize
            Title = "Select a config file",
            ExtensionFilter = "Conf (*.conf)|*.conf",
        });

        if (browseResult.CanceledByUser)
            return;

        ConfigFiles.Add(browseResult.SelectedFile);

        ApplyChanges();
    }

    public void ApplyChanges()
    {
        GameClientInstallation.SetObject(GameClientDataKey.DosBox_ConfigFilePaths, new DosBoxConfigFilePaths(ConfigFiles.ToList()));
        GameClientDescriptor.RefreshGames(GameClientInstallation);
    }
}