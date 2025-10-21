namespace RayCarrot.RCP.Metro.Games.Components;

public interface IGameComponentBuilder
{
    void Register(Type baseType, Type instanceType, GameComponent? instance, ComponentPriority priority, ComponentFlags flags);
}