namespace RayCarrot.RCP.Metro.Games.Components;

/// <summary>
/// A game component which provides an object factory of a specific type
/// </summary>
/// <typeparam name="T">The type of objects to create</typeparam>
public abstract class FactoryGameComponent<T> : DescriptorComponent
{
    protected FactoryGameComponent(Func<GameInstallation, T> objFactory)
    {
        _objFactory = objFactory;
    }

    private readonly Func<GameInstallation, T> _objFactory;

    public T CreateObject(GameInstallation gameInstallation) => _objFactory(gameInstallation);
}