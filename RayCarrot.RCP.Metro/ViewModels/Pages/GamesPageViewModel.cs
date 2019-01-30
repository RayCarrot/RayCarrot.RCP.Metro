using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Nito.AsyncEx;
using RayCarrot.CarrotFramework;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// View model for the games page
    /// </summary>
    public class GamesPageViewModel : BaseViewModel
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public GamesPageViewModel()
        {
            AsyncLock = new AsyncLock();
            InstalledGames = new ObservableCollection<GameDisplayViewModel>();
            NotInstalledGames = new ObservableCollection<GameDisplayViewModel>();

            RCFRCP.App.RefreshRequired += async (s, e) => await RefreshAsync();
            RCF.Data.UserLevelChanged += async (s, e) => await RefreshAsync();
        }

        #endregion

        #region Private Properties

        /// <summary>
        /// The async lock to use for the game page
        /// </summary>
        private AsyncLock AsyncLock { get; }

        #endregion

        #region Public Properties

        /// <summary>
        /// The installed games
        /// </summary>
        public ObservableCollection<GameDisplayViewModel> InstalledGames { get; }

        /// <summary>
        /// The not installed games
        /// </summary>
        public ObservableCollection<GameDisplayViewModel> NotInstalledGames { get; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Refreshes the games
        /// </summary>
        /// <returns>The task</returns>
        public async Task RefreshAsync()
        {
            RCF.Logger.LogInformationSource($"The displayed games are being refreshed...");

            using (await AsyncLock.LockAsync())
            {
                InstalledGames.Clear();
                NotInstalledGames.Clear();

                // Enumerate each game
                foreach (Games game in RCFRCP.App.GetGames)
                {
                    // Check if it has been added
                    if (!game.IsAdded())
                    {
                        NotInstalledGames.Add(game.GetDisplayViewModel());
                        continue;
                    }

                    // Get the game info
                    var info = game.GetInfo();

                    // Check if it's valid
                    if (!game.IsValid(info.GameType, info.InstallDirectory))
                    {
                        // Show message
                        await RCF.MessageUI.DisplayMessageAsync($"The game {game.GetDisplayName()} was not found", "Unable to find game", MessageType.Error);

                        // Remove the game from app data
                        RCFRCP.Data.Games.Remove(game);

                        NotInstalledGames.Add(game.GetDisplayViewModel());

                        RCF.Logger.LogInformationSource($"The game {game} has been removed due to not being valid");

                        continue;
                    }

                    // Add the game to the collection
                    InstalledGames.Add(game.GetDisplayViewModel());
                }
            }

            RCF.Logger.LogInformationSource($"The displayed games have been refreshed");
        }

        #endregion
    }
}