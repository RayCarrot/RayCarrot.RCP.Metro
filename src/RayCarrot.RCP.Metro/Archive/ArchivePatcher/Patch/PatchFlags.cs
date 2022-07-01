using System;

namespace RayCarrot.RCP.Metro.Archive;

[Flags]
public enum PatchFlags
{
    None = 0,
    SyncTextures = 1 << 0,
}