namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rabbids Coding game descriptor
/// </summary>
public sealed class GameDescriptor_RabbidsCoding_Win32 : Win32GameDescriptor
{
    #region Public Override Properties

    public override string Id => "RabbidsCoding_Win32";
    public override Game Game => Game.RabbidsCoding;

    /// <summary>
    /// The game
    /// </summary>
    public override Games LegacyGame => Games.RabbidsCoding;

    /// <summary>
    /// The category for the game
    /// </summary>
    public override GameCategory Category => GameCategory.Rabbids;

    /// <summary>
    /// The game display name
    /// </summary>
    public override string DisplayName => "Rabbids Coding";

    /// <summary>
    /// The game backup name
    /// </summary>
    public override string BackupName => "Rabbids Coding";

    /// <summary>
    /// Gets the launch name for the game
    /// </summary>
    public override string DefaultFileName => "Rabbids Coding.exe";

    #endregion
}