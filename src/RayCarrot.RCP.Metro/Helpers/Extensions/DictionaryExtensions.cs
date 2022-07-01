using System.Collections.Generic;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// Extension methods for <see cref="Dictionary{TKey,TValue}"/>
/// </summary>
public static class DictionaryExtensions
{
    /// <summary>
    /// Tries to get a value from the specified dictionary using the specified key.
    /// Returns the default value if the value can not be found.
    /// </summary>
    /// <typeparam name="TKey">The key type</typeparam>
    /// <typeparam name="TValue">The value type</typeparam>
    /// <param name="dictionary">The dictionary</param>
    /// <param name="key">The key</param>
    /// <returns>The value, or the default value if not found</returns>
    public static TValue? TryGetValue<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
    {
        return dictionary.TryGetValue(key, out TValue output) ? output : default;
    }

    /// <summary>
    /// Tries to get a value from the specified dictionary using the specified key.
    /// Returns the specified default value if the value can not be found.
    /// </summary>
    /// <typeparam name="TKey">The key type</typeparam>
    /// <typeparam name="TValue">The value type</typeparam>
    /// <param name="dictionary">The dictionary</param>
    /// <param name="key">The key</param>
    /// <param name="defaultValue">The default value to return if the value can not be found</param>
    /// <returns>The value, or the specified default value if not found</returns>
    public static TValue? TryGetValue<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue)
    {
        return dictionary.TryGetValue(key, out TValue output) ? output : defaultValue;
    }

    /// <summary>
    /// Tries to get a value from the specified dictionary using the specified key.
    /// Returns the default value if the value can not be found or if it does not match the specified return type.
    /// </summary>
    /// <typeparam name="TKey">The key type</typeparam>
    /// <typeparam name="TValue">The value type</typeparam>
    /// <typeparam name="TReturnValue">The value type to cast to</typeparam>
    /// <param name="dictionary">The dictionary</param>
    /// <param name="key">The key</param>
    /// <returns>The value, or the default value if not found</returns>
    public static TReturnValue? TryGetValue<TKey, TValue, TReturnValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
        where TReturnValue : TValue
    {
        return dictionary.TryGetValue(key, out TValue value) && value is TReturnValue output ? output : default;
    }
}