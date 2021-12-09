using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using NLog;
using RayCarrot.IO;

namespace RayCarrot.RCP.Metro;

public abstract class ProgressionGameViewModel : BaseViewModel
{
    protected ProgressionGameViewModel(Games game, string? displayName = null)
    {
        // Get the info
        GameInfo gameInfo = game.GetGameInfo();

        Game = game;
        IconSource = gameInfo.IconSource;
        IsDemo = gameInfo.IsDemo;
        DisplayName = displayName ?? gameInfo.DisplayName;
        InstallDir = game.GetInstallDir(false);
        
        Slots = new ObservableCollection<ProgressionSlotViewModel>();
    }

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    protected FileSystemPath InstallDir { get; }

    public Games Game { get; }
    public string IconSource { get; }
    public bool IsDemo { get; }
    public string DisplayName { get; }
    public bool IsLoading { get; set; }
    public bool IsExpanded { get; set; }

    protected virtual string BackupName => Game.GetGameInfo().BackupName;
    protected virtual GameBackups_Directory[] BackupDirectories => Array.Empty<GameBackups_Directory>();

    public ObservableCollection<ProgressionSlotViewModel> Slots { get; }
    public ProgressionSlotViewModel? PrimarySlot { get; private set; }

    protected virtual Task LoadSlotsAsync() => Task.CompletedTask;

    public async Task LoadAsync()
    {
        if (IsLoading)
            return;

        IsLoading = true;

        try
        {
            Slots.Clear();

            await LoadSlotsAsync();

            PrimarySlot = Slots.OrderBy(x => x.Percentage).LastOrDefault();
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to load progression for {0} ({1})", Game, DisplayName);

            Slots.Clear();
            PrimarySlot = null;
        }
        finally
        {
            IsLoading = false;
        }
    }
}