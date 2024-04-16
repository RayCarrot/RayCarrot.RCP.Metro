using RayCarrot.RCP.Metro.Games.Components;

namespace RayCarrot.RCP.Metro.Games.Structure;

public abstract class ProgramInstallationStructure
{
    protected ProgramInstallationStructure() : this(Array.Empty<ProgramLayout>()) { }
    protected ProgramInstallationStructure(IReadOnlyList<ProgramLayout> layouts)
    {
        Layouts = layouts;
    }

    protected IReadOnlyList<ProgramLayout> Layouts { get; }

    public virtual void RegisterComponents(IGameComponentBuilder builder) { }

    protected virtual ProgramLayout? FindMatchingLayout(InstallLocation location) => null;

    /// <summary>
    /// Indicates if the location has a valid installation structure
    /// </summary>
    /// <param name="location">The location to check</param>
    /// <returns>The validation result</returns>
    public abstract GameLocationValidationResult IsLocationValid(InstallLocation location);

    /// <summary>
    /// Gets the program layout for the specified game installation, or null if
    /// no matching layout was found or if this structure has none defined
    /// </summary>
    /// <param name="gameInstallation">The game installation to get the layout for</param>
    /// <returns>The layout, or null if not found</returns>
    public ProgramLayout? GetLayout(GameInstallation gameInstallation)
    {
        // TODO: Might want to cache this to avoid having to find the matching layout each time it's
        //       requested. Might be better only caching during runtime rather than in app data though
        //       since we might make changes to the layouts in future updates, or add new ones, which
        //       might cause issues.
        return FindMatchingLayout(gameInstallation.InstallLocation);
    }
}