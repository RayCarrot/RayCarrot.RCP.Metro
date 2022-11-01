using System.Collections.Generic;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman ReDesigner (Win32) game descriptor
/// </summary>
public sealed class GameDescriptor_RaymanRedesigner_Win32 : Win32GameDescriptor
{
    #region Public Properties

    public override string Id => "RaymanRedesigner_Win32";
    public override Game Game => Game.RaymanRedesigner;
    public override GameCategory Category => GameCategory.Fan;
    public override Games LegacyGame => Games.RaymanRedesigner;

    public override string DisplayName => "Rayman ReDesigner";
    public override string DefaultFileName => "Rayman ReDesigner.exe";

    #endregion

    #region Public Methods

    public override IEnumerable<GamePurchaseLink> GetGamePurchaseLinks() => new GamePurchaseLink[]
    {
        new(Resources.GameDisplay_GameJolt, "https://gamejolt.com/games/Rayman_ReDesigner/539216", GenericIconKind.GameDisplay_Web),
    };

    public override IEnumerable<OverflowButtonItemViewModel> GetAdditionalOverflowButtonItems() => new OverflowButtonItemViewModel[]
    {
        new(Resources.GameDisplay_OpenGameJoltPage, GenericIconKind.GameDisplay_Web, new AsyncRelayCommand(async () =>
        {
            (await Services.File.LaunchFileAsync("https://gamejolt.com/games/Rayman_ReDesigner/539216"))?.Dispose();
        })),
    };

    #endregion
}