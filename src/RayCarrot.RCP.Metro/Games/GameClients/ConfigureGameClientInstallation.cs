namespace RayCarrot.RCP.Metro.Games.Clients;

public class ConfigureGameClientInstallation : ConfigureProgramInstallation<GameClientInstallation>
{
    public ConfigureGameClientInstallation(Action<GameClientInstallation>? action) : base(action) { }
    public ConfigureGameClientInstallation(Func<GameClientInstallation, Task> asyncAction) : base(asyncAction) { }
}