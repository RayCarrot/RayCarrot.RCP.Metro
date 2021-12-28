using System;
using System.IO;
using BinarySerializer;

namespace RayCarrot.RCP.Metro;

public class RRR_SaveEncoder : IStreamEncoder
{
    public string Name => nameof(RRR_SaveEncoder);

    public Stream DecodeStream(Stream s)
    {
        using Reader file = new(s, true, leaveOpen: true);

        MemoryStream outputStream = new();

        // Read the magic header which determines if it's encoded
        uint header = file.ReadUInt32();

        // Go back to the start of the stream
        file.BaseStream.Position = 0;

        // Check if it's encoded
        if (header is not (0xC0DE1BAF or 0xC0DE2BAF))
        {
            // If it's not encoded we simply copy over the stream
            s.CopyTo(outputStream);
            return outputStream;
        }

        int[] v73 = new int[32];
        byte[] v74 = new byte[8];

        int sizeWithCRC = (int)file.BaseStream.Length;
        byte[] inputBuffer = file.ReadBytes(sizeWithCRC);

        bool usesMode1 = header == 0xC0DE1BAF; // RRR only uses mode 1!
        bool usesMode2 = header != 0xC0DE1BAF;

        int v9;

        if (usesMode2)
        {
            // v9 = 4;
            throw new NotImplementedException("Save encoding mode 2 is currently not supported");
        }
        else
        {
            byte v10 = (byte)(inputBuffer[4] & 0xF);

            if (v10 == 0 || v10 > sizeWithCRC - 36)
                v10 = 1;

            // TODO: Clean up code and use a loop instead

            int v11 = (byte)(v10 + 4);
            byte v12 = inputBuffer[v11];
            v74[0] = (byte)(v12 & 0xF);
            v73[0] = v11;

            int v13 = ((v12 >> 4) & 0xF) + v11;
            byte v14 = inputBuffer[v13];
            v74[1] = (byte)(v14 & 0xF);
            v73[1] = v13;

            int v15 = ((v14 >> 4) & 0xF) + v13;
            byte v16 = inputBuffer[v15];
            v73[2] = v15;
            int v17 = ((v16 >> 4) & 0xF) + v15;
            byte v18 = inputBuffer[v17];
            v74[2] = (byte)(v16 & 0xF);

            v73[3] = v17;
            int v20 = ((v18 >> 4) & 0xF) + v17;
            byte v21 = inputBuffer[v20];
            v74[3] = (byte)(v18 & 0xF);

            v73[4] = v20;
            int v23 = ((v21 >> 4) & 0xF) + v20;
            byte v24 = inputBuffer[v23];
            v74[4] = (byte)(v21 & 0xF);

            v73[5] = v23;
            int v26 = ((v24 >> 4) & 0xF) + v23;
            byte v27 = inputBuffer[v26];
            v74[5] = (byte)(v24 & 0xF);

            v73[6] = v26;
            int v29 = ((v27 >> 4) & 0xF) + v26;
            byte v30 = inputBuffer[v29];
            v74[6] = (byte)(v27 & 0xF);

            v73[7] = v29;
            v74[7] = (byte)(v30 & 0xF);
            v9 = ((v30 >> 4) & 0xF) + v29;
        }

        byte v32 = (byte)(((sizeWithCRC - (!usesMode1 ? 0 : 8) + 4) & 0x1F) + 1);

        int v33 = sizeWithCRC - (usesMode1 ? 8 : 0) - 28;

        if (v9 + (uint)v32 >= v33)
            v32 = 1;

        int v34 = v32 + v9;

        var v76 = new byte[4 * 6];

        for (int i = 0; i < 24; i++)
        {
            byte v36 = inputBuffer[v34];
            v76[i + 0] = v36;
            uint v37 = (uint)(i + (usesMode1 ? 8 : 0));
            v73[v37] = v34;
            byte v38 = (byte)(v36 % 8 + 1);
            if (v34 + (uint)v38 >= v33)
                v38 = 1;
            v34 = v38 + v34;
        }

        // NOTE: Game checks CRC here

        uint v66 = (uint)sizeWithCRC;
        uint v67 = 4;
        uint v68 = 0;

        for (int j = 0; v67 < v66; ++v67)
        {
            if (v68 < 0x20 && v67 == v73[v68])
            {
                ++v68;
            }
            else
            {
                int v70 = inputBuffer[v67] << (8 - (v74[j & 7] & 7));
                outputStream.WriteByte((byte)(BitHelpers.ExtractBits(v70, 8, 0) + BitHelpers.ExtractBits(v70, 8, 8)));
                ++j;
            }
        }

        return outputStream;
    }

    public Stream EncodeStream(Stream s)
    {
        // Copy the data. The game can read decoded files.
        MemoryStream outputStream = new();
        s.CopyTo(outputStream);
        outputStream.Position = 0;
        return outputStream;
    }
}