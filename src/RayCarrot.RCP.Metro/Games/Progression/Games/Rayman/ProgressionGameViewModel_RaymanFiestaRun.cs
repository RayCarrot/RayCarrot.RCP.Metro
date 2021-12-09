namespace RayCarrot.RCP.Metro;

public class ProgressionGameViewModel_RaymanFiestaRun : ProgressionGameViewModel
{
    public ProgressionGameViewModel_RaymanFiestaRun(UserData_FiestaRunEdition edition, string displayName) : base(Games.RaymanFiestaRun, displayName)
    {
        Edition = edition;
    }

    public UserData_FiestaRunEdition Edition { get; }

    protected override string BackupName => $"Rayman Fiesta Run ({Edition})";
    protected override GameBackups_Directory[] BackupDirectories => GameManager_WinStore.GetWinStoreBackupDirs(Game.GetManager<GameManager_RaymanFiestaRun_WinStore>(GameType.WinStore).GetFiestaRunFullPackageName(Edition));
}