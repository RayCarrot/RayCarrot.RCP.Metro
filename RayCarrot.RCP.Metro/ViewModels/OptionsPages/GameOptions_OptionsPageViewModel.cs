using RayCarrot.Common;
using RayCarrot.UI;
using RayCarrot.WPF;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows.Data;

namespace RayCarrot.RCP.Metro
{
    public class GameOptions_OptionsPageViewModel : GameOptions_BasePageViewModel
    {
        #region Constructor

        public GameOptions_OptionsPageViewModel(Games game) 
            : base(new LocalizedString(() => Resources.GameOptions_Options), GenericIconKind.GameOptions_General)
        {
            // Get the info
            var gameInfo = game.GetGameInfo();

            // Set properties
            Game = game;
            GameInfoItems = new ObservableCollection<DuoGridItemViewModel>();
            OptionsContent = gameInfo.OptionsUI;

            // Check if the launch mode can be changed
            CanChangeLaunchMode = Game.GetManager().SupportsGameLaunchMode;

            // Enable collection synchronization
            BindingOperations.EnableCollectionSynchronization(GameInfoItems, this);

            // Refresh the game info
            RefreshGameInfo();

            // Refresh the game data on certain events
            App.RefreshRequired += App_RefreshRequiredAsync;
            Services.Data.CultureChanged += Data_CultureChanged;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// The game
        /// </summary>
        public Games Game { get; }

        /// <summary>
        /// The game info items
        /// </summary>
        public ObservableCollection<DuoGridItemViewModel> GameInfoItems { get; }

        /// <summary>
        /// The game options content
        /// </summary>
        public object OptionsContent { get; }

        /// <summary>
        /// Indicates if the launch mode can be changed
        /// </summary>
        public bool CanChangeLaunchMode { get; }

        public bool HasOptionsOrCanChangeLaunchMode => CanChangeLaunchMode || OptionsContent != null;

        /// <summary>
        /// The game data
        /// </summary>
        public UserData_GameData GameData => RCPServices.Data.Games.TryGetValue(Game);

        /// <summary>
        /// The game's launch mode
        /// </summary>
        public UserData_GameLaunchMode LaunchMode
        {
            get => GameData.LaunchMode;
            set
            {
                GameData.LaunchMode = value;
                _ = App.OnRefreshRequiredAsync(new RefreshRequiredEventArgs(Game, false, true, false, false));
            }
        }

        #endregion

        #region Event Handlers

        private Task App_RefreshRequiredAsync(object sender, RefreshRequiredEventArgs e)
        {
            if (e.GameInfoModified)
                // Refresh the game info
                RefreshGameInfo();

            return Task.CompletedTask;
        }

        private void Data_CultureChanged(object sender, PropertyChangedEventArgs<CultureInfo> e)
        {
            RefreshGameInfo();
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Refreshes the game info
        /// </summary>
        protected void RefreshGameInfo()
        {
            GameInfoItems.Clear();
            GameInfoItems.AddRange(Game.GetManager().GetGameInfoItems);
        }

        protected override object GetPageUI() => new GameOptions_UI()
        {
            DataContext = this
        };

        public override void Dispose()
        {
            // Dispose base
            base.Dispose();

            // Unsubscribe events
            App.RefreshRequired -= App_RefreshRequiredAsync;
            Services.Data.CultureChanged -= Data_CultureChanged;

            // Disable collection synchronization
            BindingOperations.DisableCollectionSynchronization(GameInfoItems);
        }

        #endregion
    }
}