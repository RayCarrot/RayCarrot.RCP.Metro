namespace RayCarrot.RCP.Metro;

public static class GameSearch
{
    public delegate bool Predicate(GameDescriptor gameDescriptor);

    public static Predicate Create(Game game, GamePlatform platform, bool allowDemos = false) =>
        x => x.Game == game && x.Platform == platform && (allowDemos || !x.IsDemo);

    public static Predicate Create(params Predicate[] predicates) =>
        x => predicates.Any(s => s(x));
}