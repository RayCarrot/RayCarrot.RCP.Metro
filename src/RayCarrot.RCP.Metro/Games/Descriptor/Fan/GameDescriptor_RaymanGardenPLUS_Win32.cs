#nullable disable
namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman Garden PLUS game descriptor
/// </summary>
public sealed class GameDescriptor_RaymanGardenPLUS_Win32 : Win32GameDescriptor
{
    #region Public Override Properties

    public override string Id => "RaymanGardenPLUS_Win32";
    public override Game Game => Game.RaymanGardenPLUS;

    /// <summary>
    /// The game
    /// </summary>
    public override Games LegacyGame => Games.RaymanGardenPLUS;

    /// <summary>
    /// The category for the game
    /// </summary>
    public override GameCategory Category => GameCategory.Fan;

    /// <summary>
    /// The game display name
    /// </summary>
    public override string DisplayName => "Rayman Garden PLUS";

    /// <summary>
    /// The game backup name
    /// </summary>
    public override string BackupName => "Rayman Garden PLUS";

    /// <summary>
    /// Gets the launch name for the game
    /// </summary>
    public override string DefaultFileName => "rayman-garden-plus.exe";

    #endregion
}