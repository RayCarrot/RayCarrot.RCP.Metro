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
                        SelectMany(x => x.GameDescriptor.GetJumpListItems(x)).
                        // Keep only the included items
                        Where(x => Data.App_JumpListItemIDCollection.Contains(x.ID)).
                        // Keep custom order
                        OrderBy(x => Data.App_JumpListItemIDCollection.IndexOf(x.ID)).
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
        int count = Data.App_JumpListItemIDCollection.Count;

        Data.App_JumpListItemIDCollection.AddRange(gameInstallation.GameDescriptor.GetJumpListItems(gameInstallation).Select(x => x.ID));

        if (count != Data.App_JumpListItemIDCollection.Count)
            Refresh();
    }

    public void RemoveGame(GameInstallation gameInstallation)
    {
        int count = Data.App_JumpListItemIDCollection.Count;

        foreach (JumpListItemViewModel item in gameInstallation.GameDescriptor.GetJumpListItems(gameInstallation))
            Data.App_JumpListItemIDCollection.RemoveWhere(x => x == item.ID);

        if (count != Data.App_JumpListItemIDCollection.Count)
            Refresh();
    }

    public void Receive(RemovedGamesMessage message) => Refresh();
    public void Receive(ModifiedGamesMessage message) => Refresh();
}