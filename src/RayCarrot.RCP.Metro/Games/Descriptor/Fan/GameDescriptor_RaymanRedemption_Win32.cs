using System.Collections.Generic;

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
    public override Games? LegacyGame => Games.RaymanRedemption;

    public override string DisplayName => "Rayman Redemption";
    public override string DefaultFileName => "Rayman Redemption.exe";

    public override GameIconAsset Icon => GameIconAsset.RaymanRedemption;

    #endregion

    #region Public Methods

    public override IEnumerable<GameProgressionManager> GetGameProgressionManagers(GameInstallation gameInstallation) => 
        new GameProgressionManager_RaymanRedemption(gameInstallation, "Rayman Redemption").Yield();

    public override IEnumerable<GameUriLink> GetExternalUriLinks(GameInstallation gameInstallation) => new[]
    {
        new GameUriLink(
            Header: new ResourceLocString(nameof(Resources.GameDisplay_OpenGameJoltPage)),
            Uri: "https://gamejolt.com/games/raymanredemption/340532",
            Icon: GenericIconKind.GameDisplay_Web)
    };

    public override IEnumerable<GamePurchaseLink> GetPurchaseLinks() => new GamePurchaseLink[]
    {
        new(new ResourceLocString(nameof(Resources.GameDisplay_GameJolt)), "https://gamejolt.com/games/raymanredemption/340532", GenericIconKind.GameDisplay_Web),
    };

    #endregion
}