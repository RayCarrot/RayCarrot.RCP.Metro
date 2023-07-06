using RayCarrot.RCP.Metro.Games.Structure;

namespace RayCarrot.RCP.Metro.Games.GameFileFinder;

public class GameFileFinderItem
{
    public GameFileFinderItem(GameDescriptor gameDescriptor, RomProgramInstallationStructure romStructure)
    {
        GameDescriptor = gameDescriptor;
        RomStructure = romStructure;

        _foundLocations = new List<InstallLocation>();
    }

    private readonly List<InstallLocation> _foundLocations;

    public GameDescriptor GameDescriptor { get; }
    public RomProgramInstallationStructure RomStructure { get; }
    public bool HasBeenFound => _foundLocations.Any();

    public IReadOnlyList<InstallLocation> GetFoundLocations() => _foundLocations.AsReadOnly();
    public void AddLocation(InstallLocation location) => _foundLocations.Add(location);
}