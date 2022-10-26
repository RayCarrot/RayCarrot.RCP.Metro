using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Shell;
using NLog;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// Manager for the application jump-list
/// </summary>
public class JumpListManager
{
    public JumpListManager(AppViewModel appViewModel, AppUserData data, GamesManager gamesManager)
    {
        AppViewModel = appViewModel ?? throw new ArgumentNullException(nameof(appViewModel));
        Data = data ?? throw new ArgumentNullException(nameof(data));
        GamesManager = gamesManager ?? throw new ArgumentNullException(nameof(gamesManager));
    }

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    private AppViewModel AppViewModel { get; }
    private AppUserData Data { get; }
    private GamesManager GamesManager { get; }

    public void Initialize()
    {
        // Subscribe to when to refresh the jump list
        AppViewModel.RefreshRequired += (_, e) =>
        {
            if (e.GameCollectionModified || e.GameInfoModified || e.JumpListModified)
                Refresh();

            return Task.CompletedTask;
        };
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
                        Select(x => x.GameManager.GetJumpListItems(x)).
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
}