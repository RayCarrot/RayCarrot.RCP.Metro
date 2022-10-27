#nullable disable
using System.Collections.Generic;
using static RayCarrot.RCP.Metro.GameManager;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman Raving Rabbids game descriptor
/// </summary>
public sealed class GameDescriptor_RaymanRavingRabbids_Win32 : Win32GameDescriptor
{
    #region Public Override Properties

    public override string Id => "RaymanRavingRabbids_Win32";
    public override Game Game => Game.RaymanRavingRabbids;

    /// <summary>
    /// The game
    /// </summary>
    public override Games LegacyGame => Games.RaymanRavingRabbids;

    /// <summary>
    /// The category for the game
    /// </summary>
    public override GameCategory Category => GameCategory.Rabbids;

    /// <summary>
    /// The game display name
    /// </summary>
    public override string DisplayName => "Rayman Raving Rabbids";

    /// <summary>
    /// The game backup name
    /// </summary>
    public override string BackupName => "Rayman Raving Rabbids";

    /// <summary>
    /// Gets the launch name for the game
    /// </summary>
    public override string DefaultFileName => "CheckApplication.exe";

    /// <summary>
    /// The config page view model, if any is available
    /// </summary>
    public override GameOptionsDialog_ConfigPageViewModel GetConfigPageViewModel(GameInstallation gameInstallation) => 
        new Config_RaymanRavingRabbids_ViewModel(gameInstallation);

    public override IEnumerable<ProgressionGameViewModel> GetProgressionGameViewModels(GameInstallation gameInstallation) => 
        new ProgressionGameViewModel_RaymanRavingRabbids(gameInstallation).Yield();

    /// <summary>
    /// Gets the file links for the game
    /// </summary>
    public override IEnumerable<GameFileLink> GetGameFileLinks(GameInstallation gameInstallation) => new GameFileLink[]
    {
        new(Resources.GameLink_Setup, gameInstallation.InstallLocation + "SettingsApplication.exe")
    };

    /// <summary>
    /// Gets the purchase links for the game
    /// </summary>
    public override IEnumerable<GamePurchaseLink> GetGamePurchaseLinks() => new GamePurchaseLink[]
    {
        new(Resources.GameDisplay_PurchaseGOG, "https://www.gog.com/game/rayman_raving_rabbids"),
        new(Resources.GameDisplay_PurchaseUplay, "https://store.ubi.com/eu/rayman-raving-rabbids/56c4948888a7e300458b47de.html")
    };

    #endregion
}