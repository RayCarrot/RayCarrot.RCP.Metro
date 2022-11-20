namespace RayCarrot.RCP.Metro;

/// <summary>
/// Defines the platform a game is for
/// </summary>
public enum GamePlatform
{
    /// <summary>
    /// MS-DOS
    /// </summary>
    [GamePlatformInfo("MS-DOS", GamePlatformIconAsset.MSDOS)]
    MSDOS = GamePlatformFlag.Plat_MSDOS | GamePlatformFlag.PC,

    /// <summary>
    /// Win32
    /// </summary>
    [GamePlatformInfo("Windows (Win32)", GamePlatformIconAsset.Win32)]
    Win32 = GamePlatformFlag.Plat_Win32 | GamePlatformFlag.PC,

    /// <summary>
    /// Steam
    /// </summary>
    [GamePlatformInfo("Steam", GamePlatformIconAsset.Steam)]
    Steam = GamePlatformFlag.Plat_Steam | GamePlatformFlag.PC,

    /// <summary>
    /// Windows package (.appx/.msix)
    /// </summary>
    [GamePlatformInfo("Windows Package (.appx/.msix)", GamePlatformIconAsset.WindowsPackage)]
    WindowsPackage = GamePlatformFlag.Plat_WindowsPackage | GamePlatformFlag.PC,

    // TODO: Add console platforms such as PS1, GBA etc.
}