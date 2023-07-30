using RayCarrot.RCP.Metro.Games.Clients;

namespace RayCarrot.RCP.Metro.Games.Finder;

public class GameClientFinderItem : FinderItem
{
    public GameClientFinderItem(GameClientDescriptor gameClientDescriptor, FinderQuery[] queries) : base(queries)
    {
        GameClientDescriptor = gameClientDescriptor;
    }

    public override string ItemId => GameClientDescriptor.GameClientId;
    public GameClientDescriptor GameClientDescriptor { get; }

    public GameClientsManager.GameClientToAdd? GetGameClientToAdd()
    {
        if (!HasBeenFound)
            return null;

        ConfigureGameClientInstallation? configureGameClientInstallation = null;

        if (FoundQuery.ConfigureInstallation != null)
            configureGameClientInstallation = new ConfigureGameClientInstallation(FoundQuery.ConfigureInstallation);

        return new GameClientsManager.GameClientToAdd(GameClientDescriptor, FoundLocation.Value, configureGameClientInstallation);
    }

    protected override bool ValidateLocation(InstallLocation location) => GameClientDescriptor.IsValid(location);
}