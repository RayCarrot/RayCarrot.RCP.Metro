namespace RayCarrot.RCP.Metro.Pages.Games;

public class GameCategoryViewModel : BaseViewModel
{
    public GameCategoryViewModel(GameCategory category)
    {
        Category = category;
        
        GameCategoryInfoAttribute info = category.GetInfo();
        Icon = info.Icon;
        DisplayName = info.DisplayName;
    }

    public GameCategory Category { get; }
    public LocalizedString DisplayName { get; }
    public GameCategoryIconAsset Icon { get; }

    public bool IsEnabled { get; set; }
    public bool IsSelected { get; set; }
}