namespace RayCarrot.RCP.Metro;

/// <summary>
/// Defines files added for a game installation. These should be removed
/// when the game is removed from the Rayman Control Panel.
/// </summary>
public class AddedGameFiles
{
    public AddedGameFiles() : this(null) { }
    public AddedGameFiles(HashSet<FileSystemPath>? files)
    {
        Files = files ?? new HashSet<FileSystemPath>();
    }

    public HashSet<FileSystemPath> Files { get; }
}