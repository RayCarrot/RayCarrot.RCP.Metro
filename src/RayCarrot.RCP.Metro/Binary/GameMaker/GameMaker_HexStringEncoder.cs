using System;
using System.IO;
using System.Text;
using BinarySerializer;

namespace RayCarrot.RCP.Metro;

public class GameMaker_HexStringEncoder : IStreamEncoder
{
    public string Name => nameof(GameMaker_HexStringEncoder);

    public void DecodeStream(Stream input, Stream output)
    {
        // Use a reader to read as a string
        using StreamReader reader = new(input, Encoding.ASCII, true, 1024, true);

        // Read string
        string str = reader.ReadToEnd();

        // Convert the hex string to bytes
        byte[] bytes = StringToByteArray(str);

        output.Write(bytes, 0, bytes.Length);
    }

    public void EncodeStream(Stream input, Stream output)
    {
        // TODO-UPDATE: Implement
        throw new NotImplementedException();
    }

    private static byte[] StringToByteArray(string hex)
    {
        if (hex.Length % 2 == 1)
            throw new Exception("The hex string cannot have an odd number of characters");

        static int GetHexVal(char hex)
        {
            int val = hex;
            // For uppercase A-F letters:
            // return val - (val < 58 ? 48 : 55);
            // For lowercase a-f letters:
            // return val - (val < 58 ? 48 : 87);
            // Or the two combined, but a bit slower:
            return val - (val < 58 ? 48 : (val < 97 ? 55 : 87));
        }

        byte[] buffer = new byte[hex.Length >> 1];

        for (int i = 0; i < hex.Length >> 1; ++i)
            buffer[i] = (byte)((GetHexVal(hex[i << 1]) << 4) + (GetHexVal(hex[(i << 1) + 1])));

        return buffer;
    }
}