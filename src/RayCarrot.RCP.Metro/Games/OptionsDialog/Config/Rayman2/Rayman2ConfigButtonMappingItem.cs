namespace RayCarrot.RCP.Metro.Games.OptionsDialog;

/// <summary>
/// A key mapping item
/// </summary>
public class Rayman2ConfigButtonMappingItem
{
    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="originalKey">The original key</param>
    /// <param name="newKey">The new key</param>
    public Rayman2ConfigButtonMappingItem(int originalKey, int newKey)
    {
        OriginalKey = originalKey;
        NewKey = newKey;
    }

    /// <summary>
    /// The original key
    /// </summary>
    public int OriginalKey { get; }

    /// <summary>
    /// The new key
    /// </summary>
    public int NewKey { get; }
}