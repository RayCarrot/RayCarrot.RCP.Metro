namespace RayCarrot.RCP.Metro.Games.Components;

[GameComponent]
public abstract class GameValidationCheckComponent : GameComponent
{
    public abstract bool IsValid();
}