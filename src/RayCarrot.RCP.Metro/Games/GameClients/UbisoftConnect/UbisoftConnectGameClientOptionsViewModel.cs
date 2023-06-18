using System.IO;

namespace RayCarrot.RCP.Metro.Games.Clients.UbisoftConnect;

public class UbisoftConnectGameClientOptionsViewModel : GameClientOptionsViewModel
{
    public UbisoftConnectGameClientOptionsViewModel(GameClientInstallation gameClientInstallation) 
        : base(gameClientInstallation)
    {
        UserIds = new ObservableCollection<string>(GetUserIds());

        if (UserIds.Count > 0 && !UserIds.Contains(SelectedUserId))
            SelectedUserId = UserIds.First();
    }

    public ObservableCollection<string> UserIds { get; }

    public string SelectedUserId
    {
        get => GameClientInstallation.GetValue<string>(GameClientDataKey.UbisoftConnect_UserId) ?? String.Empty;
        set => GameClientInstallation.SetValue<string>(GameClientDataKey.UbisoftConnect_UserId, value);
    }

    private IEnumerable<string> GetUserIds()
    {
        try
        {
            FileSystemPath saveDir = GameClientInstallation.InstallLocation.Directory + "savegames";
            return Directory.GetDirectories(saveDir).Select(x => new FileSystemPath(x).Name);
        }
        catch (Exception ex)
        {
            // TODO-UPDATE: Log
            return Enumerable.Empty<string>();
        }
    }
}