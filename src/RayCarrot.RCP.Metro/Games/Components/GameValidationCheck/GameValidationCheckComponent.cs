namespace RayCarrot.RCP.Metro.Games.Components;

[GameComponentBase]
public abstract class GameValidationCheckComponent : GameComponent
{
    public abstract bool IsValid();
}