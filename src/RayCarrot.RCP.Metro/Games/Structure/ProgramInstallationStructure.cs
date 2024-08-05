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

    public bool HasLayouts => Layouts.Count > 0;

    public virtual void RegisterComponents(IGameComponentBuilder builder) { }

    public virtual ProgramLayout? FindMatchingLayout(InstallLocation location) => null;

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
        const string cacheKey = "Layout";

        // First try getting from cache
        if (gameInstallation.TryGetCachedObject(cacheKey, out ProgramLayout? layout))
            return layout;

        // Find a matching layout
        layout = FindMatchingLayout(gameInstallation.InstallLocation);

        // Cache if not null
        if (layout != null)
            gameInstallation.CacheObject(cacheKey, layout);

        return layout;
    }
}