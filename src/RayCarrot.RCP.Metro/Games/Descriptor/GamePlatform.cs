namespace RayCarrot.RCP.Metro;

/// <summary>
/// Defines the platform a game is for
/// </summary>
public enum GamePlatform
{
    /// <summary>
    /// MS-DOS
    /// </summary>
    MSDOS = GamePlatformFlag.Plat_MSDOS | GamePlatformFlag.PC,

    /// <summary>
    /// Win32
    /// </summary>
    Win32 = GamePlatformFlag.Plat_Win32 | GamePlatformFlag.PC,

    /// <summary>
    /// Steam
    /// </summary>
    Steam = GamePlatformFlag.Plat_Steam | GamePlatformFlag.PC,

    /// <summary>
    /// Windows package (.appx/.msix)
    /// </summary>
    WindowsPackage = GamePlatformFlag.Plat_WindowsPackage | GamePlatformFlag.PC,

    // TODO: Add console platforms such as PS1, GBA etc.
}