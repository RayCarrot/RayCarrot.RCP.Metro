namespace RayCarrot.RCP.Metro;

public class AddGamesGameGroupViewModel : BaseViewModel
{
    public AddGamesGameGroupViewModel(GameIconAsset icon, LocalizedString displayName, IEnumerable<GameDescriptor> gameDescriptors)
    {
        Icon = icon;
        DisplayName = displayName;
        Games = new ObservableCollection<AddGamesGameViewModel>(gameDescriptors.Select(x => new AddGamesGameViewModel(x)));

        int nonRetailVersionsCount = Games.Count(x => x.GameDescriptor.Type != GameType.Retail);
        if (nonRetailVersionsCount != 0)
            ShowNonRetailVersionsText = $"Show {nonRetailVersionsCount} demos/prototypes"; // TODO-LOC

        RefreshGamesVisibility();
    }

    public GameIconAsset Icon { get; }
    public LocalizedString DisplayName { get; }
    public ObservableCollection<AddGamesGameViewModel> Games { get; }

    public LocalizedString? ShowNonRetailVersionsText { get; }

    public bool ShowNonRetailVersions
    {
        get;
        set
        {
            field = value;
            RefreshGamesVisibility();
        }
    }

    private void RefreshGamesVisibility()
    {
        foreach (AddGamesGameViewModel game in Games)
            game.IsVisible = ShowNonRetailVersions || game.GameDescriptor.Type == GameType.Retail;
    }
}