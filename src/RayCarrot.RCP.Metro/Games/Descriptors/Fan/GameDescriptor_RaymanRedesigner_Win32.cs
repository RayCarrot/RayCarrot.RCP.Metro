namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman ReDesigner (Win32) game descriptor
/// </summary>
public sealed class GameDescriptor_RaymanRedesigner_Win32 : Win32GameDescriptor
{
    #region Public Properties

    public override string GameId => "RaymanRedesigner_Win32";
    public override Game Game => Game.RaymanRedesigner;
    public override GameCategory Category => GameCategory.Fan;
    public override LegacyGame? LegacyGame => Metro.LegacyGame.RaymanRedesigner;

    public override string DisplayName => "Rayman ReDesigner";
    public override string DefaultFileName => "Rayman ReDesigner.exe";
    public override DateTime ReleaseDate => new(2021, 02, 04);

    public override GameIconAsset Icon => GameIconAsset.RaymanRedesigner;

    #endregion

    #region Public Methods

    public override IEnumerable<GameUriLink> GetExternalUriLinks(GameInstallation gameInstallation) => new[]
    {
        new GameUriLink(
            Header: new ResourceLocString(nameof(Resources.GameDisplay_OpenGameJoltPage)),
            Uri: "https://gamejolt.com/games/Rayman_ReDesigner/539216",
            Icon: GenericIconKind.GameAction_Web)
    };

    public override IEnumerable<GamePurchaseLink> GetPurchaseLinks() => new GamePurchaseLink[]
    {
        new(new ResourceLocString(nameof(Resources.GameDisplay_GameJolt)), "https://gamejolt.com/games/Rayman_ReDesigner/539216", GenericIconKind.GameAction_Web),
    };

    #endregion
}