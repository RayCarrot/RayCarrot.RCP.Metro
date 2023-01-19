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

    protected override bool ValidateLocation(InstallLocation location) => GameClientDescriptor.IsValid(location);
}