#nullable disable
using BinarySerializer;

namespace RayCarrot.RCP.Metro;

public class R2GBC_SaveSlot : BinarySerializable
{
    public byte[] SaveData { get; set; }

    // 5:57e0
    public byte GetLivesCount()
    {
        return (byte)(SaveData[0] & 0x7f);
    }

    // 5:57b9
    public void SetLivesCount(byte lives)
    {
        SaveData[0] = (byte)(lives | (SaveData[0] & 0x80));
    }

    // NOTE: Bit 7 of the first byte is an unused leftover from the first game where it determines if the worldmap has been unlocked

    // 5:5832
    public byte GetLevel()
    {
        return (byte)(SaveData[1] & 0x3F);
    }

    // 5:5802
    public void SetLevel(byte level)
    {
        SaveData[2] = (byte)(level | (SaveData[1] & 0xc0));
    }

    // 5:58cb
    public bool GetHasCollectedCage(int cageId)
    {
        int bitIndex = 14 + cageId;
        return (SaveData[bitIndex / 8] & (1 << (bitIndex % 8))) != 0;
    }

    // 5:5854
    public void SetHasCollectedCage(int cageId, bool isCollected)
    {
        int bitIndex = 14 + cageId;
        if (isCollected)
            SaveData[bitIndex / 8] |= (byte)(1 << (bitIndex % 8));
        else
            SaveData[bitIndex / 8] &= (byte)~(1 << (bitIndex % 8));
    }

    // 5:590e
    public int GetTotalCollectedCages()
    {
        int count = 0;

        for (int i = 0; i < 38; i++)
        {
            if (GetHasCollectedCage(i))
                count++;
        }

        return count;
    }

    public bool GetHasCollectedLum(int lumId)
    {
        int bitIndex = 52 + lumId;
        return (SaveData[bitIndex / 8] & (1 << (bitIndex % 8))) != 0;
    }

    public void SetHasCollectedLum(int lumId, bool isCollected)
    {
        int bitIndex = 52 + lumId;
        if (isCollected)
            SaveData[bitIndex / 8] |= (byte)(1 << (bitIndex % 8));
        else
            SaveData[bitIndex / 8] &= (byte)~(1 << (bitIndex % 8));
    }

    // 5:5957
    public int GetCollectedLumsCount()
    {
        int count = 0;

        for (int i = 0; i < 1024; i++)
        {
            if (GetHasCollectedLum(i))
                count++;
        }

        for (int i = 0; i < 256; i++)
        {
            if (GetHasCollectedLum(1024 + i))
                count += 10;
        }

        return count;
    }

    public override void SerializeImpl(SerializerObject s)
    {
        s.DoProcessed(new Checksum8Processor(true), c =>
        {
            c.Serialize<byte>(s, name: "Checksum");
            SaveData = s.SerializeArray<byte>(SaveData, 168, name: nameof(SaveData));
        });
    }
}