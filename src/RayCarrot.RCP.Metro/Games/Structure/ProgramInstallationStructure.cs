namespace RayCarrot.RCP.Metro.Games.Structure;

public abstract class ProgramInstallationStructure
{
    /// <summary>
    /// Indicates if the structure allows the game to be patched using the game patcher
    /// </summary>
    public abstract bool AllowPatching { get; }

    /// <summary>
    /// Indicates if the location has a valid installation structure
    /// </summary>
    /// <param name="location">The location to check</param>
    /// <returns>The validation result</returns>
    public abstract GameLocationValidationResult IsLocationValid(InstallLocation location);
}