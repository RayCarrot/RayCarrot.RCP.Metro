namespace RayCarrot.RCP.Metro.Games.Components;

[BaseGameComponent]
public abstract class GameValidationCheckComponent : GameComponent
{
    public abstract bool IsValid();
}