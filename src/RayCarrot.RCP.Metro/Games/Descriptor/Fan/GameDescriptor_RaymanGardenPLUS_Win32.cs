using System.Collections.Generic;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman Garden PLUS (Win32) game descriptor
/// </summary>
public sealed class GameDescriptor_RaymanGardenPLUS_Win32 : Win32GameDescriptor
{
    #region Public Properties

    public override string Id => "RaymanGardenPLUS_Win32";
    public override Game Game => Game.RaymanGardenPLUS;
    public override GameCategory Category => GameCategory.Fan;
    public override Games? LegacyGame => Games.RaymanGardenPLUS;

    public override string DisplayName => "Rayman Garden PLUS";
    public override string BackupName => "Rayman Garden PLUS";

    public override string DefaultFileName => "rayman-garden-plus.exe";

    public override GameIconAsset Icon => GameIconAsset.RaymanGardenPLUS;

    #endregion

    #region Public Methods

    public override IEnumerable<GameUriLink> GetExternalUriLinks(GameInstallation gameInstallation) => new[]
    {
        new GameUriLink(
            Header: new ResourceLocString(nameof(Resources.GameDisplay_OpenGameJoltPage)),
            Uri: "https://gamejolt.com/games/RaymanGardenPlus/622289",
            Icon: GenericIconKind.GameDisplay_Web)
    };

    public override IEnumerable<GamePurchaseLink> GetGamePurchaseLinks() => new GamePurchaseLink[]
    {
        new(Resources.GameDisplay_GameJolt, "https://gamejolt.com/games/RaymanGardenPlus/622289", GenericIconKind.GameDisplay_Web),
    };

    #endregion
}