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
        return DisplayName.Value.IndexOf(filter, StringComparison.InvariantCultureIgnoreCase) != -1;
    }
}