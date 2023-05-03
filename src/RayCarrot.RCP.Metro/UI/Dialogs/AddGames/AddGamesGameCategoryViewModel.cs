namespace RayCarrot.RCP.Metro;

public class AddGamesGameCategoryViewModel : BaseViewModel
{
    public AddGamesGameCategoryViewModel(LocalizedString displayName, GameCategoryIconAsset icon)
    {
        DisplayName = displayName;
        Icon = icon;
        GameGroups = new ObservableCollection<AddGamesGameGroupViewModel>();
    }

    public LocalizedString DisplayName { get; }
    public GameCategoryIconAsset Icon { get; }
    public ObservableCollection<AddGamesGameGroupViewModel> GameGroups { get; }
}