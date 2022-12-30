namespace RayCarrot.RCP.Metro;

/// <summary>
/// Defines the platform a game is for
/// </summary>
public enum GamePlatform
{
    /// <summary>
    /// MS-DOS
    /// </summary>
    [GamePlatformInfo("MS-DOS", GamePlatformIconAsset.MsDos, requiresEmulator: true)]
    MsDos,

    /// <summary>
    /// Win32
    /// </summary>
    [GamePlatformInfo("Desktop (Win32)", GamePlatformIconAsset.Win32, requiresEmulator: false)]
    Win32,

    /// <summary>
    /// Windows package (.appx/.msix)
    /// </summary>
    [GamePlatformInfo("Windows package (.appx/.msix)", GamePlatformIconAsset.WindowsPackage, requiresEmulator: false)]
    WindowsPackage,

    // TODO: Add console platforms such as PS1, GBA etc.
}