namespace RayCarrot.RCP.Metro;

/// <summary>
/// The available game categories
/// </summary>
public enum GameCategory
{
    /// <summary>
    /// The Rayman game category
    /// </summary>
    [GameCategoryInfo(nameof(Resources.GamesPage_Category_Rayman), GenericIconKind.Games_Rayman)]
    Rayman,

    /// <summary>
    /// The Rabbids game category
    /// </summary>
    [GameCategoryInfo(nameof(Resources.GamesPage_Category_Rabbids), GenericIconKind.Games_Rabbids)]
    Rabbids,

    /// <summary>
    /// The fan-game category
    /// </summary>
    [GameCategoryInfo(nameof(Resources.GamesPage_Category_Fan), GenericIconKind.Games_FanGames)]
    Fan,

    /// <summary>
    /// The other category
    /// </summary>
    [GameCategoryInfo(nameof(Resources.GamesPage_Category_Other), GenericIconKind.Games_Other)]
    Other
}