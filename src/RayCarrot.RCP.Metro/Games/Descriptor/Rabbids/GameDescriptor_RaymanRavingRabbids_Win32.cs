using System.Collections.Generic;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman Raving Rabbids (Win32) game descriptor
/// </summary>
public sealed class GameDescriptor_RaymanRavingRabbids_Win32 : Win32GameDescriptor
{
    #region Public Properties

    public override string Id => "RaymanRavingRabbids_Win32";
    public override Game Game => Game.RaymanRavingRabbids;
    public override GameCategory Category => GameCategory.Rabbids;
    public override Games LegacyGame => Games.RaymanRavingRabbids;

    public override string DisplayName => "Rayman Raving Rabbids";
    public override string BackupName => "Rayman Raving Rabbids";
    public override string DefaultFileName => "CheckApplication.exe";

    #endregion

    #region Public Methods

    public override GameOptionsDialog_ConfigPageViewModel GetConfigPageViewModel(GameInstallation gameInstallation) =>
        new Config_RaymanRavingRabbids_ViewModel(gameInstallation);

    public override IEnumerable<ProgressionGameViewModel> GetProgressionGameViewModels(GameInstallation gameInstallation) =>
        new ProgressionGameViewModel_RaymanRavingRabbids(gameInstallation).Yield();

    public override IEnumerable<GameFileLink> GetGameFileLinks(GameInstallation gameInstallation) => new GameFileLink[]
    {
        new(Resources.GameLink_Setup, gameInstallation.InstallLocation + "SettingsApplication.exe")
    };

    public override IEnumerable<GamePurchaseLink> GetGamePurchaseLinks() => new GamePurchaseLink[]
    {
        new(Resources.GameDisplay_PurchaseGOG, "https://www.gog.com/game/rayman_raving_rabbids"),
        new(Resources.GameDisplay_PurchaseUplay, "https://store.ubi.com/eu/rayman-raving-rabbids/56c4948888a7e300458b47de.html")
    };

    public override GameFinder_GameItem GetGameFinderItem() => new(null, "Rayman Raving Rabbids", new[]
    {
        "Rayman Raving Rabbids",
        "Rayman: Raving Rabbids",
        "RRR",
    });

    #endregion
}