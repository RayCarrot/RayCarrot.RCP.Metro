using System.Collections.Generic;
using NLog;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// A game descriptor for a Steam game
/// </summary>
public abstract class SteamGameDescriptor : GameDescriptor
{
    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Descriptor

    private SteamPlatformManager? _platformManager;

    public override GamePlatform Platform => GamePlatform.Steam;
    public override PlatformManager PlatformManager => _platformManager ??= new SteamPlatformManager(this);

    /// <summary>
    /// Gets the additional overflow button items for the game
    /// </summary>
    public override IEnumerable<OverflowButtonItemViewModel> GetAdditionalOverflowButtonItems() => new OverflowButtonItemViewModel[]
    {
        new(Resources.GameDisplay_OpenSteamStore, GenericIconKind.GameDisplay_Steam, new AsyncRelayCommand(async () =>
        {
            (await Services.File.LaunchFileAsync(SteamHelpers.GetStorePageURl(SteamID)))?.Dispose();
            Logger.Trace("The game {0} Steam store page was opened", Game);
        })),
        new(Resources.GameDisplay_OpenSteamCommunity, GenericIconKind.GameDisplay_Steam, new AsyncRelayCommand(async () =>
        {
            (await Services.File.LaunchFileAsync(SteamHelpers.GetCommunityPageURl(SteamID)))?.Dispose();
            Logger.Trace("The game {0} Steam community page was opened", Game);
        }))
    };

    #endregion

    #region Platform

    public abstract string SteamID { get; }

    #endregion
}