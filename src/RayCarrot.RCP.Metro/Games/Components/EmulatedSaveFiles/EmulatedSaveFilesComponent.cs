namespace RayCarrot.RCP.Metro.Games.Components;

[BaseGameComponent]
public class EmulatedSaveFilesComponent : FactoryGameComponent<IEnumerable<EmulatedSaveFile>>
{
    public EmulatedSaveFilesComponent(Func<GameInstallation, IEnumerable<EmulatedSaveFile>> objFactory) : base(objFactory)
    { }
}