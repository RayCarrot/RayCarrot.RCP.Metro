using RayCarrot.RCP.Metro.Games.Components;
using RayCarrot.RCP.Metro.Games.Structure;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman 3 (GBA) game descriptor
/// </summary>
public sealed class GameDescriptor_Rayman3_Gba : GbaGameDescriptor
{
    #region Public Properties

    public override string GameId => "Rayman3_Gba";
    public override Game Game => Game.Rayman3_Gba;
    public override GameCategory Category => GameCategory.Handheld;

    public override LocalizedString DisplayName => new ResourceLocString(nameof(Resources.Rayman3_Gba_Title));
    public override string[] SearchKeywords => new[] { "r3", "gba" };
    public override DateTime ReleaseDate => new(2003, 02, 21);

    public override GameIconAsset Icon => GameIconAsset.Rayman3_Gba;
    public override GameBannerAsset Banner => GameBannerAsset.Rayman3_Gba;

    #endregion

    #region Protected Methods

    protected override void RegisterComponents(IGameComponentBuilder builder)
    {
        base.RegisterComponents(builder);

        builder.Register(new ProgressionManagersComponent(x => new GameProgressionManager_Rayman3_Gba(x, "Rayman 3 - GBA")));
        builder.Register(new RayMapComponent(RayMapComponent.RayMapViewer.Ray1Map, "Rayman3GBAEU", "gba/r3_eu"));
        builder.Register(new InitializeContextComponent((_, c) => c.AddSettings(new R3GBA_Settings { IsPrototype = false })));

        builder.Register(new GameBananaGameComponent(19456));
    }

    protected override ProgramInstallationStructure CreateStructure() => new GbaRomProgramInstallationStructure(new[]
    {
        new GbaProgramLayout("EU", "RAYMAN 3", "AYZP", "41"),
        new GbaProgramLayout("US", "RAYMAN 3", "AYZE", "41"),
    });

    #endregion

    #region Public Methods

    public override IEnumerable<GameAddAction> GetAddActions() => new GameAddAction[]
    {
        new LocateFileGameAddAction(this),
    };

    #endregion
}