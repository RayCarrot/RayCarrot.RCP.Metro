namespace RayCarrot.RCP.Metro.Games.Components;

/// <summary>
/// Defines a config file to load with DOSBox
/// </summary>
[BaseGameComponent]
public class DosBoxConfigFileComponent : FactoryGameComponent<FileSystemPath>
{
    public DosBoxConfigFileComponent(Func<GameInstallation, FileSystemPath> objFactory) : base(objFactory) { }
}