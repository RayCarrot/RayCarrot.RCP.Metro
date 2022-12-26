using RayCarrot.RCP.Metro.Games.Components;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// Provides the functionality to launch the game
/// </summary>
[GameComponent(IsBase = true, SingleInstance = true)]
public abstract class LaunchGameComponent : GameComponent
{
    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Proteted Methods

    protected abstract Task<bool> LaunchImplAsync();

    #endregion

    #region Public Methods

    /// <summary>
    /// Launches the game and calls any optional game launched actions
    /// </summary>
    public async Task LaunchAsync()
    {
        Logger.Trace("The game {0} is being launched...", GameInstallation.FullId);

        // Launch the game
        bool success = await LaunchImplAsync();

        if (success)
            // Invoke any optional launch actions
            await GameInstallation.GetComponents<OnGameLaunchedComponent>().InvokeAllAsync();
    }

    public abstract void CreateShortcut(FileSystemPath shortcutName, FileSystemPath destinationDirectory);
    public abstract IEnumerable<JumpListItemViewModel> GetJumpListItems();

    #endregion
}