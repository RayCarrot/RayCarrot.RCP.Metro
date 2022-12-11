using System.Runtime.Serialization;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// Extensions methods for <see cref="SerializationInfo"/>
/// </summary>
public static class SerializationInfoExtensions
{
    /// <summary>
    /// Adds a generic value
    /// </summary>
    /// <typeparam name="T">The type of value to add</typeparam>
    /// <param name="serializationInfo">The serialization info</param>
    /// <param name="name">The name of the value</param>
    /// <param name="value">The valueS</param>
    /// <exception cref="ArgumentNullException"/>
    /// <exception cref="SerializationException">A value has already been associated with name</exception>
    public static void AddValue<T>(this SerializationInfo serializationInfo, string name, T value)
    {
        if (serializationInfo == null)
            throw new ArgumentNullException(nameof(serializationInfo));

        if (name == null)
            throw new ArgumentNullException(nameof(name));

        serializationInfo.AddValue(name, value, typeof(T));
    }

    /// <summary>
    /// Gets a generic value
    /// </summary>
    /// <typeparam name="T">The type of value to get</typeparam>
    /// <param name="serializationInfo">The serialization info</param>
    /// <param name="name">The name of the value</param>
    /// <returns>The value</returns>
    /// <exception cref="ArgumentNullException"/>
    /// <exception cref="InvalidCastException">The value associated with name cannot be converted to the specified type</exception>
    /// <exception cref="SerializationException">An element with the specified name is not found in the current instance</exception>
    public static T GetValue<T>(this SerializationInfo serializationInfo, string name)
    {
        if (serializationInfo == null)
            throw new ArgumentNullException(nameof(serializationInfo));

        if (name == null)
            throw new ArgumentNullException(nameof(name));

        return (T)serializationInfo.GetValue(name, typeof(T));
    }
}