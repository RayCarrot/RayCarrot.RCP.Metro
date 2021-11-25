#nullable disable
namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman ReDesigner game info
/// </summary>
public sealed class GameInfo_RaymanRedesigner : GameInfo
{
    #region Public Override Properties

    /// <summary>
    /// The game
    /// </summary>
    public override Games Game => Games.RaymanRedesigner;

    /// <summary>
    /// The category for the game
    /// </summary>
    public override GameCategory Category => GameCategory.Fan;

    /// <summary>
    /// The game display name
    /// </summary>
    public override string DisplayName => "Rayman ReDesigner";

    /// <summary>
    /// The game backup name
    /// </summary>
    public override string BackupName => null;

    /// <summary>
    /// Gets the launch name for the game
    /// </summary>
    public override string DefaultFileName => "Rayman ReDesigner.exe";

    #endregion
}