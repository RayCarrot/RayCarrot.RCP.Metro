using RayCarrot.RCP.Metro.Games.Components;

namespace RayCarrot.RCP.Metro;

[BaseGameComponent]
[SingleInstanceGameComponent]
public class Win32LaunchPathComponent : FactoryGameComponent<FileSystemPath>
{
    public Win32LaunchPathComponent(Func<GameInstallation, FileSystemPath> objFactory) : base(objFactory) { }
}