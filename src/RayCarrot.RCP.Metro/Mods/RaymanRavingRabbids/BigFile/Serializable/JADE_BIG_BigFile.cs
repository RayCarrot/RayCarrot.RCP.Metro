﻿#nullable disable
using System;
using System.Collections.Generic;
using RayCarrot.Binary;

namespace RayCarrot.RCP.Metro;

public class JADE_BIG_BigFile : IBinarySerializable
{
    public static readonly byte[] XORKey = { 0xb3, 0x98, 0xcc, 0x66 };
    public static uint HeaderLength => 44;

    public string BIG_gst { get; set; }
    public uint Version { get; set; }
    public uint MaxFile { get; set; }
    public uint MaxDir { get; set; }
    public uint MaxKey { get; set; }
    public uint Root { get; set; }
    public int FirstFreeFile { get; set; }
    public int FirstFreeDirectory { get; set; }
    public uint SizeOfFat { get; set; }
    public uint NumFat { get; set; }
    public uint UniverseKey { get; set; } // First file it loads

    public long FatFilesOffset { get; set; }
    public JADE_BIG_FatFile[] FatFiles { get; set; }

    public Dictionary<uint, uint> KeyToPos { get; } = new Dictionary<uint, uint>();

    public void Serialize(IBinarySerializer s)
    {
        BIG_gst = s.SerializeString(BIG_gst, 4, name: nameof(BIG_gst));

        XORIfNecessary(s, () => 
        {
            Version = s.Serialize<uint>(Version, name: nameof(Version));
            MaxFile = s.Serialize<uint>(MaxFile, name: nameof(MaxFile));
            MaxDir = s.Serialize<uint>(MaxDir, name: nameof(MaxDir));
            MaxKey = s.Serialize<uint>(MaxKey, name: nameof(MaxKey));
            Root = s.Serialize<uint>(Root, name: nameof(Root));
            FirstFreeFile = s.Serialize<int>(FirstFreeFile, name: nameof(FirstFreeFile));
            FirstFreeDirectory = s.Serialize<int>(FirstFreeDirectory, name: nameof(FirstFreeDirectory));
            SizeOfFat = s.Serialize<uint>(SizeOfFat, name: nameof(SizeOfFat));
            NumFat = s.Serialize<uint>(NumFat, name: nameof(NumFat));
        });

        UniverseKey = s.Serialize<uint>(UniverseKey, name: nameof(UniverseKey));
            
        if (Version >= 43)
            throw new NotImplementedException("V43: Not implemented in RayCarrot.Binary");
            
        FatFilesOffset = s.Stream.Position;
        SerializeFatFiles(s);

        foreach (var fat in FatFiles)
        {
            foreach (var f in fat.Files)
            {
                KeyToPos[f.Key] = f.FileOffset;
            }
        }
    }

    public void SerializeFatFiles(IBinarySerializer s)
    {
        s.Stream.Position = FatFilesOffset;
        XORIfNecessary(s, () => 
        {
            FatFiles = s.SerializeObjectArray<JADE_BIG_FatFile>(FatFiles, (int)NumFat, onPreSerializing: (_, ff) => 
            {
                ff.Pre_Big = this;
            }, name: nameof(FatFiles));
        });
    }

    public void XORIfNecessary(IBinarySerializer s, Action action)
    {
        if (BIG_gst == "BUG")
            throw new NotImplementedException("Not implemented in RayCarrot.Binary version");
        else
            action();
    }
}