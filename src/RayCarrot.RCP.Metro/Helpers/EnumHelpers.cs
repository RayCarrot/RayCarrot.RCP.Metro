#nullable disable
namespace RayCarrot.RCP.Metro;

/// <summary>
/// Helper methods for an <see cref="Enum"/>
/// </summary>
public static class EnumHelpers
{
    /// <summary>
    /// Gets all values for an <see cref="Enum"/>
    /// </summary>
    /// <typeparam name="T">The type of enum</typeparam>
    /// <returns>An array with the values</returns>
    /// <exception cref="ArgumentException">input is not an <see cref="Enum"/></exception>
    /// <exception cref="InvalidOperationException">The method is invoked by reflection in a reflection-only context, -or- 
    /// input s a type from an assembly loaded in a reflection-only context</exception>
    public static T[] GetValues<T>()
        where T : Enum
    {
        return (T[])Enum.GetValues(typeof(T));
    }
}