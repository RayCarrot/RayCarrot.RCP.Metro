namespace RayCarrot.RCP.Metro.Games.Settings;

/// <summary>
/// A key mapping item
/// </summary>
public class Rayman2ButtonMappingItem
{
    public Rayman2ButtonMappingItem(int originalKey, int newKey)
    {
        OriginalKey = originalKey;
        NewKey = newKey;
    }

    public int OriginalKey { get; }
    public int NewKey { get; }
}