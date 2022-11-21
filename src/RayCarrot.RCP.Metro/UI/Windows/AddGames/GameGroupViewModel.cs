using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace RayCarrot.RCP.Metro;

public class GameGroupViewModel : BaseViewModel
{
    public GameGroupViewModel(string iconSource, LocalizedString displayName, IEnumerable<GameDescriptor> gameDescriptors)
    {
        IconSource = iconSource;
        DisplayName = displayName;
        Games = new ObservableCollection<GameViewModel>(gameDescriptors.Select(x => new GameViewModel(x)));
    }

    public string IconSource { get; }
    public LocalizedString DisplayName { get; }
    public ObservableCollection<GameViewModel> Games { get; }
}