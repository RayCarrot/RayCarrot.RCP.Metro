namespace RayCarrot.RCP.Metro;

// TODO-14: Serialize as string rather than int

/// <summary>
/// The game types supported by the Rayman Control Panel
/// </summary>
public enum GameType
{
    /// <summary>
    /// A Win32 game
    /// </summary>
    Win32,

    /// <summary>
    /// A Steam game
    /// </summary>
    Steam,

    /// <summary>
    /// A Windows store game
    /// </summary>
    WinStore,

    /// <summary>
    /// A DOSBox game
    /// </summary>
    DosBox,

    /// <summary>
    /// An education DOSBox game
    /// </summary>
    EducationalDosBox,
}

// TODO-14: We want to change how this is handled. Some ideas:
// 
// 
// GameType: Win32, Steam, Package, Emulated
// 
// GamePlatform: MS_DOS, Win32, Steam, Package, PS1, PS2 etc. (where each enum has a list of available manager types or something)