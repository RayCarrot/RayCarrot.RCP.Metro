using RayCarrot.RCP.Metro.ModLoader.Resource;

namespace RayCarrot.RCP.Metro.ModLoader.Modules;

/// <summary>
/// A module for a mod. Each mod consists of multiple modules, each in their own folder. Each module
/// provides its own implementation for how the game should be modified upon applying the mod.
/// </summary>
public abstract class ModModule
{
    public abstract string Id { get; }
    public abstract LocalizedString Description { get; }

    public virtual ModModuleViewModel GetViewModel() => new(this);

    public virtual IReadOnlyCollection<IModFileResource> GetAddedFiles(Mod mod, FileSystemPath modulePath) =>
        new ReadOnlyCollection<IModFileResource>(new List<IModFileResource>());
    public virtual IReadOnlyCollection<ModFilePath> GetRemovedFiles(Mod mod, FileSystemPath modulePath) =>
        new ReadOnlyCollection<ModFilePath>(new List<ModFilePath>());
    public virtual IReadOnlyCollection<IFilePatch> GetPatchedFiles(Mod mod, FileSystemPath modulePath) =>
        new ReadOnlyCollection<IFilePatch>(new List<IFilePatch>());
}