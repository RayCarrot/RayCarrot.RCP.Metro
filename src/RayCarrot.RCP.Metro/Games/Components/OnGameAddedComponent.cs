namespace RayCarrot.RCP.Metro;

public class OnGameAddedComponent : ActionGameComponent
{
    public OnGameAddedComponent(Func<GameInstallation, Task> asyncAction) : base(asyncAction) { }
    public OnGameAddedComponent(Action<GameInstallation> action) : base(action) { }
}