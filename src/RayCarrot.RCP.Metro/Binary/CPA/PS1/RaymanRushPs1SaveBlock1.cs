#nullable disable
using BinarySerializer;

namespace RayCarrot.RCP.Metro;

public class RaymanRushPs1SaveBlock1 : CPAPs1SaveBlock
{
    public override int AllocatedSize => 0x3B0;
    public override int ExpectedSize => 0x360;

    public int Random { get; set; } // Random value between 0-99, probably so checksum changes when saving?
    public int Language { get; set; }
    public int ScreenOffsetX { get; set; }
    public int ScreenOffsetY { get; set; }
    public ushort Percentage { get; set; } // 0-1000
    public byte[] Bytes_12 { get; set; } // Padding?
    public RaymanRushLevelSave[] Levels { get; set; }
    public ushort Player1JumpInput { get; set; } // Inputs are saved as PSX pad flags in big endian?
    public ushort Player2JumpInput { get; set; }
    public ushort Player1ShootInput { get; set; }
    public ushort Player2ShootInput { get; set; }
    public ushort Player1OptimizeInput { get; set; }
    public ushort Player2OptimizeInput { get; set; }
    public ushort Player1NoInput { get; set; }
    public ushort Player2NoInput { get; set; }
    public short[] LumCounts { get; set; } // Per level - never updated
    public short[] TargetCounts { get; set; } // Per level - never updated
    public short Short_324 { get; set; } // 1-6, checked against 5 a lot
    public byte[] Bytes_326 { get; set; }
    public short Short_328 { get; set; }
    public short Short_32A { get; set; }
    public short Short_32C { get; set; }
    public short TotalLums { get; set; } // Never updated
    public short LevelIndex { get; set; }
    public byte Byte_332 { get; set; }
    public byte Byte_333 { get; set; } // Padding?
    public byte[] Bytes_334 { get; set; }
    public byte Byte_338 { get; set; }
    public byte Byte_339 { get; set; }
    public string Name { get; set; }
    public byte[] Bytes_342 { get; set; }
    public byte SfxVolume { get; set; }
    public byte MusicVolume { get; set; }
    public byte Byte_34C { get; set; }
    public byte Byte_34D { get; set; }
    public byte Byte_34E { get; set; } // Bool?
    public byte Byte_34F { get; set; }
    public byte[] Bytes_350 { get; set; }
    public int Int_354 { get; set; }
    public byte[] Bytes_358 { get; set; }

    public int Checksum { get; set; }

    protected override void SerializeData(SerializerObject s)
    {
        s.DoProcessed(new Checksum32Processor(valueSize: 2), p =>
        {
            Random = s.Serialize<int>(Random, name: nameof(Random));
            Language = s.Serialize<int>(Language, name: nameof(Language));
            ScreenOffsetX = s.Serialize<int>(ScreenOffsetX, name: nameof(ScreenOffsetX));
            ScreenOffsetY = s.Serialize<int>(ScreenOffsetY, name: nameof(ScreenOffsetY));
            Percentage = s.Serialize<ushort>(Percentage, name: nameof(Percentage));
            Bytes_12 = s.SerializeArray<byte>(Bytes_12, 2, name: nameof(Bytes_12));
            Levels = s.SerializeObjectArray<RaymanRushLevelSave>(Levels, 12, name: nameof(Levels));
            Player1JumpInput = s.Serialize<ushort>(Player1JumpInput, name: nameof(Player1JumpInput));
            Player2JumpInput = s.Serialize<ushort>(Player2JumpInput, name: nameof(Player2JumpInput));
            Player1ShootInput = s.Serialize<ushort>(Player1ShootInput, name: nameof(Player1ShootInput));
            Player2ShootInput = s.Serialize<ushort>(Player2ShootInput, name: nameof(Player2ShootInput));
            Player1OptimizeInput = s.Serialize<ushort>(Player1OptimizeInput, name: nameof(Player1OptimizeInput));
            Player2OptimizeInput = s.Serialize<ushort>(Player2OptimizeInput, name: nameof(Player2OptimizeInput));
            Player1NoInput = s.Serialize<ushort>(Player1NoInput, name: nameof(Player1NoInput));
            Player2NoInput = s.Serialize<ushort>(Player2NoInput, name: nameof(Player2NoInput));
            LumCounts = s.SerializeArray<short>(LumCounts, 12, name: nameof(LumCounts));
            TargetCounts = s.SerializeArray<short>(TargetCounts, 12, name: nameof(TargetCounts));
            Short_324 = s.Serialize<short>(Short_324, name: nameof(Short_324));
            Bytes_326 = s.SerializeArray<byte>(Bytes_326, 2, name: nameof(Bytes_326));
            Short_328 = s.Serialize<short>(Short_328, name: nameof(Short_328));
            Short_32A = s.Serialize<short>(Short_32A, name: nameof(Short_32A));
            Short_32C = s.Serialize<short>(Short_32C, name: nameof(Short_32C));
            TotalLums = s.Serialize<short>(TotalLums, name: nameof(TotalLums));
            LevelIndex = s.Serialize<short>(LevelIndex, name: nameof(LevelIndex));
            Byte_332 = s.Serialize<byte>(Byte_332, name: nameof(Byte_332));
            Byte_333 = s.Serialize<byte>(Byte_333, name: nameof(Byte_333));
            Bytes_334 = s.SerializeArray<byte>(Bytes_334, 4, name: nameof(Bytes_334));
            Byte_338 = s.Serialize<byte>(Byte_338, name: nameof(Byte_338));
            Byte_339 = s.Serialize<byte>(Byte_339, name: nameof(Byte_339));
            Name = s.SerializeString(Name, length: 8, name: nameof(Name));
            Bytes_342 = s.SerializeArray<byte>(Bytes_342, 8, name: nameof(Bytes_342));
            SfxVolume = s.Serialize<byte>(SfxVolume, name: nameof(SfxVolume));
            MusicVolume = s.Serialize<byte>(MusicVolume, name: nameof(MusicVolume));
            Byte_34C = s.Serialize<byte>(Byte_34C, name: nameof(Byte_34C));
            Byte_34D = s.Serialize<byte>(Byte_34D, name: nameof(Byte_34D));
            Byte_34E = s.Serialize<byte>(Byte_34E, name: nameof(Byte_34E));
            Byte_34F = s.Serialize<byte>(Byte_34F, name: nameof(Byte_34F));
            Bytes_350 = s.SerializeArray<byte>(Bytes_350, 4, name: nameof(Bytes_350));
            Int_354 = s.Serialize<int>(Int_354, name: nameof(Int_354));
            Bytes_358 = s.SerializeArray<byte>(Bytes_358, 4, name: nameof(Bytes_358));
            p.Serialize<int>(s, name: "Checksum");
            Checksum = (int)p.CalculatedValue;
        });
    }
}