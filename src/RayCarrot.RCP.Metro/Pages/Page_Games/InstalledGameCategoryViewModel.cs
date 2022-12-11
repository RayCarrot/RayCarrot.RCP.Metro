using System.ComponentModel;
using System.Windows.Data;

namespace RayCarrot.RCP.Metro;

public class InstalledGameCategoryViewModel : BaseViewModel
{
    public InstalledGameCategoryViewModel(LocalizedString displayName, Func<string> getGameFilter)
    {
        DisplayName = displayName;
        GameGroups = new ObservableCollection<InstalledGameGroupViewModel>();
        var source = CollectionViewSource.GetDefaultView(GameGroups);
        source.Filter = p => ((InstalledGameGroupViewModel)p).MatchesFilter(getGameFilter());
        FilteredGameGroups = source;
    }

    public LocalizedString DisplayName { get; }
    public bool IsExpanded { get; set; } = true;
    public ObservableCollection<InstalledGameGroupViewModel> GameGroups { get; }
    public ICollectionView FilteredGameGroups { get; }

    public void UpdateFilteredCollections()
    {
        FilteredGameGroups.Refresh();
    }
}