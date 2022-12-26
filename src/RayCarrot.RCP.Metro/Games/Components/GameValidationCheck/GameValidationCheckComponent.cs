namespace RayCarrot.RCP.Metro.Games.Components;

[GameComponent(IsBase = true)]
public abstract class GameValidationCheckComponent : GameComponent
{
    public abstract bool IsValid();
}