using System.Threading.Tasks;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// An available action to perform for attempting to add the game
/// </summary>
public abstract class GameAddAction
{
    public abstract LocalizedString Header { get; }
    public abstract GenericIconKind Icon { get; }
    public abstract bool IsAvailable { get; }

    /// <summary>
    /// Attempts to add the game
    /// </summary>
    /// <returns>The added game installation or null if not added</returns>
    public abstract Task<GameInstallation?> AddGameAsync();
}