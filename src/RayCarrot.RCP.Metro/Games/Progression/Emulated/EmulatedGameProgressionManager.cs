using RayCarrot.RCP.Metro.Games.Components;

namespace RayCarrot.RCP.Metro;

public abstract class EmulatedGameProgressionManager : GameProgressionManager
{
    protected EmulatedGameProgressionManager(GameInstallation gameInstallation, string progressionId) 
        : base(gameInstallation, progressionId) 
    { }

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    // For now we don't allow backups for emulated games as it doesn't work very well with the current backup system
    public override GameBackups_Directory[]? BackupDirectories => null;

    public override bool RefreshOnRebuiltComponents => true;

    public override async IAsyncEnumerable<GameProgressionSlot> LoadSlotsAsync(FileSystemWrapper fileSystem)
    {
        Logger.Info("{0} save is being loaded...", GameInstallation.FullId);

        foreach (EmulatedSaveFile emulatedSaveFile in GameInstallation.GetComponents<EmulatedSaveFilesComponent>().CreateManyObjects())
        {
            foreach (EmulatedSave emulatedSave in await emulatedSaveFile.GetSavesAsync(GameInstallation))
            {
                using (emulatedSave)
                    await foreach (EmulatedGameProgressionSlot slot in LoadSlotsAsync(emulatedSave)) 
                        yield return slot;
            }
        }
    }

    public abstract IAsyncEnumerable<EmulatedGameProgressionSlot> LoadSlotsAsync(EmulatedSave emulatedSave);
}