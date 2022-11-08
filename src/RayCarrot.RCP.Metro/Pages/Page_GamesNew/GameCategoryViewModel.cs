using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;

namespace RayCarrot.RCP.Metro;

public class GameCategoryViewModel : BaseViewModel
{
    public GameCategoryViewModel(string displayName, Func<string> getGameFilter)
    {
        DisplayName = displayName;
        GameGroups = new ObservableCollection<GameGroupViewModel>();
        var source = CollectionViewSource.GetDefaultView(GameGroups);
        source.Filter = p => ((GameGroupViewModel)p).MatchesFilter(getGameFilter());
        FilteredGameGroups = source;
    }

    public string DisplayName { get; }
    public bool IsExpanded { get; set; } = true;
    public ObservableCollection<GameGroupViewModel> GameGroups { get; }
    public ICollectionView FilteredGameGroups { get; }

    public void UpdateFilteredCollections()
    {
        FilteredGameGroups.Refresh();
    }
}