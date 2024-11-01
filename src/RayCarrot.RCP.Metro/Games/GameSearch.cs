namespace RayCarrot.RCP.Metro;

public static class GameSearch
{
    public delegate bool Predicate(GameDescriptor gameDescriptor);

    public static Predicate Create(Game game, GamePlatform platform, bool allowNonRetailVersions = false) =>
        x => x.Game == game && x.Platform == platform && (allowNonRetailVersions || x.Type == GameType.Retail);

    public static Predicate Create(params Predicate[] predicates) =>
        x => predicates.Any(s => s(x));
}