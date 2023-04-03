namespace RayCarrot.RCP.Metro;

public class ConfigureGameInstallation : ConfigureProgramInstallation<GameInstallation>
{
    public ConfigureGameInstallation(Action<GameInstallation>? action) : base(action) { }
    public ConfigureGameInstallation(Func<GameInstallation, Task> asyncAction) : base(asyncAction) { }
}