using System.Windows.Shell;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// Manager for the application jump-list
/// </summary>
public class JumpListManager : IRecipient<RemovedGamesMessage>, IRecipient<ModifiedGamesMessage>
{
    public JumpListManager(AppUserData data, GamesManager gamesManager, IMessenger messenger)
    {
        // Set services
        Data = data ?? throw new ArgumentNullException(nameof(data));
        GamesManager = gamesManager ?? throw new ArgumentNullException(nameof(gamesManager));

        // Register for messages
        messenger.RegisterAll(this);
    }

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    private AppUserData Data { get; }
    private GamesManager GamesManager { get; }

    public void Initialize()
    {
        Services.InstanceData.CultureChanged += (_, _) => Refresh();
    }

    /// <summary>
    /// Refreshes the application jump list
    /// </summary>
    public void Refresh()
    {
        App.Current.Dispatcher?.Invoke(() =>
        {
            try
            {
                // Create a jump list
                new JumpList(GamesManager.GetInstalledGames().
                        // Get the items for each game
                        SelectMany(x => x.GetComponent<LaunchGameComponent>()?.GetJumpListItems() 
                                        ?? Enumerable.Empty<JumpListItemViewModel>()).
                        // Keep only the included items
                        Where(x => Data.App_JumpListItems.Any(j => j.ItemId == x.Id)).
                        // Keep custom order
                        OrderBy(x => Data.App_JumpListItems.FindIndex(j => j.ItemId == x.Id)).
                        // Create the jump tasks
                        Select(x => new JumpTask
                        {
                            Title = x.Name,
                            Description = String.Format(Resources.JumpListItemDescription, x.Name),
                            ApplicationPath = x.LaunchPath,
                            WorkingDirectory = x.WorkingDirectory,
                            Arguments = x.LaunchArguments,
                            IconResourcePath = x.IconSource,
                        }), false, false).
                    // Apply the new jump list
                    Apply();

                Logger.Info("The jump list has been refreshed");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Creating jump list");
            }
        });
    }

    public void AddGame(GameInstallation gameInstallation)
    {
        // TODO-14: Do emulated games get added to the jump list on add with this? Test!
        LaunchGameComponent? component = gameInstallation.GetComponent<LaunchGameComponent>();
        
        if (component == null)
        {
            Logger.Info("The game {0} was not added to the jump list due to it not having a launch component", 
                gameInstallation.FullId);
            return;
        }

        int count = Data.App_JumpListItems.Count;
        JumpListItemComparer comparer = new(GamesManager);

        foreach (string itemId in component.GetJumpListItems().Select(x => x.Id))
            Data.App_JumpListItems.AddSorted(new JumpListItem(gameInstallation.InstallationId, itemId), comparer);

        Logger.Info("Added {0} to the jump list", gameInstallation.FullId);

        if (count != Data.App_JumpListItems.Count)
            Refresh();
    }

    public void RemoveGame(GameInstallation gameInstallation)
    {
        int count = Data.App_JumpListItems.Count;

        Data.App_JumpListItems.RemoveWhere(x => x.GameInstallationId == gameInstallation.InstallationId);

        Logger.Info("Removed {0} from the jump list", gameInstallation.FullId);

        if (count != Data.App_JumpListItems.Count)
            Refresh();
    }

    public void SetItems(IEnumerable<JumpListItem> items)
    {
        Data.App_JumpListItems = items.ToList();

        Logger.Info("Set the jump list items");

        Refresh();
    }

    public void Receive(RemovedGamesMessage message) => Refresh();
    public void Receive(ModifiedGamesMessage message) => Refresh();
}