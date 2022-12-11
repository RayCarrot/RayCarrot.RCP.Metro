namespace RayCarrot.RCP.Metro;

public static class GameExtensions
{
    public static GameInfoAttribute GetInfo(this Game game) =>
        game.GetAttribute<GameInfoAttribute>() ?? throw new InvalidOperationException($"Game {game} has no info attribute");

    public static GameCategoryInfoAttribute GetInfo(this GameCategory category) =>
        category.GetAttribute<GameCategoryInfoAttribute>() ?? throw new InvalidOperationException($"Category {category} has no info attribute");

    public static GamePlatformInfoAttribute GetInfo(this GamePlatform platform) =>
        platform.GetAttribute<GamePlatformInfoAttribute>() ?? throw new InvalidOperationException($"Platform {platform} has no info attribute");
}