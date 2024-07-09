#nullable disable
using BinarySerializer;

namespace RayCarrot.RCP.Metro;

// TODO: Figure out what the different variables are and how the id works.
//       ID 6-127: 3 ints, where last two are volumes
//       ID 3-127: 376 bytes - a lot of data, maybe contains bitfields for lums and cages?
public class Rayman2Ps2SaveData : BinarySerializable
{
    public byte[] Bytes_00 { get; set; }
    public int Int_10C { get; set; } // 0x00-0x7F
    public int Int_110 { get; set; } // 0x00-0x7E
    public int Int_114 { get; set; } // 0x00-0x7F
    public SavedDsgVar[] DsgVars { get; set; }
    public ushort DsgVarsCount { get; set; }
    public short NextDsgVarsDataOffset { get; set; }
    public byte[] Bytes_205C { get; set; }

    public override void SerializeImpl(SerializerObject s)
    {
        Bytes_00 = s.SerializeArray<byte>(Bytes_00, 268, name: nameof(Bytes_00));
        Int_10C = s.Serialize<int>(Int_10C, name: nameof(Int_10C));
        Int_110 = s.Serialize<int>(Int_110, name: nameof(Int_110));
        Int_114 = s.Serialize<int>(Int_114, name: nameof(Int_114));
        DsgVars = s.SerializeObjectArray<SavedDsgVar>(DsgVars, 2000, name: nameof(DsgVars));
        DsgVarsCount = s.Serialize<ushort>(DsgVarsCount, name: nameof(DsgVarsCount));
        NextDsgVarsDataOffset = s.Serialize<short>(NextDsgVarsDataOffset, name: nameof(NextDsgVarsDataOffset));
        Bytes_205C = s.SerializeArray<byte>(Bytes_205C, 32, name: nameof(Bytes_205C));

        // Parse the data. 32 000 bytes reserved for the data.
        List<SavedDsgVar> orderedVars = DsgVars.
            Take(DsgVarsCount).
            OrderBy(x => x.DataOffset).
            ToList();
        for (int i = 0; i < orderedVars.Count; i++)
        {
            SavedDsgVar dsgVar = orderedVars[i];

            // Determine the length of the data
            long length = (i == orderedVars.Count - 1 ? NextDsgVarsDataOffset : orderedVars[i + 1].DataOffset) - dsgVar.DataOffset;

            s.Log($"DsgVar Id: {dsgVar.Id_0}-{dsgVar.Id_1}");
            s.DoAt(s.CurrentPointer + dsgVar.DataOffset, 
                () => dsgVar.Data = s.SerializeArray<byte>(dsgVar.Data, length, nameof(dsgVar.Data)));
        }

        // The save data has a fixed length
        s.Goto(Offset + 0x9d7c);
    }

    public class SavedDsgVar : BinarySerializable
    {
        // Some identifier for the variable. Seems to consist of two values?
        public ushort Id_0 { get; set; }
        public ushort Id_1 { get; set; }

        public ushort DataOffset { get; set; }

        public byte[] Data { get; set; }

        public override void SerializeImpl(SerializerObject s)
        {
            s.DoBits<ushort>(b =>
            {
                Id_0 = b.SerializeBits<ushort>(Id_0, 9, name: nameof(Id_0));
                Id_1 = b.SerializeBits<ushort>(Id_1, 7, name: nameof(Id_1));
            });
            DataOffset = s.Serialize<ushort>(DataOffset, name: nameof(DataOffset));
        }
    }
}