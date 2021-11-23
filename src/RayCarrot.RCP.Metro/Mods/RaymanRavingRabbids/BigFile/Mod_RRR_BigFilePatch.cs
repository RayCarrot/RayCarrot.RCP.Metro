﻿using System.Linq;
using RayCarrot.Binary;

namespace RayCarrot.RCP.Metro;

public class Mod_RRR_BigFilePatch
{
    #region Constructor

    public Mod_RRR_BigFilePatch(uint fileKey, uint fileOffset, params byte[][] patchBytes)
    {
        FileKey = fileKey;
        FileOffset = fileOffset;
        PatchBytes = patchBytes;
    }

    #endregion

    #region Public Properties

    public uint FileKey { get; }
    public uint FileOffset { get; }
    public byte[][] PatchBytes { get; }
    public uint[] Sizes { get; init; }
    public WriteMode Mode { get; init; } = WriteMode.Overwrite;

    #endregion

    #region Public Methods

    public void Apply(BinarySerializer s, JADE_BIG_BigFile bf, int patchToApply)
    {
        uint keyToPos = bf.KeyToPos[FileKey];

        if (Sizes != null)
        {
            uint newSize = Sizes[patchToApply];

            // Write new size
            s.Stream.Position = keyToPos;
            s.Serialize<uint>(newSize, name: "Size");
        }

        uint fileStartOffset = keyToPos + 4;
        uint offsetToWriteTo = fileStartOffset + FileOffset;
        byte[] bytesToWrite = PatchBytes[patchToApply];

        // Write patched bytes
        s.Stream.Position = offsetToWriteTo;
        s.SerializeArray<byte>(bytesToWrite, bytesToWrite.Length, name: "PatchBytes");

        if ((Mode == WriteMode.Insert && Sizes != null) || (patchToApply == 0 && Sizes != null))
        {
            if (patchToApply == 0)
            {
                // Original - Append 0x0 removing the rest of the patch
                bytesToWrite = Enumerable.Repeat((byte)0, (int)(Sizes.Max() - Sizes[0])).ToArray();
            }
            else
            {
                // Patch - Append original data
                bytesToWrite = PatchBytes[0];
            }

            s.SerializeArray<byte>(bytesToWrite, bytesToWrite.Length, name: "PatchBytes");
        }
    }

    public int GetAppliedPatch(BinaryDeserializer s, JADE_BIG_BigFile bf)
    {
        uint keyToPos = bf.KeyToPos[FileKey];

        if (Sizes != null && PatchBytes.Length == 2)
        {
            // Read the current size
            s.Stream.Position = keyToPos;
            uint size = s.Serialize<uint>(default, name: "CurrentSize");

            if (size == Sizes[1])
                return 1;
            else
                return 0;
        }

        uint fileStartOffset = keyToPos + 4;
        uint offsetToWriteTo = fileStartOffset + FileOffset;

        s.Stream.Position = offsetToWriteTo;
        byte[] bytes = s.SerializeArray<byte>(default, PatchBytes[1].Length, name: "CurrentBytes");

        for (int i = 1; i < PatchBytes.Length; i++)
        {
            if (PatchBytes[i].SequenceEqual(bytes))
                return i;
        }

        return 0;
    }

    #endregion

    #region Data Types

    public enum WriteMode
    {
        Overwrite,
        Insert
    }

    #endregion

    #region Patches: Sound

