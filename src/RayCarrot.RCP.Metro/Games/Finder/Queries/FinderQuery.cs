namespace RayCarrot.RCP.Metro.Games.Finder;

public abstract class FinderQuery
{
    public Func<FileSystemPath, FileSystemPath>? ValidateLocationFunc { get; init; }
}