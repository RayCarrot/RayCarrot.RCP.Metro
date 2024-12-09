namespace RayCarrot.RCP.Metro.Archive.UbiArt;

public class UbiArtArchiveRepackResult : ArchiveRepackResult
{
    public UbiArtArchiveRepackResult(bool addedOrRemovedFiles)
    {
        AddedOrRemovedFiles = addedOrRemovedFiles;
    }

    public bool AddedOrRemovedFiles { get; }
}