    public static Mod_RRR_BigFilePatch[] FixSoundEffects => new Mod_RRR_BigFilePatch[] 
    {
        // Patch Rayman's sound effect bank reference
        new Mod_RRR_BigFilePatch(0x9E00DCD3, 0x1900,
            new byte[] { 0xF7, 0x43, 0x00, 0x87 }, // 0 - Original
            new byte[] { 0xEF, 0x08, 0x00, 0x2C }  // 1 - Patched
        ),

        // Patch 1 sound modifier reference in _Music_juice.snk
        new Mod_RRR_BigFilePatch(0x8700446A, 0x10,
            new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, // 0 - Original
            new byte[] { 0x67, 0x44, 0x00, 0x87, 0x2E, 0x73, 0x6D, 0x64 }  // 1 - Patched
        ),

        // Stop bank merges from restoring the odd footstep sound
        new Mod_RRR_BigFilePatch(0x2C0018E1, 0xF0,
            new byte[] { 0x00, 0x12, 0x00, 0x2C, 0x2E, 0x73, 0x6D, 0x64 }, // 0 - Original
            new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }  // 1 - Patched
        ),
        new Mod_RRR_BigFilePatch(0x2C0025C2, 0xF0,
            new byte[] { 0x00, 0x12, 0x00, 0x2C, 0x2E, 0x73, 0x6D, 0x64 }, // 0 - Original
            new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }  // 1 - Patched
        ),
    };

    public static Mod_RRR_BigFilePatch MakeBatSoundLikeEagle => new Mod_RRR_BigFilePatch(0x3A0031F4, 0x13D8,
        new byte[] { 0xC6, 0x15, 0x00, 0x2C }, // 0 - Original
        new byte[] { 0xF8, 0x43, 0x00, 0x87 }  // 1 - Patched
    );

    public static Mod_RRR_BigFilePatch[] MakeSpiderRobotsSoundLikeSpider => new Mod_RRR_BigFilePatch[] 
    {
        // 17003CE2__PNJ_RM4_Quadripode_1.gao
        new Mod_RRR_BigFilePatch(0x17003CE2, 0xB98,
            new byte[] { 0xA4, 0xAC, 0x00, 0x72 }, // 0 - Original
            new byte[] { 0xD5, 0x07, 0x00, 0x2C }  // 1 - Patched
        ),
        // 17003D65__PNJ_RM4_Quadripode_2.gao
        new Mod_RRR_BigFilePatch(0x17003D65, 0xB98,
            new byte[] { 0xA4, 0xAC, 0x00, 0x72 }, // 0 - Original
            new byte[] { 0xD5, 0x07, 0x00, 0x2C }  // 1 - Patched
        ),
        // 0D00A19C__PNJ_RM4_Quadripode_Roof.gao
        new Mod_RRR_BigFilePatch(0x0D00A19C, 0xB98,
            new byte[] { 0xA4, 0xAC, 0x00, 0x72 }, // 0 - Original
            new byte[] { 0xD5, 0x07, 0x00, 0x2C }  // 1 - Patched
        ),
        // 0B01292A_[0b012926] 05_PNJ_RM4_Quadripode_00_00.gao
        new Mod_RRR_BigFilePatch(0x0B01292A, 0xB98,
            new byte[] { 0xA4, 0xAC, 0x00, 0x72 }, // 0 - Original
            new byte[] { 0xD5, 0x07, 0x00, 0x2C }  // 1 - Patched
        ),
        // 0B013D6D_[0b013d69] 05_PNJ_RM4_Quadripode_01.gao
        new Mod_RRR_BigFilePatch(0x0B013D6D, 0xB98,
            new byte[] { 0xA4, 0xAC, 0x00, 0x72 }, // 0 - Original
            new byte[] { 0xD5, 0x07, 0x00, 0x2C }  // 1 - Patched
        ),
        // 0B013F4C_05_PNJ_RM4_Quadripode_00_01.gao
        new Mod_RRR_BigFilePatch(0x0B013F4C, 0xB98,
            new byte[] { 0xA4, 0xAC, 0x00, 0x72 }, // 0 - Original
            new byte[] { 0xD5, 0x07, 0x00, 0x2C }  // 1 - Patched
        ),
        // 0D0083A5_[0d0083a1] _PNJ_RM4_Quadripode.gao
        new Mod_RRR_BigFilePatch(0x0D0083A5, 0xA90,
            new byte[] { 0xA4, 0xAC, 0x00, 0x72 }, // 0 - Original
            new byte[] { 0xD5, 0x07, 0x00, 0x2C }  // 1 - Patched
        ),
        // 0D00886F__PNJ_RM4_Quadripode_B.gao
        new Mod_RRR_BigFilePatch(0x0D00886F, 0xA90,
            new byte[] { 0xA4, 0xAC, 0x00, 0x72 }, // 0 - Original
            new byte[] { 0xD5, 0x07, 0x00, 0x2C }  // 1 - Patched
        ),
        // 3A003FAB__PNJ_RM4_Quadripode.gao
        new Mod_RRR_BigFilePatch(0x3A003FAB, 0xA90,
            new byte[] { 0xA4, 0xAC, 0x00, 0x72 }, // 0 - Original
            new byte[] { 0xD5, 0x07, 0x00, 0x2C }  // 1 - Patched
        ),
    };

    #endregion

    #region Patches: Models

    public static Mod_RRR_BigFilePatch PlayableCharacters => new Mod_RRR_BigFilePatch(0x9E00DCD3, 0x80,
        new byte[] { 0x59, 0x05, 0x01, 0x8F, 0x4F, 0x05, 0x01, 0x8F }, // 0 - Original
        new byte[] { 0x55, 0x37, 0x01, 0x8F, 0x50, 0x37, 0x01, 0x8F }, // 1 - Globox
        new byte[] { 0xFE, 0x6C, 0x00, 0xCA, 0x16, 0x0C, 0x01, 0x8F }, // 2 - Serguei
        new byte[] { 0xFF, 0x07, 0x01, 0x8F, 0xD2, 0x07, 0x01, 0x8F }, // 3 - Rabbid
        new byte[] { 0xAE, 0x1A, 0x00, 0x50, 0xA3, 0x1A, 0x00, 0x50 }, // 4 - Grey Leader Rabbid
        new byte[] { 0xCF, 0x18, 0x00, 0x50, 0xB4, 0x18, 0x00, 0x50 }, // 5 - Superman Rabbid
        new byte[] { 0x0B, 0x6A, 0x00, 0xCA, 0x02, 0x6A, 0x00, 0xCA }, // 6 - Terminator Rabbid (Pink)
        new byte[] { 0x03, 0x1A, 0x00, 0x50, 0xF8, 0x19, 0x00, 0x50 }, // 7 - Sam Fisher Rabbid
        new byte[] { 0x9A, 0x0B, 0x00, 0x50, 0x92, 0x0B, 0x00, 0x50 }  // 8 - Nurgle Demon
    );

    public static Mod_RRR_BigFilePatch AddCustomHelicopterTexture => new Mod_RRR_BigFilePatch(0xCA007203, 0x50,
        new byte[] { 0x2D, 0x0E, 0x00, 0x56 }, // 0 - Original
        new byte[] { 0x01, 0x00, 0x00, 0xCC }  // 1 - Patched
    );

    private static byte[] FurModifier => new byte[] 
    {
        0x1A, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x20, 0x00, 0x00, 0x00, 0xBD, 0xFD, 0x2D, 0x3F,
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x05, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
    };

    public static Mod_RRR_BigFilePatch[] AddFurToRabbids => new Mod_RRR_BigFilePatch[] 
    {
        // 8F0107CE_S_Lapin.gao
        new Mod_RRR_BigFilePatch(0x8F0107CE, 0x1EB0,
            new byte[] { // 0 - Original
                0xFF, 0xFF, 0xFF, 0xFF, 0x0C, 0x00, 0x00, 0x00,
                0x53, 0x5F, 0x4C, 0x61, 0x70, 0x69, 0x6E, 0x2E, 0x67, 0x61, 0x6F, 0x00,
                0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
            },
            FurModifier // 1 - Patched
        ) { Sizes = new uint[] { 0x1EDC, 0x1F0C }, Mode = WriteMode.Insert },

        // 50000DE4_S_Lapin_01FurLOD001.gao
        new Mod_RRR_BigFilePatch(0x50000DE4, 0x1410,
            new byte[] { // 0 - Original
                0xFF, 0xFF, 0xFF, 0xFF, 0x18, 0x00, 0x00, 0x00,
                0x53, 0x5F, 0x4C, 0x61, 0x70, 0x69, 0x6E, 0x5F, 0x30, 0x31, 0x46, 0x75, 0x72, 0x4C, 0x4F, 0x44, 0x30, 0x30, 0x31, 0x2E, 0x67, 0x61, 0x6F, 0x00,
                0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            },
            FurModifier // 1 - Patched
        ) { Sizes = new uint[] { 0x1448, 0x1478 }, Mode = WriteMode.Insert },

        // 50002C85_S_Lapin_01FurLOD001Bis.gao
        new Mod_RRR_BigFilePatch(0x50002C85, 0x1410,
            new byte[] { // 0 - Original
                0xFF, 0xFF, 0xFF, 0xFF, 0x1B, 0x00, 0x00, 0x00,
                0x53, 0x5F, 0x4C, 0x61, 0x70, 0x69, 0x6E, 0x5F, 0x30, 0x31, 0x46, 0x75, 0x72, 0x4C, 0x4F, 0x44, 0x30, 0x30, 0x31, 0x42, 0x69, 0x73, 0x2E, 0x67, 0x61, 0x6F, 0x00,
                0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            },
            FurModifier // 1 - Patched
        ) { Sizes = new uint[] { 0x144B, 0x147B }, Mode = WriteMode.Insert },

        // 500012C6_S_Lapin_01FurLOD003.gao
        new Mod_RRR_BigFilePatch(0x500012C6, 0x570,
            new byte[] { // 0 - Original
                0xFF, 0xFF, 0xFF, 0xFF, 0x18, 0x00, 0x00, 0x00,
                0x53, 0x5F, 0x4C, 0x61, 0x70, 0x69, 0x6E, 0x5F, 0x30, 0x31, 0x46, 0x75, 0x72, 0x4C, 0x4F, 0x44, 0x30, 0x30, 0x33, 0x2E, 0x67, 0x61, 0x6F, 0x00,
                0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            },
            FurModifier // 1 - Patched
        ) { Sizes = new uint[] { 0x5A8, 0x5D8 }, Mode = WriteMode.Insert },

        // 50002C86_S_Lapin_01FurLOD003Bis.gao
        new Mod_RRR_BigFilePatch(0x50002C86, 0x570,
            new byte[] { // 0 - Original
                0xFF, 0xFF, 0xFF, 0xFF, 0x1B, 0x00, 0x00, 0x00,
                0x53, 0x5F, 0x4C, 0x61, 0x70, 0x69, 0x6E, 0x5F, 0x30, 0x31, 0x46, 0x75, 0x72, 0x4C, 0x4F, 0x44, 0x30, 0x30, 0x33, 0x42, 0x69, 0x73, 0x2E, 0x67, 0x61, 0x6F, 0x00,
                0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            },
            FurModifier // 1 - Patched
        ) { Sizes = new uint[] { 0x5AB, 0x5DB }, Mode = WriteMode.Insert },

        // 50001A9B_S_Lapin_Gris.gao
        new Mod_RRR_BigFilePatch(0x50001A9B, 0x1EC0,
            new byte[] { // 0 - Original
                0xFF, 0xFF, 0xFF, 0xFF, 0x11, 0x00, 0x00, 0x00,
                0x53, 0x5F, 0x4C, 0x61, 0x70, 0x69, 0x6E, 0x5F, 0x47, 0x72, 0x69, 0x73, 0x2E, 0x67, 0x61, 0x6F, 0x00,
                0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            },
            FurModifier // 1 - Patched
        ) { Sizes = new uint[] { 0x1EF1, 0x1F21 }, Mode = WriteMode.Insert },

        // 500029E6_S_LapinGris_LOD01.gao
        new Mod_RRR_BigFilePatch(0x500029E6, 0x1420,
            new byte[] { // 0 - Original
                0xFF, 0xFF, 0xFF, 0xFF, 0x16, 0x00, 0x00, 0x00,
                0x53, 0x5F, 0x4C, 0x61, 0x70, 0x69, 0x6E, 0x47, 0x72, 0x69, 0x73, 0x5F, 0x4C, 0x4F, 0x44, 0x30, 0x31, 0x2E, 0x67, 0x61, 0x6F, 0x00,
                0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            },
            FurModifier // 1 - Patched
        ) { Sizes = new uint[] { 0x1456, 0x1486 }, Mode = WriteMode.Insert },

        // 500029E8_S_LapinGris_LOD03.gao
        new Mod_RRR_BigFilePatch(0x500029E8, 0xB00,
            new byte[] { // 0 - Original
                0xFF, 0xFF, 0xFF, 0xFF, 0x16, 0x00, 0x00, 0x00,
                0x53, 0x5F, 0x4C, 0x61, 0x70, 0x69, 0x6E, 0x47, 0x72, 0x69, 0x73, 0x5F, 0x4C, 0x4F, 0x44, 0x30, 0x33, 0x2E, 0x67, 0x61, 0x6F, 0x00,
                0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            },
            FurModifier // 1 - Patched
        ) { Sizes = new uint[] { 0xB36, 0xB66 }, Mode = WriteMode.Insert },

        // 50001A9E.grm (Grey rabbid material)
        new Mod_RRR_BigFilePatch(0x50001A9E, 0x2C,
            new byte[] { // 0 - Original
                0x00, 0x00, 0x00, 0x00, 0x07, 0x04, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x80, 0x3F,
                0x66, 0x18, 0x00, 0xDB,
            },
            new byte[] { // 1 - Patched
                0x01, 0x00, 0x00, 0x00, 0x07, 0x04, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x80, 0x3F,
                0x66, 0x18, 0x00, 0xDB,
                0x00, 0x00, 0x00, 0x00, 0xAF, 0x04, 0x00, 0x3F,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x80, 0x3F,
                0x03, 0x00, 0x00, 0xCC,
            }
        ) { Sizes = new uint[] { 0x40, 0x54 } },

        // CA007247.grm (Rabbid material)
        new Mod_RRR_BigFilePatch(0xCA007247, 0x2C,
            new byte[] { // 0 - Original
                0x00, 0x00, 0x00, 0x00, 0x07, 0x04, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x80, 0x3F,
                0x4B, 0x72, 0x00, 0xCA,
            },
            new byte[] { // 1 - Patched
                0x01, 0x00, 0x00, 0x00, 0x07, 0x04, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x80, 0x3F,
                0x4B, 0x72, 0x00, 0xCA,
                0x00, 0x00, 0x00, 0x00, 0xAF, 0x04, 0x00, 0x3F,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x80, 0x3F,
                0x02, 0x00, 0x00, 0xCC,
            }
        ) { Sizes = new uint[] { 0x40, 0x54 } },
    };

    #endregion

    #region Patches: Other

    public static Mod_RRR_BigFilePatch[] ModdedRabbidItems => new Mod_RRR_BigFilePatch[] 
    {
        new Mod_RRR_BigFilePatch(0x0D009244, 0x24E,
            new byte[] { 0x17, 0x96, 0x00, 0x33 }, // 0 - Original
            new byte[] { 0xF4, 0x94, 0x00, 0x33 }  // 1 - Patched
        ),

        new Mod_RRR_BigFilePatch(0x0D00981A, 0x2A2,
            new byte[] { 0x17, 0x96, 0x00, 0x33 }, // 0 - Original
            new byte[] { 0x60, 0x02, 0x00, 0x67 }  // 1 - Patched
        ),
    };

    public static Mod_RRR_BigFilePatch EnableFlashlight => new Mod_RRR_BigFilePatch(0x0D008275, 0x8,
        new byte[] { 0x22, 0x2F }, // 0 - Original
        new byte[] { 0x2A, 0xFF }  // 1 - Patched
    );

    public static Mod_RRR_BigFilePatch[] AddFlashlightToMines => new Mod_RRR_BigFilePatch[] 
    {
        // Add flashlight to GameObjectList
        new Mod_RRR_BigFilePatch(0x17002D63, 0x1638,
            new byte[0], // 0 - Original
            new byte[] { 0x74, 0x82, 0x00, 0x0D, 0x2E, 0x67, 0x61, 0x6F, }  // 1 - Patched
        ) { Sizes = new uint[] { 0x1638, 0x1640 } },

        // Patch reference to vars to a new custom file
        new Mod_RRR_BigFilePatch(0x17002D6D, 0x4,
            new byte[] { 0x6C, 0x2D, 0x00, 0x17 }, // 0 - Original
            new byte[] { 0x04, 0x00, 0x00, 0xCC }  // 1 - Patched
        ),
    };

    #endregion
}