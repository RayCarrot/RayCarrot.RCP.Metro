namespace RayCarrot.RCP.Metro.Games.Components;

[GameComponentBase]
public class ModLibraryPathComponent : FactoryGameComponent<FileSystemPath>
{
    public ModLibraryPathComponent(Func<GameInstallation, FileSystemPath> objFactory) : base(objFactory) { }
}