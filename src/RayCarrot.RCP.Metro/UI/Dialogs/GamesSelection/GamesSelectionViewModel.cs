namespace RayCarrot.RCP.Metro;

/// <summary>
/// View model for a games selection dialog
/// </summary>
public class GamesSelectionViewModel : UserInputViewModel
{
    public GamesSelectionViewModel(GamesManager gamesManager)
    {
        Games = new ObservableCollection<GameViewModel>(gamesManager.GetInstalledGames().Select(x => new GameViewModel(this, x)));
    }

    public ObservableCollection<GameViewModel> Games { get; }
    public IEnumerable<GameViewModel> SelectedGames => Games.Where(x => x.IsSelected);
    public bool MultiSelection { get; set; }
    public bool HasSelection { get; private set; }

    public class GameViewModel : BaseViewModel
    {
        public GameViewModel(GamesSelectionViewModel parentViewModel, GameInstallation gameInstallation)
        {
            ParentViewModel = parentViewModel;
            GameInstallation = gameInstallation;
        }

        private bool _isSelected;

        public GamesSelectionViewModel ParentViewModel { get; }
        public GameInstallation GameInstallation { get; }
        public LocalizedString DisplayName => GameInstallation.GetDisplayName();
        public GameIconAsset Icon => GameInstallation.GameDescriptor.Icon;
        public bool IsDemo => GameInstallation.GameDescriptor.IsDemo;

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                ParentViewModel.HasSelection = value || ParentViewModel.SelectedGames.Any();
            }
        }
    }
}