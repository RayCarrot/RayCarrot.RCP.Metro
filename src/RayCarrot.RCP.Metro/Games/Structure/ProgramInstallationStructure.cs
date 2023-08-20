namespace RayCarrot.RCP.Metro.Games.Structure;

public abstract class ProgramInstallationStructure
{
    /// <summary>
    /// Indicates if the structure allows mods to be installed for the game
    /// </summary>
    public abstract bool SupportsMods { get; }

    /// <summary>
    /// Indicates if the location has a valid installation structure
    /// </summary>
    /// <param name="location">The location to check</param>
    /// <returns>The validation result</returns>
    public abstract GameLocationValidationResult IsLocationValid(InstallLocation location);
}