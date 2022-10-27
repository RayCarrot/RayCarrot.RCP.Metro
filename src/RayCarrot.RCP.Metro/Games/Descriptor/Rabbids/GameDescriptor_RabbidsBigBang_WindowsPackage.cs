#nullable disable
using System.Collections.Generic;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rabbids Big Bang game descriptor
/// </summary>
public sealed class GameDescriptor_RabbidsBigBang_WindowsPackage : WindowsPackageGameDescriptor
{
    #region Descriptor

    public override string Id => "RabbidsBigBang_WindowsPackage";
    public override Game Game => Game.RabbidsBigBang;

    /// <summary>
    /// The game
    /// </summary>
    public override Games LegacyGame => Games.RabbidsBigBang;

    /// <summary>
    /// The category for the game
    /// </summary>
    public override GameCategory Category => GameCategory.Rabbids;

    /// <summary>
    /// The game display name
    /// </summary>
    public override string DisplayName => "Rabbids Big Bang";

    /// <summary>
    /// The game backup name
    /// </summary>
    public override string BackupName => "Rabbids Big Bang";

    /// <summary>
    /// Gets the default file name for launching the game, if available
    /// </summary>
    public override string DefaultFileName => "Template.exe";

    public override IEnumerable<ProgressionGameViewModel> GetProgressionGameViewModels(GameInstallation gameInstallation) => 
        new ProgressionGameViewModel_RabbidsBigBang(gameInstallation).Yield();

    public override bool AllowPatching => false;

    #endregion

    #region Platform

    public override string PackageName => "UbisoftEntertainment.RabbidsBigBang";
    public override string FullPackageName => "UbisoftEntertainment.RabbidsBigBang_dbgk1hhpxymar";

    #endregion

    #region Other

    private const string MicrosoftStoreID = "9WZDNCRFJCS3";

    #endregion
}