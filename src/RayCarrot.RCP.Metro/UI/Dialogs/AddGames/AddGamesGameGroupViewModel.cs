using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace RayCarrot.RCP.Metro;

public class AddGamesGameGroupViewModel : BaseViewModel
{
    public AddGamesGameGroupViewModel(GameIconAsset icon, LocalizedString displayName, IEnumerable<GameDescriptor> gameDescriptors)
    {
        Icon = icon;
        DisplayName = displayName;
        Games = new ObservableCollection<AddGamesGameViewModel>(gameDescriptors.Select(x => new AddGamesGameViewModel(x)));
    }

    public GameIconAsset Icon { get; }
    public LocalizedString DisplayName { get; }
    public ObservableCollection<AddGamesGameViewModel> Games { get; }
}