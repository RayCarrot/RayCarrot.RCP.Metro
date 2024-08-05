#nullable disable
using BinarySerializer;

namespace RayCarrot.RCP.Metro;

/*
Lum/cage level indexes:

00: ?
01: The Woods of Light
02: The Fairy Glade
03: The Marshes of Awakening
04: The Bayou
05: The Sanctuary of Water and Ice
06: The Menhir Hills
07: The Canopy
08: Whale Bay
09: The Sanctuary of Stone and Fire
10: The Echoing Caves
11: The Precipice
12: ?
13: Beneath the Lava Sanctuary
14: The Lava Sanctuary
15: The Tomb of the Ancients
16: The Pirate Mines
17: The Iron Mountains
18: The Prison Ship
19: ?
20: ?
21: ?
22: Minisouras Plains
23: Globox's House
24: Rainbow Creak
25: The Crow's Nest
26: ?
27: ?
28: ?
29: ?
 */

public class Rayman2Ps2SaveData : BinarySerializable
{
    public byte[] Bytes_00 { get; set; }
    public int Int_10C { get; set; } // 0x00-0x7F
    public int Int_110 { get; set; } // 0x00-0x7E
    public int Int_114 { get; set; } // 0x00-0x7F
    public PersoDsgData[] PersoDsgDatas { get; set; }
    public ushort PersoDsgDatasCount { get; set; }
    public short NextDataOffset { get; set; }
    public byte[] Bytes_205C { get; set; }

    public override void SerializeImpl(SerializerObject s)
    {
        Bytes_00 = s.SerializeArray<byte>(Bytes_00, 268, name: nameof(Bytes_00));
        Int_10C = s.Serialize<int>(Int_10C, name: nameof(Int_10C));
        Int_110 = s.Serialize<int>(Int_110, name: nameof(Int_110));
        Int_114 = s.Serialize<int>(Int_114, name: nameof(Int_114));
        PersoDsgDatas = s.SerializeObjectArray<PersoDsgData>(PersoDsgDatas, 2000, name: nameof(PersoDsgDatas));
        PersoDsgDatasCount = s.Serialize<ushort>(PersoDsgDatasCount, name: nameof(PersoDsgDatasCount));
        NextDataOffset = s.Serialize<short>(NextDataOffset, name: nameof(NextDataOffset));
        Bytes_205C = s.SerializeArray<byte>(Bytes_205C, 32, name: nameof(Bytes_205C));

        // Parse the data. 32 000 bytes reserved for the data.
        List<PersoDsgData> orderedVars = PersoDsgDatas.
            Take(PersoDsgDatasCount).
            OrderBy(x => x.DataOffset).
            ToList();
        for (int i = 0; i < orderedVars.Count; i++)
        {
            PersoDsgData dsgData = orderedVars[i];

            // Determine the length of the data
            long length = (i == orderedVars.Count - 1 ? NextDataOffset : orderedVars[i + 1].DataOffset) - dsgData.DataOffset;

            if (dsgData.LevelId == null)
                s.Log($"Id: {dsgData.Id}, Fix");
            else
                s.Log($"Id: {dsgData.Id}, Level: {dsgData.LevelId}");

            s.DoAt(s.CurrentPointer + dsgData.DataOffset, 
                () => dsgData.SerializeData(s, length));
        }

        // The save data has a fixed length
        s.Goto(Offset + 0x9d7c);
    }

    public class PersoDsgData : BinarySerializable
    {
        public ushort Id { get; set; }
        public ushort? LevelId { get; set; } // null for fix
        public ushort DataOffset { get; set; }

        public byte[] Data { get; set; }
        public Data_Fix_3 Data_Fix_3 { get; set; }

        public override void SerializeImpl(SerializerObject s)
        {
            s.DoBits<ushort>(b =>
            {
                Id = b.SerializeBits<ushort>(Id, 9, name: nameof(Id));
                LevelId = b.SerializeNullableBits<ushort>(LevelId, 7, name: nameof(LevelId));
            });
            DataOffset = s.Serialize<ushort>(DataOffset, name: nameof(DataOffset));
        }

        public void SerializeData(SerializerObject s, long length)
        {
            if (Id == 3 && LevelId == null)
                Data_Fix_3 = s.SerializeObject<Data_Fix_3>(Data_Fix_3, name: nameof(Data_Fix_3));
            else
                Data = s.SerializeArray<byte>(Data, length, nameof(Data));
        }
    }

