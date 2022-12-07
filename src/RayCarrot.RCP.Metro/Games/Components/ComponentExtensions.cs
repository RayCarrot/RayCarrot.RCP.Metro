using System.Collections.Generic;
using System.Linq;

namespace RayCarrot.RCP.Metro.Games.Components;

public static class ComponentExtensions
{
    public static IEnumerable<T> CreateObjects<T>(
        this IEnumerable<FactoryGameComponent<T>> factories, 
        GameInstallation gameInstallation) => factories.Select(x => x.CreateObject(gameInstallation));

    public static IEnumerable<T> CreateManyObjects<T>(
        this IEnumerable<FactoryGameComponent<IEnumerable<T>>> factories, 
        GameInstallation gameInstallation) => factories.SelectMany(x => x.CreateObject(gameInstallation));
}