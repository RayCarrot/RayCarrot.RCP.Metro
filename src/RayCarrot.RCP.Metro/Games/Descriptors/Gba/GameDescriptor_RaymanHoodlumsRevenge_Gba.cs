using RayCarrot.RCP.Metro.Games.Components;
using RayCarrot.RCP.Metro.Games.Structure;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman Hoodlums' Revenge (GBA) game descriptor
/// </summary>
public sealed class GameDescriptor_RaymanHoodlumsRevenge_Gba : GbaGameDescriptor
{
    #region Public Properties

    public override string GameId => "RaymanHoodlumsRevenge_Gba";
    public override Game Game => Game.RaymanHoodlumsRevenge;
    public override GameCategory Category => GameCategory.Handheld;

    public override LocalizedString DisplayName => "Rayman Hoodlums' Revenge";
    public override string[] SearchKeywords => new[] { "rhr", "gba" };
    public override DateTime ReleaseDate => new(2005, 03, 17);

    public override GameIconAsset Icon => GameIconAsset.RaymanHoodlumsRevenge;
    public override GameBannerAsset Banner => GameBannerAsset.RaymanHoodlumsRevenge;

    #endregion

    #region Protected Methods

    protected override void RegisterComponents(IGameComponentBuilder builder)
    {
        base.RegisterComponents(builder);

        builder.Register(new RayMapComponent(RayMapComponent.RayMapViewer.Ray1Map, "RaymanHoodlumsRevengeEU", "gba_isometric/rhr_eu"));
    }

    protected override ProgramInstallationStructure GetStructure() => new GbaRomProgramInstallationStructure(new[]
    {
        new GbaRomLayout("EU", "HOODLUMS REV", "BRYP", "41"),
        new GbaRomLayout("US", "HOODLUMS REV", "BRYE", "41"),
    });

    #endregion

    #region Public Methods

    public override IEnumerable<GameAddAction> GetAddActions() => new GameAddAction[]
    {
        new LocateFileGameAddAction(this),
    };

    #endregion
}