namespace RayCarrot.RCP.Metro;

/// <summary>
/// Defines the platform a game is for. The <see cref="PlatformManager"/> is then responsible for managing it.
/// </summary>
public enum GamePlatform
{
    /// <summary>
    /// MS-DOS
    /// </summary>
    MSDOS,

    /// <summary>
    /// Win32
    /// </summary>
    Win32,

    /// <summary>
    /// Steam
    /// </summary>
    Steam,

    /// <summary>
    /// Windows package (.appx/.msix)
    /// </summary>
    WindowsPackage,

    // TODO: Add console platforms such as PS1, GBA etc.
}