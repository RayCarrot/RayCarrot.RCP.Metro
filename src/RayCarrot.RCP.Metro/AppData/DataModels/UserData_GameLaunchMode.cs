namespace RayCarrot.RCP.Metro;

// TODO-14: Serialize as string rather than int

/// <summary>
/// The different game launch modes
/// </summary>
public enum UserData_GameLaunchMode
{
    /// <summary>
    /// The game launches as invoker
    /// </summary>
    AsInvoker,

    /// <summary>
    /// The game launches as invoker with an option to launch as administrator
    /// </summary>
    AsAdminOption,

    /// <summary>
    /// The game launches as administrator
    /// </summary>
    AsAdmin
}