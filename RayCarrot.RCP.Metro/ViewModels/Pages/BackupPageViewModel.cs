using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using Nito.AsyncEx;
using RayCarrot.CarrotFramework.Abstractions;
using RayCarrot.Extensions;
using RayCarrot.UI;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// View model for the backup page
    /// </summary>
    public class BackupPageViewModel : BaseRCPViewModel
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public BackupPageViewModel()
        {
            // Create commands
            RefresCommand = new AsyncRelayCommand(RefreshAsync);
            BackupAllCommand = new AsyncRelayCommand(BackupAllAsync);

            // Create properties
            AsyncLock = new AsyncLock();
            GameBackupItems = new ObservableCollection<GameBackupItemViewModel>();

            // Enable collection synchronization
            BindingOperations.EnableCollectionSynchronization(GameBackupItems, Application.Current);

            // Refresh on app refresh
            App.RefreshRequired += async (s, e) =>
            {
                if (e.BackupsModified || e.GameCollectionModified)
                    await RefreshAsync();
            };

            // Refresh on culture changed
            RCFCore.Data.CultureChanged += async (s, e) => await RefreshAsync();

            // Refresh on startup
            Metro.App.Current.StartupComplete += async (s, e) => await RefreshAsync();
        }

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
        public ObservableCollection<GameBackupItemViewModel> GameBackupItems { get; }

        #endregion

        #region Commands

        public ICommand RefresCommand { get; }

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
                await Task.Run(async () =>
                {
                    // Clear current items
                    GameBackupItems.Clear();

                    // Enumerate the saved games
                    foreach (Games game in App.GetGames.Where(x => x.IsAdded()))
                    {
                        // Enumerate the backup info
                        foreach (IBackupInfo info in await game.GetGameManager().GetBackupInfosAsync())
                        {
                            // Create the backup item
                            var backupItem = new GameBackupItemViewModel(game, info);

                            // Add the item
                            GameBackupItems.Add(backupItem);

                            // Refresh the item
                            await backupItem.RefreshAsync();
                        }
                    }
                });
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
                if (!await RCFUI.MessageUI.DisplayMessageAsync(Resources.Backup_ConfirmBackupAll, Resources.Backup_ConfirmBackupAllHeader, MessageType.Warning, true))
                {
                    RCFCore.Logger?.LogInformationSource($"Backup canceled");

                    return;
                }

                int completed = 0;

                // Perform the backups
                await Task.Run(async () =>
                {
                    foreach (GameBackupItemViewModel game in GameBackupItems)
                    {
                        game.ShowBackupRestoreIndicator = true;

                        if (await RCFRCP.Backup.BackupAsync(game.BackupInfo))
                            completed++;

                        game.ShowBackupRestoreIndicator = false;
                    }
                });

                if (completed == GameBackupItems.Count)
                    await RCFUI.MessageUI.DisplaySuccessfulActionMessageAsync(Resources.Backup_BackupAllSuccess);
                else
                    await RCFUI.MessageUI.DisplayMessageAsync(String.Format(Resources.Backup_BackupAllFailed, completed, GameBackupItems.Count), Resources.Backup_BackupAllFailedHeader, MessageType.Information);

                // Refresh the item
                await RefreshAsync();
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
}