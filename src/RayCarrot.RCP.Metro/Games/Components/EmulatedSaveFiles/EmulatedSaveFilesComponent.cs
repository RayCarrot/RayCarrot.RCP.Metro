namespace RayCarrot.RCP.Metro.Games.Components;

[GameComponentBase]
public class EmulatedSaveFilesComponent : FactoryGameComponent<IEnumerable<EmulatedSaveFile>>
{
    public EmulatedSaveFilesComponent(Func<GameInstallation, IEnumerable<EmulatedSaveFile>> objFactory) : base(objFactory)
    { }
}