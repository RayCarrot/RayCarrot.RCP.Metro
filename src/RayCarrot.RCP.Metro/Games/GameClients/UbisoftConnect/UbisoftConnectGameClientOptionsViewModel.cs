namespace RayCarrot.RCP.Metro.Games.Clients.UbisoftConnect;

public class UbisoftConnectGameClientOptionsViewModel : GameClientOptionsViewModel
{
    public UbisoftConnectGameClientOptionsViewModel(GameClientInstallation gameClientInstallation, IEnumerable<string> availableUserIds) 
        : base(gameClientInstallation)
    {
        UserIds = new ObservableCollection<string>(availableUserIds);

        if (UserIds.Count > 0 && !UserIds.Contains(SelectedUserId))
            SelectedUserId = UserIds.First();
    }

    public ObservableCollection<string> UserIds { get; }

    public string SelectedUserId
    {
        get => GameClientInstallation.GetValue<string>(GameClientDataKey.UbisoftConnect_UserId) ?? String.Empty;
        set => GameClientInstallation.SetValue<string>(GameClientDataKey.UbisoftConnect_UserId, value);
    }
}