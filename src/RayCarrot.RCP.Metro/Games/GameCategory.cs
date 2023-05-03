namespace RayCarrot.RCP.Metro;

/// <summary>
/// The available game categories
/// </summary>
public enum GameCategory
{
    /// <summary>
    /// The Rayman game category
    /// </summary>
    [GameCategoryInfo(nameof(Resources.GamesPage_Category_Rayman), GameCategoryIconAsset.Rayman)]
    Rayman,

    /// <summary>
    /// The Rabbids game category
    /// </summary>
    [GameCategoryInfo(nameof(Resources.GamesPage_Category_Rabbids), GameCategoryIconAsset.Rabbids)]
    Rabbids,

    /// <summary>
    /// The handheld game category
    /// </summary>
    [GameCategoryInfo(nameof(Resources.GamesPage_Category_Handheld), GameCategoryIconAsset.Handheld)]
    Handheld,

    /// <summary>
    /// The fan-game category
    /// </summary>
    [GameCategoryInfo(nameof(Resources.GamesPage_Category_Fan), GameCategoryIconAsset.Fan)]
    Fan,

    /// <summary>
    /// The other category
    /// </summary>
    [GameCategoryInfo(nameof(Resources.GamesPage_Category_Other), GameCategoryIconAsset.Other)]
    Other
}