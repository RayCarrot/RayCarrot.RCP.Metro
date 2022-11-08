using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace RayCarrot.RCP.Metro;

public class GameGroupViewModel : BaseViewModel
{
    public GameGroupViewModel(string iconSource, IEnumerable<InstalledGameViewModel> installedGames)
    {
        IconSource = iconSource;
        InstalledGames = new ObservableCollection<InstalledGameViewModel>(installedGames);
        DisplayName = InstalledGames.First().DisplayName;
    }

    public string IconSource { get; }
    public string DisplayName { get; }
    public ObservableCollection<InstalledGameViewModel> InstalledGames { get; }

    public bool IsSelected
    {
        get => SelectedInstalledGame != null;
        set => SelectedInstalledGame = value ? InstalledGames.First() : null;
    }

    public InstalledGameViewModel? SelectedInstalledGame { get; set; }

    public bool MatchesFilter(string filter)
    {
        return DisplayName.IndexOf(filter, StringComparison.InvariantCultureIgnoreCase) != -1;
    }
}