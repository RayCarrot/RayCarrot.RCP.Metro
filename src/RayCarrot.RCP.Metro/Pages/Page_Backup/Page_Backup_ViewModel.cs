#nullable disable
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using Nito.AsyncEx;
using NLog;
using RayCarrot.UI;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// View model for the backup page
/// </summary>
public class Page_Backup_ViewModel : BaseRCPViewModel
{
    #region Constructor

    /// <summary>
    /// Default constructor
    /// </summary>
    public Page_Backup_ViewModel()
    {
        // Create commands
        RefreshCommand = new AsyncRelayCommand(async () => await Task.Run(async () => await RefreshAsync()));
        BackupAllCommand = new AsyncRelayCommand(BackupAllAsync);

        // Create properties
        AsyncLock = new AsyncLock();
        GameBackupItems = new ObservableCollection<Page_Backup_ItemViewModel>();

        // Enable collection synchronization
        BindingOperations.EnableCollectionSynchronization(GameBackupItems, Application.Current);

        // Refresh on app refresh
        App.RefreshRequired += async (s, e) =>
        {
            if (e.BackupsModified || e.GameCollectionModified)
                await Task.Run(async () => await RefreshAsync());
        };

        // Refresh on culture changed
        Services.InstanceData.CultureChanged += async (s, e) => await Task.Run(async () => await RefreshAsync());

        // Refresh on startup
        Metro.App.Current.StartupComplete += async (s, e) => await RefreshAsync();
    }

    #endregion

    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Private Properties

    /// <summary>
    /// The async lock for refreshing the backup items
    /// </summary>
    private AsyncLock AsyncLock { get; }

    #endregion

    #region Public Properties   

    /// <summary>
    /// The game backup items
    /// </summary>
    public ObservableCollection<Page_Backup_ItemViewModel> GameBackupItems { get; }

    #endregion

    #region Commands

    public ICommand RefreshCommand { get; }

    public ICommand BackupAllCommand { get; }

    #endregion

    #region Public Methods

    /// <summary>
    /// Refreshes the backup items
    /// </summary>
    /// <returns></returns>
    public async Task RefreshAsync()
    {
        using (await AsyncLock.LockAsync())
        {
            try
            {
                // Clear current items
                GameBackupItems.Clear();

                // Enumerate the saved games
                foreach (Games game in App.GetGames.Where(x => x.IsAdded()))
                {
                    // Enumerate the backup info
                    foreach (IGameBackups_BackupInfo info in game.GetGameInfo().GetBackupInfos)
                    {
                        // Refresh the info
                        await info.RefreshAsync();

                        // Create the backup item
                        var backupItem = new Page_Backup_ItemViewModel(game, info);

                        // Add the item
                        GameBackupItems.Add(backupItem);

                        // Refresh the item
                        await backupItem.RefreshAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex, "Refreshing backups");
                throw;
            }
        }
    }

    /// <summary>
    /// Performs a backup on all games
    /// </summary>
    /// <returns>The task</returns>
    public async Task BackupAllAsync()
    {
        // Make sure no backups are running
        if (GameBackupItems.Any(x => x.PerformingBackupRestore))
            return;

        try
        {
            GameBackupItems.ForEach(x => x.PerformingBackupRestore = true);

            // Confirm backup
            if (!await Services.MessageUI.DisplayMessageAsync(Resources.Backup_ConfirmBackupAll, Resources.Backup_ConfirmBackupAllHeader, MessageType.Warning, true))
            {
                Logger.Info("Backup canceled");

                return;
            }

            int completed = 0;

            // Perform the backups
            await Task.Run(async () =>
            {
                foreach (Page_Backup_ItemViewModel game in GameBackupItems)
                {
                    game.ShowBackupRestoreIndicator = true;

                    if (await Services.Backup.BackupAsync(game.BackupInfo))
                        completed++;

                    game.ShowBackupRestoreIndicator = false;
                }
            });

            if (completed == GameBackupItems.Count)
                await Services.MessageUI.DisplaySuccessfulActionMessageAsync(Resources.Backup_BackupAllSuccess);
            else
                await Services.MessageUI.DisplayMessageAsync(String.Format(Resources.Backup_BackupAllFailed, completed, GameBackupItems.Count), Resources.Backup_BackupAllFailedHeader, MessageType.Information);

            // Refresh the item
            await Task.Run(async () => await RefreshAsync());
        }
        finally
        {
            foreach (var x in GameBackupItems)
            {
                x.PerformingBackupRestore = false;
                x.ShowBackupRestoreIndicator= false;
            }
        }
    }

    #endregion
}