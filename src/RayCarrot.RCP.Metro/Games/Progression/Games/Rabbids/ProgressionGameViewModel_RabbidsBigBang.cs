namespace RayCarrot.RCP.Metro;

public class ProgressionGameViewModel_RabbidsBigBang : ProgressionGameViewModel
{
    public ProgressionGameViewModel_RabbidsBigBang() : base(Games.RabbidsBigBang) { }

    protected override GameBackups_Directory[] BackupDirectories => GameManager_WinStore.GetWinStoreBackupDirs(Game.GetManager<GameManager_WinStore>().FullPackageName);
}