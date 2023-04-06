namespace RayCarrot.RCP.Metro;

/// <summary>
/// Defines the platform a game is for
/// </summary>
public enum GamePlatform
{
    /// <summary>
    /// MS-DOS
    /// </summary>
    [GamePlatformInfo("MS-DOS", GamePlatformIconAsset.MsDos)]
    MsDos,

    /// <summary>
    /// Win32
    /// </summary>
    [GamePlatformInfo("Desktop (Win32)", GamePlatformIconAsset.Win32)]
    Win32,

    /// <summary>
    /// Windows package (.appx/.msix)
    /// </summary>
    [GamePlatformInfo("Windows package (.appx/.msix)", GamePlatformIconAsset.WindowsPackage)]
    WindowsPackage,

    /// <summary>
    /// Game Boy Advance
    /// </summary>
    [GamePlatformInfo("Game Boy Advance", GamePlatformIconAsset.Gba)]
    Gba,

    // TODO: Add console platforms such as PS1, GBA etc.
}