    public class DsgVarArray<T> : BinarySerializable
        where T : struct
    {
        public int Int_00 { get; set; }
        public byte ElementType { get; set; }
        public byte MaxElementsCount { get; set; }

        public T[] Values { get; set; }

        public override void SerializeImpl(SerializerObject s)
        {
            Int_00 = s.Serialize<int>(Int_00, name: nameof(Int_00));
            ElementType = s.Serialize<byte>(ElementType, name: nameof(ElementType));
            MaxElementsCount = s.Serialize<byte>(MaxElementsCount, name: nameof(MaxElementsCount));
            s.Align();

            Values = s.SerializeArray<T>(Values, MaxElementsCount, name: nameof(Values));
        }
    }

    public class Data_Fix_3 : BinarySerializable
    {
        public int Int_7 { get; set; }
        public DsgVarArray<int> CagesPerLevel { get; set; } // IntegerArray_8
        public int Int_9 { get; set; }
        public DsgVarArray<int> LumsPerLevel { get; set; } // IntegerArray_10
        public int Int_11 { get; set; }
        public int Int_12 { get; set; }
        public int Int_13 { get; set; }
        public int Int_14 { get; set; }
        public int Int_15 { get; set; }
        public int Int_16 { get; set; }
        public int Int_17 { get; set; }
        public bool Boolean_18 { get; set; }
        public bool Boolean_19 { get; set; }
        public bool Boolean_25 { get; set; }
        public DsgVarArray<int> IntegerArray_27 { get; set; }
        public DsgVarArray<int> IntegerArray_28 { get; set; }
        public float DistanceCovered { get; set; } // Float_29 - meters
        public int TimePlayed { get; set; } // Int_30 - milliseconds
        public int EnemyShotsFired { get; set; } // Int_31
        public int ShotsFired { get; set; } // Int_32
        public int ShotsHitTarget { get; set; } // Int_33 - Accuracy: (ShotsHitTarget * 100) / ShotsFired
        public int TimesKilled { get; set; } // Int_34
        public int NumberOfTryAgain { get; set; } // Int_35
        public int SecretsDiscovered { get; set; } // Int_36
        public int NumberOfJumps { get; set; } // Int_37
        public bool Boolean_43 { get; set; }

        public override void SerializeImpl(SerializerObject s)
        {
            Int_7 = s.Serialize<int>(Int_7, name: nameof(Int_7));
            CagesPerLevel = s.SerializeObject<DsgVarArray<int>>(CagesPerLevel, name: nameof(CagesPerLevel));
            Int_9 = s.Serialize<int>(Int_9, name: nameof(Int_9));
            LumsPerLevel = s.SerializeObject<DsgVarArray<int>>(LumsPerLevel, name: nameof(LumsPerLevel));
            Int_11 = s.Serialize<int>(Int_11, name: nameof(Int_11));
            Int_12 = s.Serialize<int>(Int_12, name: nameof(Int_12));
            Int_13 = s.Serialize<int>(Int_13, name: nameof(Int_13));
            Int_14 = s.Serialize<int>(Int_14, name: nameof(Int_14));
            Int_15 = s.Serialize<int>(Int_15, name: nameof(Int_15));
            Int_16 = s.Serialize<int>(Int_16, name: nameof(Int_16));
            Int_17 = s.Serialize<int>(Int_17, name: nameof(Int_17));
            Boolean_18 = s.Serialize<bool>(Boolean_18, name: nameof(Boolean_18));
            s.Align();
            Boolean_19 = s.Serialize<bool>(Boolean_19, name: nameof(Boolean_19));
            s.Align();
            Boolean_25 = s.Serialize<bool>(Boolean_25, name: nameof(Boolean_25));
            s.Align();
            IntegerArray_27 = s.SerializeObject<DsgVarArray<int>>(IntegerArray_27, name: nameof(IntegerArray_27));
            IntegerArray_28 = s.SerializeObject<DsgVarArray<int>>(IntegerArray_28, name: nameof(IntegerArray_28));
            DistanceCovered = s.Serialize<float>(DistanceCovered, name: nameof(DistanceCovered));
            TimePlayed = s.Serialize<int>(TimePlayed, name: nameof(TimePlayed));
            EnemyShotsFired = s.Serialize<int>(EnemyShotsFired, name: nameof(EnemyShotsFired));
            ShotsFired = s.Serialize<int>(ShotsFired, name: nameof(ShotsFired));
            ShotsHitTarget = s.Serialize<int>(ShotsHitTarget, name: nameof(ShotsHitTarget));
            TimesKilled = s.Serialize<int>(TimesKilled, name: nameof(TimesKilled));
            NumberOfTryAgain = s.Serialize<int>(NumberOfTryAgain, name: nameof(NumberOfTryAgain));
            SecretsDiscovered = s.Serialize<int>(SecretsDiscovered, name: nameof(SecretsDiscovered));
            NumberOfJumps = s.Serialize<int>(NumberOfJumps, name: nameof(NumberOfJumps));
            Boolean_43 = s.Serialize<bool>(Boolean_43, name: nameof(Boolean_43));
        }
    }
}