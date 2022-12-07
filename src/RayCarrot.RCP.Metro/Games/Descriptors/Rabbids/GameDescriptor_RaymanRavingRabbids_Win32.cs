using System.Collections.Generic;
using RayCarrot.RCP.Metro.Games.Components;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman Raving Rabbids (Win32) game descriptor
/// </summary>
public sealed class GameDescriptor_RaymanRavingRabbids_Win32 : Win32GameDescriptor
{
    #region Public Properties

    public override string GameId => "RaymanRavingRabbids_Win32";
    public override Game Game => Game.RaymanRavingRabbids;
    public override GameCategory Category => GameCategory.Rabbids;
    public override LegacyGame? LegacyGame => Metro.LegacyGame.RaymanRavingRabbids;

    public override string DisplayName => "Rayman Raving Rabbids";
    public override string DefaultFileName => "CheckApplication.exe";

    public override GameIconAsset Icon => GameIconAsset.RaymanRavingRabbids;

    #endregion

    #region Protected Methods

    protected override void RegisterComponents(DescriptorComponentBuilder builder)
    {
        base.RegisterComponents(builder);

        builder.Register(new ProgressionManagersComponent(x => new GameProgressionManager_RaymanRavingRabbids(x, "Rayman Raving Rabbids")));
    }

    #endregion

    #region Public Methods

    public override GameOptionsDialog_ConfigPageViewModel GetConfigPageViewModel(GameInstallation gameInstallation) =>
        new Config_RaymanRavingRabbids_ViewModel(gameInstallation);

    public override IEnumerable<GameUriLink> GetLocalUriLinks(GameInstallation gameInstallation) => new GameUriLink[]
    {
        new(new ResourceLocString(nameof(Resources.GameLink_Setup)), gameInstallation.InstallLocation + "SettingsApplication.exe")
    };

    public override IEnumerable<GamePurchaseLink> GetPurchaseLinks() => new GamePurchaseLink[]
    {
        new(new ResourceLocString(nameof(Resources.GameDisplay_PurchaseGOG)), "https://www.gog.com/game/rayman_raving_rabbids"),
        new(new ResourceLocString(nameof(Resources.GameDisplay_PurchaseUplay)), "https://store.ubi.com/eu/rayman-raving-rabbids/56c4948888a7e300458b47de.html")
    };

    public override GameFinder_GameItem GetGameFinderItem() => new(null, "Rayman Raving Rabbids", new[]
    {
        "Rayman Raving Rabbids",
        "Rayman: Raving Rabbids",
        "RRR",
    });

    #endregion
}