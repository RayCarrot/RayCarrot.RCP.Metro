using RayCarrot.RCP.Metro.Archive;

namespace RayCarrot.RCP.Metro.Games.Components;

[BaseGameComponent]
[GameFeature(nameof(Resources.Utilities_ArchiveExplorer_Header), GenericIconKind.GameFeature_Archive)]
public class ArchiveComponent : FactoryGameComponent<IArchiveDataManager>
{
    public ArchiveComponent(
        Func<GameInstallation, IArchiveDataManager> objFactory, 
        Func<GameInstallation, IEnumerable<string>> archivePathsFunc, 
        string id) 
        : base(objFactory)
    {
        _archivePathsFunc = archivePathsFunc;
        Id = id;
    }

    private readonly Func<GameInstallation, IEnumerable<string>> _archivePathsFunc;

    /// <summary>
    /// An ID to use to identify this type of archive
    /// </summary>
    public string Id { get; }

    public IEnumerable<string> GetArchiveFilePaths() => _archivePathsFunc(GameInstallation);

    public virtual AdditionalArchiveAction? GetAdditionalAction() => null;

    public record AdditionalArchiveAction(LocalizedString Header, LocalizedString Description, Func<GameInstallation, Task> Action);
}