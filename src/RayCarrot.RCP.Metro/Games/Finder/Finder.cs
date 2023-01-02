namespace RayCarrot.RCP.Metro.Games.Finder;

// TODO-14: Find game clients as well

public class Finder
{
    #region Constructor

    public Finder(FinderOperation[] operations, FinderItem[] finderItems)
    {
        Operations = operations ?? throw new ArgumentNullException(nameof(operations));
        FinderItems = finderItems ?? throw new ArgumentNullException(nameof(finderItems));
    }

    #endregion

    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Public Properties

    public static FinderOperation[] DefaultOperations => new FinderOperation[]
    {
        new UbiIniFinderOperation(),
        new SteamFinderOperation(),
        new UninstallProgramFinderOperation(),
        new Win32ShortcutFinderOperation(),
        new WindowsPackageFinderOperation(),
    };

    public FinderOperation[] Operations { get; }
    public FinderItem[] FinderItems { get; }

    #endregion

    #region Public Methods

    public void Run()
    {
        Logger.Info("Running the finder with {0} operations and {1} finder items", Operations.Length, FinderItems.Length);

        foreach (FinderOperation operation in Operations)
        {
            // Break if all items have been found
            if (FinderItems.All(x => x.HasBeenFound))
                break;

            Logger.Info("Running the finder operation {0}", operation.GetType());

            try
            {
                operation.Run(FinderItems);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Finder on operation {0}", operation.GetType());
            }
        }

        Logger.Info("The finder found {0} items", FinderItems.Count(x => x.HasBeenFound));
    }

    #endregion
}