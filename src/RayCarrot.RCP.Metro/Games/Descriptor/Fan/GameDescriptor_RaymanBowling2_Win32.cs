using System.Collections.Generic;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman Bowling 2 (Win32) game descriptor
/// </summary>
public sealed class GameDescriptor_RaymanBowling2_Win32 : Win32GameDescriptor
{
    #region Public Properties

    public override string Id => "RaymanBowling2_Win32";
    public override Game Game => Game.RaymanBowling2;
    public override GameCategory Category => GameCategory.Fan;
    public override Games? LegacyGame => Games.RaymanBowling2;

    public override string DisplayName => "Rayman Bowling 2";
    public override string BackupName => "Rayman Bowling 2";
    public override string DefaultFileName => "Rayman Bowling 2.exe";

    public override GameIconAsset Icon => GameIconAsset.RaymanBowling2;

    #endregion

    #region Public Methods

    public override GameProgressionManager GetGameProgressionManager(GameInstallation gameInstallation) => 
        new GameProgressionManager_RaymanBowling2(gameInstallation);

    public override IEnumerable<GameUriLink> GetExternalUriLinks(GameInstallation gameInstallation) => new[]
    {
        new GameUriLink(
            Header: new ResourceLocString(nameof(Resources.GameDisplay_OpenGameJoltPage)),
            Uri: "https://gamejolt.com/games/rayman_bowling_2/532563",
            Icon: GenericIconKind.GameDisplay_Web)
    };

    public override IEnumerable<GamePurchaseLink> GetPurchaseLinks() => new GamePurchaseLink[]
    {
        new(new ResourceLocString(nameof(Resources.GameDisplay_GameJolt)), "https://gamejolt.com/games/rayman_bowling_2/532563", GenericIconKind.GameDisplay_Web),
    };

    #endregion
}