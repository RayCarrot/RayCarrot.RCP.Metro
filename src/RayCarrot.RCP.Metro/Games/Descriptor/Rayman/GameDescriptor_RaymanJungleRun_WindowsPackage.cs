#nullable disable
using System.Collections.Generic;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman Jungle Run game descriptor
/// </summary>
public sealed class GameDescriptor_RaymanJungleRun_WindowsPackage : WindowsPackageGameDescriptor
{
    #region Descriptor

    public override string Id => "RaymanJungleRun_WindowsPackage";
    public override Game Game => Game.RaymanJungleRun;

    /// <summary>
    /// The game
    /// </summary>
    public override Games LegacyGame => Games.RaymanJungleRun;

    /// <summary>
    /// The category for the game
    /// </summary>
    public override GameCategory Category => GameCategory.Rayman;

    /// <summary>
    /// The game display name
    /// </summary>
    public override string DisplayName => "Rayman Jungle Run";

    /// <summary>
    /// The game backup name
    /// </summary>
    public override string BackupName => "Rayman Jungle Run";

    /// <summary>
    /// Gets the default file name for launching the game, if available
    /// </summary>
    public override string DefaultFileName => "RO1Mobile.exe";

    public override GameOptionsDialog_ConfigPageViewModel GetConfigPageViewModel(GameInstallation gameInstallation) => 
        new Config_RaymanJungleRun_ViewModel(gameInstallation);

    public override IEnumerable<ProgressionGameViewModel> GetProgressionGameViewModels(GameInstallation gameInstallation) => 
        new ProgressionGameViewModel_RaymanJungleRun(gameInstallation).Yield();

    public override bool AllowPatching => false;

    #endregion

    #region Platform

    public override string PackageName => "UbisoftEntertainment.RaymanJungleRun";
    public override string FullPackageName => "UbisoftEntertainment.RaymanJungleRun_dbgk1hhpxymar";

    #endregion

    #region Other

    private const string MicrosoftStoreID = "9WZDNCRFJ13P";

    #endregion
}