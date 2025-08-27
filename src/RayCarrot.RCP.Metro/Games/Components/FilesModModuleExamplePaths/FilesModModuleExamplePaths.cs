namespace RayCarrot.RCP.Metro.Games.Components;

[GameComponentBase(SingleInstance = true)]
public class FilesModModuleExamplePaths : GameComponent
{
    public FilesModModuleExamplePaths(Func<string, string?> getExamplePathFunc)
    {
        GetExamplePathFunc = getExamplePathFunc;
    }

    public Func<string, string?> GetExamplePathFunc { get; }
}