using System.Diagnostics.CodeAnalysis;
using RayCarrot.RCP.Metro.Games.Structure;

namespace RayCarrot.RCP.Metro.Games.GameFileFinder;

public class GameFileFinderItem
{
    public GameFileFinderItem(GameDescriptor gameDescriptor, RomProgramInstallationStructure romStructure)
    {
        GameDescriptor = gameDescriptor;
        RomStructure = romStructure;
    }

    public GameDescriptor GameDescriptor { get; }
    public RomProgramInstallationStructure RomStructure { get; }

    /// <summary>
    /// The location. This is set if it has been found.
    /// </summary>
    public InstallLocation? FoundLocation { get; private set; }

    /// <summary>
    /// Indicates if this item has been found
    /// </summary>
    [MemberNotNullWhen(true, nameof(FoundLocation))]
    public bool HasBeenFound => FoundLocation != null;

    public void SetLocation(InstallLocation location) => FoundLocation = location;
}