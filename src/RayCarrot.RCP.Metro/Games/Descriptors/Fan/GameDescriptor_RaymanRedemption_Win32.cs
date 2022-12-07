using System.Collections.Generic;
using RayCarrot.RCP.Metro.Games.Components;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman Redemption (Win32) game descriptor
/// </summary>
public sealed class GameDescriptor_RaymanRedemption_Win32 : Win32GameDescriptor
{
    #region Public Properties

    public override string GameId => "RaymanRedemption_Win32";
    public override Game Game => Game.RaymanRedemption;
    public override GameCategory Category => GameCategory.Fan;
    public override LegacyGame? LegacyGame => Metro.LegacyGame.RaymanRedemption;

    public override string DisplayName => "Rayman Redemption";
    public override string DefaultFileName => "Rayman Redemption.exe";

    public override GameIconAsset Icon => GameIconAsset.RaymanRedemption;

    #endregion

    #region Protected Methods

    protected override void RegisterComponents(DescriptorComponentBuilder builder)
    {
        base.RegisterComponents(builder);

        builder.Register(new ProgressionManagersComponent(x => new GameProgressionManager_RaymanRedemption(x, "Rayman Redemption")));
    }

    #endregion

    #region Public Methods

    public override IEnumerable<GameUriLink> GetExternalUriLinks(GameInstallation gameInstallation) => new[]
    {
        new GameUriLink(
            Header: new ResourceLocString(nameof(Resources.GameDisplay_OpenGameJoltPage)),
            Uri: "https://gamejolt.com/games/raymanredemption/340532",
            Icon: GenericIconKind.GameAction_Web)
    };

    public override IEnumerable<GamePurchaseLink> GetPurchaseLinks() => new GamePurchaseLink[]
    {
        new(new ResourceLocString(nameof(Resources.GameDisplay_GameJolt)), "https://gamejolt.com/games/raymanredemption/340532", GenericIconKind.GameAction_Web),
    };

    #endregion
}