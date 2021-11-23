namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rabbids Coding game info
/// </summary>
public sealed class GameInfo_RabbidsCoding : GameInfo
{
    #region Public Override Properties

    /// <summary>
    /// The game
    /// </summary>
    public override Games Game => Games.RabbidsCoding;

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