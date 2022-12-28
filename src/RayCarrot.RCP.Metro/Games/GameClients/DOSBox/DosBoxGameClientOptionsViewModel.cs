namespace RayCarrot.RCP.Metro.Games.Clients.DosBox;

public class DosBoxGameClientOptionsViewModel : GameClientOptionsViewModel
{
    public DosBoxGameClientOptionsViewModel(GameClientInstallation gameClientInstallation, GameClientDescriptor gameClientDescriptor) 
        : base(gameClientInstallation)
    {
        GameClientDescriptor = gameClientDescriptor;
    }

    public GameClientDescriptor GameClientDescriptor { get; }

    public FileSystemPath ConfigFilePath
    {
        get => GameClientInstallation.GetValue(GameClientDataKey.DosBox_ConfigFilePath, FileSystemPath.EmptyPath);
        set
        {
            GameClientInstallation.SetValue(GameClientDataKey.DosBox_ConfigFilePath, value);
            GameClientDescriptor.RefreshGames(GameClientInstallation);
        }
    }
}