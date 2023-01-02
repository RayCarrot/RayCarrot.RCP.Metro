namespace RayCarrot.RCP.Metro.Games.Finder;

/// <summary>
/// An operation to run in the <see cref="Finder"/>. Each operation will usually handle
/// its own type of <see cref="FinderQuery"/> queries.
/// </summary>
public abstract class FinderOperation
{
    /// <summary>
    /// Runs this operation on the specified items
    /// </summary>
    /// <param name="finderItems">The items to run the finder operation on</param>
    public abstract void Run(FinderItem[] finderItems);
}