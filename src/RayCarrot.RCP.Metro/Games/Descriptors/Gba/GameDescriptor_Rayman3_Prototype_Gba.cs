using RayCarrot.RCP.Metro.Games.Components;
using RayCarrot.RCP.Metro.Games.Structure;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman 3 Prototype (GBA) game descriptor
/// </summary>
public sealed class GameDescriptor_Rayman3_Prototype_Gba : GbaGameDescriptor
{
    #region Public Properties

    public override string GameId => "Rayman3_Prototype_Gba";
    public override Game Game => Game.Rayman3_Gba;
    public override GameCategory Category => GameCategory.Handheld;

    public override LocalizedString DisplayName => new ResourceLocString(nameof(Resources.Rayman3_Prototype_Gba_Title));
    public override string[] SearchKeywords => new[] { "r3", "gba", "beta" };
    public override DateTime ReleaseDate => new(2003, 02, 21); // Prototype is earlier, but we use same date as final game

    public override GameIconAsset Icon => GameIconAsset.Rayman3_Gba;
    public override GameBannerAsset Banner => GameBannerAsset.Rayman3_Gba;

    #endregion

    #region Protected Methods

    protected override void RegisterComponents(IGameComponentBuilder builder)
    {
        base.RegisterComponents(builder);

        builder.Register(new ProgressionManagersComponent(x => new GameProgressionManager_Rayman3_Gba(x, "Rayman 3 - GBA")));
        builder.Register(new RayMapComponent(RayMapComponent.RayMapViewer.Ray1Map, "Rayman3GBAUSPrototype", "gba/r3_us_proto"));
        builder.Register(new InitializeContextComponent((_, c) => c.AddSettings(new R3GBA_Settings { IsPrototype = true })));
    }

    protected override ProgramInstallationStructure GetStructure() => new GbaRomProgramInstallationStructure(new[]
    {
        new GbaRomLayout("US", "RAYMAN 3    ", "AYZE", "41"),
    });

    #endregion

    #region Public Methods

    public override IEnumerable<GameAddAction> GetAddActions() => new GameAddAction[]
    {
        new LocateFileGameAddAction(this),
    };

    #endregion
}