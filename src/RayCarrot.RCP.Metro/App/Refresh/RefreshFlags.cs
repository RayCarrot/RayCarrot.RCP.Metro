using System;

namespace RayCarrot.RCP.Metro;

[Flags]
public enum RefreshFlags
{
    None = 0,

    GameCollection = 1 << 0,
    LaunchInfo = 1 << 1,
    Backups = 1 << 2,
    GameInfo = 1 << 3,
    JumpList = 1 << 4,

    All = GameCollection | LaunchInfo | Backups | GameInfo | JumpList,
}