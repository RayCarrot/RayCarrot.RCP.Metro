using System;

namespace RayCarrot.RCP.Metro;

[Flags]
public enum GamePlatformFlag
{
    Plat_MsDos = 1 << 0,
    Plat_Win32 = 1 << 1,
    Plat_Steam = 1 << 2,
    Plat_WindowsPackage = 1 << 3,
    
    PC = 1 << 24,
}