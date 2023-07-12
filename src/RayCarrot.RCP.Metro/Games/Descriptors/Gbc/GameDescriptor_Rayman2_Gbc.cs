using RayCarrot.RCP.Metro.Games.Components;
using RayCarrot.RCP.Metro.Games.Structure;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman 2 (GBC) game descriptor
/// </summary>
public sealed class GameDescriptor_Rayman2_Gbc : GbcGameDescriptor
{
    #region Public Properties

    public override string GameId => "Rayman2_Gbc";
    public override Game Game => Game.Rayman2_Gbc;
    public override GameCategory Category => GameCategory.Handheld;

    public override LocalizedString DisplayName => "Rayman 2"; // TODO-UPDATE: Localize
    public override string[] SearchKeywords => new[] { "r2", "gbc", "forever" };
    public override DateTime ReleaseDate => new(2001, 01, 01); // Not exact

    public override GameIconAsset Icon => GameIconAsset.Rayman2_Gbc;
    public override GameBannerAsset Banner => GameBannerAsset.Rayman2_Gbc;

    #endregion

    #region Protected Methods

    protected override void RegisterComponents(IGameComponentBuilder builder)
    {
        base.RegisterComponents(builder);

        builder.Register(new RayMapComponent(RayMapComponent.RayMapViewer.Ray1Map, "Rayman2GBCEU", "gbc/r2_eu"));
    }

    protected override ProgramInstallationStructure GetStructure() => new GbcRomProgramInstallationStructure(new[]
    {
        new GbcRomLayout("EU", "RAYMAN 2 TH", "BRYP", "41"),
        new GbcRomLayout("US", "RAYMAN2", "BRYE", "41"),
    });

    #endregion

    #region Public Methods

    public override IEnumerable<GameAddAction> GetAddActions() => new GameAddAction[]
    {
        new LocateFileGameAddAction(this),
    };

    #endregion
}