#nullable disable
namespace RayCarrot.RCP.Metro;

/// <summary>
/// The available modes for the admin worker
/// </summary>
public enum AdminWorkerMode
{
    /// <summary>
    /// Grants full control to the specified file
    /// </summary>
    GrantFullControl,

    /// <summary>
    /// Restarts the Rayman Control Panel as administrator
    /// </summary>
    RestartAsAdmin,

    /// <summary>
    /// Restarts the Rayman Control Panel with the specified arguments
    /// </summary>
    RestartWithArgs,
}