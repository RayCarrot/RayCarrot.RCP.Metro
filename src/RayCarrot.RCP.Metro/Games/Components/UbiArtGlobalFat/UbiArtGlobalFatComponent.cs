namespace RayCarrot.RCP.Metro.Games.Components;

[BaseGameComponent]
[SingleInstanceGameComponent]
public class UbiArtGlobalFatComponent : GameComponent
{
    public UbiArtGlobalFatComponent(string fileName)
    {
        FileName = fileName;
    }

    public string FileName { get; }
}