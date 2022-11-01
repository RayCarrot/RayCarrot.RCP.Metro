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
    public override Games LegacyGame => Games.RaymanBowling2;

    public override string DisplayName => "Rayman Bowling 2";
    public override string BackupName => "Rayman Bowling 2";
    public override string DefaultFileName => "Rayman Bowling 2.exe";

    #endregion

    #region Public Methods

    public override IEnumerable<ProgressionGameViewModel> GetProgressionGameViewModels(GameInstallation gameInstallation) =>
        new ProgressionGameViewModel_RaymanBowling2(gameInstallation).Yield();

    public override IEnumerable<GamePurchaseLink> GetGamePurchaseLinks() => new GamePurchaseLink[]
    {
        new(Resources.GameDisplay_GameJolt, "https://gamejolt.com/games/rayman_bowling_2/532563", GenericIconKind.GameDisplay_Web),
    };

    public override IEnumerable<OverflowButtonItemViewModel> GetAdditionalOverflowButtonItems() => new OverflowButtonItemViewModel[]
    {
        new(Resources.GameDisplay_OpenGameJoltPage, GenericIconKind.GameDisplay_Web, new AsyncRelayCommand(async () =>
        {
            (await Services.File.LaunchFileAsync("https://gamejolt.com/games/rayman_bowling_2/532563"))?.Dispose();
        })),
    };

    #endregion
}