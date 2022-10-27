#nullable disable
namespace RayCarrot.RCP.Metro;
// IDEA: Add backup info

/// <summary>
/// The Rayman Premiers Clics game descriptor
/// </summary>
public sealed class GameDescriptor_RaymanPremiersClics_Win32 : Win32GameDescriptor
{
    #region Public Override Properties

    public override string Id => "RaymanPremiersClics_Win32";
    public override Game Game => Game.RaymanPremiersClics;

    /// <summary>
    /// The game
    /// </summary>
    public override Games LegacyGame => Games.RaymanPremiersClics;

    /// <summary>
    /// The category for the game
    /// </summary>
    public override GameCategory Category => GameCategory.Other;

    /// <summary>
    /// The game display name
    /// </summary>
    public override string DisplayName => "Rayman Premiers Clics";

    /// <summary>
    /// The game backup name
    /// </summary>
    public override string BackupName => null;

    /// <summary>
    /// Gets the launch name for the game
    /// </summary>
    public override string DefaultFileName => "RAYMAN.exe";

    /// <summary>
    /// Indicates if the game can be located. If set to false the game is required to be downloadable.
    /// </summary>
    public override bool CanBeLocated => true;

    #endregion
}