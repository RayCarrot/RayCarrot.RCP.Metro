namespace RayCarrot.RCP.Metro;

public class AddGamesGameCategoryViewModel : BaseViewModel
{
    public AddGamesGameCategoryViewModel(LocalizedString displayName, GenericIconKind icon)
    {
        DisplayName = displayName;
        Icon = icon;
        GameGroups = new ObservableCollection<AddGamesGameGroupViewModel>();
    }

    public LocalizedString DisplayName { get; }
    public GenericIconKind Icon { get; }
    public ObservableCollection<AddGamesGameGroupViewModel> GameGroups { get; }
}