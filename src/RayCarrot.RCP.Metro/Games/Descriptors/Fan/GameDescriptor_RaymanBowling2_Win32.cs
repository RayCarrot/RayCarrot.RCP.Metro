using RayCarrot.RCP.Metro.Games.Components;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman Bowling 2 (Win32) game descriptor
/// </summary>
public sealed class GameDescriptor_RaymanBowling2_Win32 : Win32GameDescriptor
{
    #region Constant Fields

    private const string GameJoltUrl = "https://gamejolt.com/games/rayman_bowling_2/532563";

    #endregion

    #region Public Properties

    public override string GameId => "RaymanBowling2_Win32";
    public override Game Game => Game.RaymanBowling2;
    public override GameCategory Category => GameCategory.Fan;
    public override LegacyGame? LegacyGame => Metro.LegacyGame.RaymanBowling2;

    public override LocalizedString DisplayName => "Rayman Bowling 2";
    public override string DefaultFileName => "Rayman Bowling 2.exe";
    public override DateTime ReleaseDate => new(2020, 09, 01);

    public override GameIconAsset Icon => GameIconAsset.RaymanBowling2;

    #endregion

    #region Protected Methods

    protected override void RegisterComponents(IGameComponentBuilder builder)
    {
        base.RegisterComponents(builder);

        builder.Register(new ProgressionManagersComponent(x => new GameProgressionManager_RaymanBowling2(x, "Rayman Bowling 2")));
        builder.Register<ExternalGameLinksComponent>(new GameJoltExternalGameLinksComponent(GameJoltUrl));
    }

    #endregion

    #region Public Methods

    public override IEnumerable<GamePurchaseLink> GetPurchaseLinks() => new GamePurchaseLink[]
    {
        new(new ResourceLocString(nameof(Resources.GameDisplay_GameJolt)), GameJoltUrl, GenericIconKind.GameAction_Web),
    };

    #endregion
}