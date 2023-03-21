namespace RayCarrot.RCP.Metro.Games.Finder;

public class GameFinderItem : FinderItem
{
    public GameFinderItem(GameDescriptor gameDescriptor, FinderQuery[] queries) : base(queries)
    {
        GameDescriptor = gameDescriptor;
    }

    public override string ItemId => GameDescriptor.GameId;
    public GameDescriptor GameDescriptor { get; }

    protected override bool ValidateLocation(InstallLocation installLocation) => 
        GameDescriptor.ValidateLocation(installLocation).IsValid;
}