using RayCarrot.RCP.Metro.Games.Components;

namespace RayCarrot.RCP.Metro.Games.Structure;

public abstract class ProgramInstallationStructure
{
    protected ProgramInstallationStructure(IReadOnlyCollection<ProgramLayout>? layouts)
    {
        Layouts = layouts ?? new[] { new ProgramLayout(String.Empty) };
    }

    public IReadOnlyCollection<ProgramLayout> Layouts { get; }

    public virtual void RegisterComponents(IGameComponentBuilder builder) { }

    /// <summary>
    /// Indicates if the location has a valid installation structure
    /// </summary>
    /// <param name="location">The location to check</param>
    /// <returns>The validation result</returns>
    public abstract GameLocationValidationResult IsLocationValid(InstallLocation location);
}