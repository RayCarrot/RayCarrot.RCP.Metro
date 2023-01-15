namespace RayCarrot.RCP.Metro.Games.Clients.Data;

public class DosBoxConfigFilePaths
{
    public DosBoxConfigFilePaths() : this(null) { }
    public DosBoxConfigFilePaths(List<FileSystemPath>? filePaths)
    {
        FilePaths = filePaths ?? new List<FileSystemPath>();
    }

    public List<FileSystemPath> FilePaths { get; }
}