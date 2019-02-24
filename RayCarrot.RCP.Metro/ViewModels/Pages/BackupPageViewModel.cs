using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using Nito.AsyncEx;
using RayCarrot.CarrotFramework;

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
            RefresCommand = new AsyncRelayCommand(RefreshAsync);

            AsyncLock = new AsyncLock();
            GameBackupItems = new ObservableCollection<GameBackupItemViewModel>();

            BindingOperations.EnableCollectionSynchronization(GameBackupItems, Application.Current);

            App.RefreshRequired += async (s, e) => await RefreshAsync();
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
                    foreach (Games game in Data.Games.Keys)
                    {
                        // Create the backup item
                        var backupItem = new GameBackupItemViewModel(game);

                        // Add the item
                        GameBackupItems.Add(backupItem);

                        // Refresh the item
                        await backupItem.RefreshAsync();
                    }
                });
            }
        }

        #endregion
    }
}