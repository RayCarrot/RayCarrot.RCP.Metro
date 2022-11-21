using System;
using System.Linq;
using System.Windows.Shell;
using CommunityToolkit.Mvvm.Messaging;
using NLog;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// Manager for the application jump-list
/// </summary>
public class JumpListManager : IRecipient<AddedGamesMessage>, IRecipient<RemovedGamesMessage>, IRecipient<ModifiedGamesMessage>
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
                if (Data.App_JumpListItemIDCollection == null)
                {
                    Logger.Warn("The jump could not refresh due to collection not existing");

                    return;
                }

                // Create a jump list
                new JumpList(GamesManager.EnumerateInstalledGames().
                        // Get the items for each game
                        Select(x => x.GameDescriptor.GetJumpListItems(x)).
                        // Select into single collection
                        SelectMany(x => x).
                        // Keep only the included items
                        Where(x => x.IsIncluded).
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
                            IconResourcePath = x.IconSource
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

    public void Receive(AddedGamesMessage message) => Refresh();
    public void Receive(RemovedGamesMessage message) => Refresh();
    public void Receive(ModifiedGamesMessage message) => Refresh();
}