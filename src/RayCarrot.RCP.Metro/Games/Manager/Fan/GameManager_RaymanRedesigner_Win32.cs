#nullable disable
using System.Collections.Generic;
using NLog;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman ReDesigner (Win32) game manager
/// </summary>
public sealed class GameManager_RaymanRedesigner_Win32 : GameManager_Win32
{
    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Public Overrides

    /// <summary>
    /// The game
    /// </summary>
    public override Games Game => Games.RaymanRedesigner;

    /// <summary>
    /// Gets the purchase links for the game for this type
    /// </summary>
    public override IList<GamePurchaseLink> GetGamePurchaseLinks => new GamePurchaseLink[]
    {
        new GamePurchaseLink(Resources.GameDisplay_GameJolt, "https://gamejolt.com/games/Rayman_ReDesigner/539216", GenericIconKind.GameDisplay_Web), 
    };

    /// <summary>
    /// Gets the additional overflow button items for the game
    /// </summary>
    public override IList<OverflowButtonItemViewModel> GetAdditionalOverflowButtonItems => new OverflowButtonItemViewModel[]
    {
        new OverflowButtonItemViewModel(Resources.GameDisplay_OpenGameJoltPage, GenericIconKind.GameDisplay_Web, new AsyncRelayCommand(async () =>
        {
            (await Services.File.LaunchFileAsync("https://gamejolt.com/games/Rayman_ReDesigner/539216"))?.Dispose();
            Logger.Trace("The game {0} GameJolt page was opened", Game);
        })),
    };

    #endregion
}