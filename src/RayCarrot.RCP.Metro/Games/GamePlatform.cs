namespace RayCarrot.RCP.Metro;

/// <summary>
/// Defines the platform a game is for
/// </summary>
public enum GamePlatform
{
    /// <summary>
    /// MS-DOS
    /// </summary>
    [GamePlatformInfo(nameof(Resources.Platform_MsDos), GamePlatformIconAsset.MsDos)]
    MsDos,

    /// <summary>
    /// Win32
    /// </summary>
    [GamePlatformInfo(nameof(Resources.Platform_Win32), GamePlatformIconAsset.Win32)]
    Win32,

    /// <summary>
    /// Windows package (.appx/.msix)
    /// </summary>
    [GamePlatformInfo(nameof(Resources.Platform_WindowsPackage), GamePlatformIconAsset.WindowsPackage)]
    WindowsPackage,

    /// <summary>
    /// PlayStation
    /// </summary>
    [GamePlatformInfo(nameof(Resources.Platform_Ps1), GamePlatformIconAsset.Ps1)]
    Ps1,

    /// <summary>
    /// Game Boy Color
    /// </summary>
    [GamePlatformInfo(nameof(Resources.Platform_Gbc), GamePlatformIconAsset.Gbc)]
    Gbc,

    /// <summary>
    /// Game Boy Advance
    /// </summary>
    [GamePlatformInfo(nameof(Resources.Platform_Gba), GamePlatformIconAsset.Gba)]
    Gba,
}