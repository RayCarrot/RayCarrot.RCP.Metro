using RayCarrot.RCP.Metro.ModLoader.Resource;

namespace RayCarrot.RCP.Metro.ModLoader.Modules;

/// <summary>
/// A module for a mod. Each mod consists of multiple modules, each in their own folder. Each module
/// provides its own implementation for how the game should be modified upon applying the mod.
/// </summary>
public abstract class ModModule
{
    public abstract string Id { get; }

    public abstract IReadOnlyCollection<IModFileResource> GetAddedFiles(Mod mod, FileSystemPath modulePath);
    public abstract IReadOnlyCollection<ModFilePath> GetRemovedFiles(Mod mod, FileSystemPath modulePath);
}