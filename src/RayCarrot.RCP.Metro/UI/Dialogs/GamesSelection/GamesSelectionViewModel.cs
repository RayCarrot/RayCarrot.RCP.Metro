namespace RayCarrot.RCP.Metro;

/// <summary>
/// View model for a games selection dialog
/// </summary>
public class GamesSelectionViewModel : UserInputViewModel
{
    public GamesSelectionViewModel(IEnumerable<GameInstallation> gameInstallations)
    {
        Games = new ObservableCollection<GameViewModel>(gameInstallations.Select(x => new GameViewModel(this, x)));
    }

    public GamesSelectionViewModel(IEnumerable<GameInstallation> gameInstallations, IEnumerable<GameInstallation> selectedGames) 
        : this(gameInstallations)
    {
        foreach (GameInstallation gameInstallation in selectedGames)
        {
            GameViewModel? gameViewModel = Games.FirstOrDefault(x => x.GameInstallation == gameInstallation);

            if (gameViewModel != null)
                gameViewModel.IsSelected = true;
        }
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