namespace RayCarrot.RCP.Metro.Games.Finder;

public class GameFinderItem : FinderItem
{
    public GameFinderItem(GameDescriptor gameDescriptor, FinderQuery[] queries) : base(queries)
    {
        GameDescriptor = gameDescriptor;
    }

    public override string ItemId => GameDescriptor.GameId;
    public GameDescriptor GameDescriptor { get; }

    public GamesManager.GameToAdd? GetGameToAdd()
    {
        if (!HasBeenFound)
            return null;

        ConfigureGameInstallation? configureGameInstallation = null;

        if (FoundQuery.ConfigureInstallation != null)
            configureGameInstallation = new ConfigureGameInstallation(FoundQuery.ConfigureInstallation);

        return new GamesManager.GameToAdd(GameDescriptor, FoundLocation.Value, configureGameInstallation);
    }

    protected override bool ValidateLocation(InstallLocation installLocation) => 
        GameDescriptor.ValidateLocation(installLocation).IsValid;
}