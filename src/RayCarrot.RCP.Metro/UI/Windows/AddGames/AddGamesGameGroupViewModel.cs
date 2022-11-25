using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace RayCarrot.RCP.Metro;

public class AddGamesGameGroupViewModel : BaseViewModel
{
    public AddGamesGameGroupViewModel(string iconSource, LocalizedString displayName, IEnumerable<GameDescriptor> gameDescriptors)
    {
        IconSource = iconSource;
        DisplayName = displayName;
        Games = new ObservableCollection<AddGamesGameViewModel>(gameDescriptors.Select(x => new AddGamesGameViewModel(x)));
    }

    public string IconSource { get; }
    public LocalizedString DisplayName { get; }
    public ObservableCollection<AddGamesGameViewModel> Games { get; }
}