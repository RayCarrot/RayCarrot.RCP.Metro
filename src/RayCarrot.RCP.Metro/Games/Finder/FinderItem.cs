using System.Diagnostics.CodeAnalysis;

namespace RayCarrot.RCP.Metro.Games.Finder;

public abstract class FinderItem
{
    #region Constructor

    protected FinderItem(FinderQuery[] queries)
    {
        Queries = queries;
    }

    #endregion

    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Public Properties

    /// <summary>
    /// The unique id for this item
    /// </summary>
    public abstract string ItemId { get; }

    /// <summary>
    /// The finder queries to use when finding the item
    /// </summary>
    public FinderQuery[] Queries { get; }

    /// <summary>
    /// The location. This is set if it has been found.
    /// </summary>
    public FileSystemPath? FoundLocation { get; private set; }

    /// <summary>
    /// The query which found the item, if it has been found.
    /// </summary>
    public FinderQuery? FoundQuery { get; private set; }

    /// <summary>
    /// Indicates if this item has been found by one of its queries
    /// </summary>
    [MemberNotNullWhen(true, nameof(FoundLocation))]
    [MemberNotNullWhen(true, nameof(FoundQuery))]
    public bool HasBeenFound => FoundLocation != null && FoundQuery != null;

    #endregion

    #region Protected Methods

    protected abstract bool ValidateLocation(FileSystemPath location);

    #endregion

    #region Public Methods

    /// <summary>
    /// Validates a found location for the item
    /// </summary>
    /// <param name="query">The query from which the location was found</param>
    /// <param name="location">The found location</param>
    public void Validate(FinderQuery query, FileSystemPath location)
    {
        Logger.Info("A location was found for {0}", ItemId);

        if (HasBeenFound)
        {
            Logger.Warn("{0} could not be validated. The item has already been found.", ItemId);
            return;
        }

        // Make sure the location exists
        if (!location.DirectoryExists)
        {
            Logger.Warn("{0} could not be validated. The location does not exist.", ItemId);
            return;
        }

        // Have the query validate the location
        if (query.ValidateLocationFunc != null)
        {
            location = query.ValidateLocationFunc(location);

            // Make sure the location till exists
            if (!location.DirectoryExists)
            {
                Logger.Warn("{0} could not be validated. The location does not exist after query validation.", ItemId);
                return;
            }
        }

        if (ValidateLocation(location))
        {
            FoundLocation = location;
            FoundQuery = query;
            Logger.Info("The location for {0} was valid", ItemId);
        }
        else
        {
            Logger.Warn("The location for {0} was not valid", ItemId);
        }
    }

    #endregion
}