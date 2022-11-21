using System.Collections.ObjectModel;
using System.Linq;

namespace RayCarrot.RCP.Metro;

public class AddGamesViewModel : BaseViewModel
{
    public AddGamesViewModel()
    {
        GameCategories = new ObservableCollection<GameCategoryViewModel>();

        // Enumerate every category of games
        foreach (var categorizedGames in Services.Games.EnumerateGameDescriptors().
                     OrderBy(x => x.Category). // TODO-14: Normalize games sorting
                     GroupBy(x => x.Category))
        {
            // Create a view model
            GameCategoryInfoAttribute categoryInfo = categorizedGames.Key.GetInfo();
            GameCategoryViewModel category = new(categoryInfo.DisplayName, categoryInfo.Icon);
            GameCategories.Add(category);

            // Enumerate every group of games
            foreach (var gameDescriptors in categorizedGames.
                         OrderBy(x => x.Game). // TODO-14: Normalize games sorting
                         GroupBy(x => x.Game))
            {
                // Get the game info
                GameInfoAttribute gameInfo = gameDescriptors.Key.GetInfo();

                // Add the group of games
                category.GameGroups.Add(new GameGroupViewModel(
                    // TODO-UPDATE: Normalize
                    iconSource: $"{AppViewModel.WPFApplicationBasePath}Img/GameIcons/{gameInfo.GameIcon.GetAttribute<ImageFileAttribute>()!.FileName}",
                    displayName: gameInfo.DisplayName,
                    gameDescriptors: gameDescriptors));
            }
        }
    }

    public ObservableCollection<GameCategoryViewModel> GameCategories { get; }
}