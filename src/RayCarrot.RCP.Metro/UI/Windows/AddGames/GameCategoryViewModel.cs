using System.Collections.ObjectModel;

namespace RayCarrot.RCP.Metro;

public class GameCategoryViewModel : BaseViewModel
{
    public GameCategoryViewModel(LocalizedString displayName, GenericIconKind icon)
    {
        DisplayName = displayName;
        Icon = icon;
        GameGroups = new ObservableCollection<GameGroupViewModel>();
    }

    public LocalizedString DisplayName { get; }
    public GenericIconKind Icon { get; }
    public ObservableCollection<GameGroupViewModel> GameGroups { get; }
}