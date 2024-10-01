using System.IO;

namespace RayCarrot.RCP.Metro.Games.Settings;

/// <summary>
/// Manages Rayman 2 button mapping through dinput.dll editing
/// </summary>
public static class Rayman2ButtonMappingManager
{
    #region Public Static Methods

    /// <summary>
    /// Loads the current button mapping
    /// </summary>
    /// <param name="dllFile">The dinput.dll path</param>
    /// <returns>The collection of the current mapping used</returns>
    public static async Task<HashSet<Rayman2ButtonMappingItem>> LoadCurrentMappingAsync(FileSystemPath dllFile)
    {
        var output = new HashSet<Rayman2ButtonMappingItem>();

        // Open the file in a new stream
        using var stream = new FileStream(dllFile, FileMode.Open);

        // Create the buffer to read into
        byte[] buffer = new byte[4];

        stream.Seek(CodeAdress + 14, SeekOrigin.Begin);
        await stream.ReadAsync(buffer, 0, 4);
        int originalKey = BitConverter.ToInt32(buffer, 0);

        // Get all key items
        while (originalKey > 0)
        {
            stream.Seek(-14, SeekOrigin.Current);
            await stream.ReadAsync(buffer, 0, 4);
            var newKey = BitConverter.ToInt32(buffer, 0) & 0x000000FF;

            output.Add(new Rayman2ButtonMappingItem(originalKey, newKey));

            stream.Seek(29, SeekOrigin.Current);
            await stream.ReadAsync(buffer, 0, 4);
            originalKey = BitConverter.ToInt32(buffer, 0);
        }

        return output;
    }

    /// <summary>
    /// Applies the specified button mapping
    /// </summary>
    /// <param name="dllFile">The dinput.dll path</param>
    /// <param name="items">The button mapping items to use</param>
    /// <returns>The task</returns>
    public static async Task ApplyMappingAsync(FileSystemPath dllFile, List<Rayman2ButtonMappingItem> items)
    {
        using FileStream stream = new FileStream(dllFile, FileMode.Open, FileAccess.ReadWrite);

        int i;

        stream.Seek(CodeAdress, SeekOrigin.Begin);

        // Write each key item
        for (i = 0; i < items.Count; i++)
        {
            await stream.WriteAsync(Code1, 0, 4);

            stream.WriteByte((byte)items[i].NewKey);

            await stream.WriteAsync(Code2, 0, 9);

            stream.WriteByte((byte)items[i].OriginalKey);

            await stream.WriteAsync(Code3, 0, 4);

            // Custom "jmp" instruction, you should always append to the final codef code
            await stream.WriteAsync(BitConverter.GetBytes((items.Count - i - 1) * 23), 0, 4);
        }

        // End code
        await stream.WriteAsync(Codef, 0, 7);

        uint n = 0xFFFE4082;

        n -= (uint)(23 * i);

        // Custom "jmp" instruction, go back to where we were at the beginning
        await stream.WriteAsync(BitConverter.GetBytes(n), 0, 4);

        // Set the remaining bytes to 0 until the end of the file
        while (stream.Position < stream.Length)
            stream.WriteByte(0);
    }

    #endregion

    #region Private Fields

    private static readonly byte[] Code1 = { 0x8B, 0xC3, 0x83, 0xE8 };
    private static readonly byte[] Code2 = { 0x0F, 0x85, 0x0C, 0x00, 0x00, 0x00, 0xC7, 0x45, 0x10 };
    private static readonly byte[] Code3 = { 0x00, 0x00, 0x00, 0xE9 };
    private static readonly byte[] Codef = { 0xFF, 0x75, 0x10, 0xFF, 0x75, 0x0C, 0xE9 };

    #endregion

    #region Private Constants

    /// <summary>
    /// The offset of the DLL almost to the end of this, from which the code is injected
    /// </summary>
    private const int CodeAdress = 0x213c3;

    #endregion
}