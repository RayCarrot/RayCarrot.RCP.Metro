namespace RayCarrot.RCP.Metro;

public class AddGamesViewModel : BaseViewModel, IInitializable, 
    IRecipient<AddedGamesMessage>, IRecipient<RemovedGamesMessage>
{
    public AddGamesViewModel()
    {
        GameCategories = new ObservableCollection<AddGamesGameCategoryViewModel>();

        // Enumerate every category of games
        foreach (var categorizedGames in Services.Games.GetGameDescriptors().GroupBy(x => x.Category))
        {
            // Create a view model
            GameCategoryInfoAttribute categoryInfo = categorizedGames.Key.GetInfo();
            AddGamesGameCategoryViewModel category = new(categoryInfo.DisplayName, categoryInfo.Icon);
            GameCategories.Add(category);

            // Enumerate every group of games
            foreach (var gameDescriptors in categorizedGames.GroupBy(x => x.Game))
            {
                // Get the game info
                GameInfoAttribute gameInfo = gameDescriptors.Key.GetInfo();

                // Add the group of games
                category.GameGroups.Add(new AddGamesGameGroupViewModel(
                    icon: gameInfo.GameIcon,
                    displayName: gameInfo.DisplayName,
                    gameDescriptors: gameDescriptors));
            }
        }
    }

    public ObservableCollection<AddGamesGameCategoryViewModel> GameCategories { get; }

    private void RefreshGames()
    {
        foreach (AddGamesGameCategoryViewModel gameCategory in GameCategories)
        {
            foreach (AddGamesGameGroupViewModel gameGroup in gameCategory.GameGroups)
            {
                foreach (AddGamesGameViewModel game in gameGroup.Games)
                {
                    game.Refresh();
                }
            }
        }
    }

    public void Initialize() => Services.Messenger.RegisterAll(this);
    public void Deinitialize() => Services.Messenger.UnregisterAll(this);

    public void Receive(AddedGamesMessage message) => RefreshGames();
    public void Receive(RemovedGamesMessage message) => RefreshGames();
}