#nullable disable
using System;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// Extension methods for an <see cref="Object"/>
/// </summary>
public static class ObjectExtensions
{
    /// <summary>
    /// Casts the object to the specified type
    /// </summary>
    /// <typeparam name="T">The type to cast to</typeparam>
    /// <param name="item">The object to cast</param>
    /// <returns>The same object, cast to the specified type</returns>
    /// <exception cref="InvalidCastException">The item can not be cast to the specified type</exception>
    public static T CastTo<T>(this object item)
    {
        return (T)item;
    }

    /// <summary>
    /// Clamps the value to the specified min and max values
    /// </summary>
    /// <typeparam name="T">The value type</typeparam>
    /// <param name="val">The value to clamp</param>
    /// <param name="min">The minimum allowed value</param>
    /// <param name="max">The maximum allowed value</param>
    /// <returns>The clamped value</returns>
    public static T Clamp<T>(this T val, T min, T max) 
        where T : IComparable<T>
    {
        if (val.CompareTo(min) < 0) 
            return min;
        else if (val.CompareTo(max) > 0) 
            return max;
        else return val;
    }
}