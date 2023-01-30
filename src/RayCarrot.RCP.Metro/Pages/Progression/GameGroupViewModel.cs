namespace RayCarrot.RCP.Metro.Pages.Progression;

public class GameGroupViewModel : BaseViewModel
{
    public GameGroupViewModel(GameIconAsset icon, LocalizedString displayName, IEnumerable<GameViewModel> games)
    {
        Icon = icon;
        DisplayName = displayName;
        Games = new ObservableCollection<GameViewModel>(games);
    }

    public GameIconAsset Icon { get; }
    public LocalizedString DisplayName { get; }
    public ObservableCollection<GameViewModel> Games { get; }
}