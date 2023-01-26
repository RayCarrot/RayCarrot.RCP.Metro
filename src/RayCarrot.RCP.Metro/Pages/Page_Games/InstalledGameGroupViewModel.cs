namespace RayCarrot.RCP.Metro;

public class InstalledGameGroupViewModel : BaseViewModel
{
    public InstalledGameGroupViewModel(GameIconAsset icon, LocalizedString displayName, IEnumerable<GameInstallation> gameInstallations)
    {
        Icon = icon;
        DisplayName = displayName;
        InstalledGames = new ObservableCollection<InstalledGameViewModel>(gameInstallations.Select(x => new InstalledGameViewModel(x)));
    }

    private InstalledGameViewModel? _selectedInstalledGame;

    public GameIconAsset Icon { get; }
    public LocalizedString DisplayName { get; }
    public ObservableCollection<InstalledGameViewModel> InstalledGames { get; }

    public bool IsSelected
    {
        get => SelectedInstalledGame != null;
        set => SelectedInstalledGame = value ? InstalledGames.First() : null;
    }

    public InstalledGameViewModel? SelectedInstalledGame
    {
        get => _selectedInstalledGame;
        set
        {
            _selectedInstalledGame = value;

            if (value != null)
                Invoke();

            async void Invoke() => await value.LoadAsync();
        }
    }

    public bool MatchesFilter(string filter)
    {
        bool matchesString(string str) => str.IndexOf(filter, StringComparison.CurrentCultureIgnoreCase) != -1;

        // Check the display name of the group
        if (matchesString(DisplayName))
            return true;

        // Check the games in the group
        foreach (InstalledGameViewModel game in InstalledGames)
        {
            if (matchesString(game.DisplayName) ||
                game.GameDescriptor.SearchKeywords.Any(matchesString))
                return true;
        }

        return false;
    }
}