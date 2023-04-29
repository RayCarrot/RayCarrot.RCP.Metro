namespace RayCarrot.RCP.Metro.Games.Clients.Custom;

public class CustomGameClientOptionsViewModel : GameClientOptionsViewModel
{
    public CustomGameClientOptionsViewModel(GameClientInstallation gameClientInstallation) : base(gameClientInstallation) { }

    public string LaunchArgs
    {
        get => GameClientInstallation.GetValue<string>(GameClientDataKey.Custom_LaunchArgs) ?? String.Empty;
        set => GameClientInstallation.SetValue(GameClientDataKey.Custom_LaunchArgs, value);
    }
}