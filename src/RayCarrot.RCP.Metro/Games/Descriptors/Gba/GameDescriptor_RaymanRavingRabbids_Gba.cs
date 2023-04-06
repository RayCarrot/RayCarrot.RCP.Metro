﻿using RayCarrot.RCP.Metro.Games.Components;
using RayCarrot.RCP.Metro.Games.Structure;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman Raving Rabbids (GBA) game descriptor
/// </summary>
public sealed class GameDescriptor_RaymanRavingRabbids_Gba : GbaGameDescriptor
{
    #region Public Properties

    public override string GameId => "RaymanRavingRabbids_Gba";
    public override Game Game => Game.RaymanRavingRabbids_Gba;
    public override GameCategory Category => GameCategory.Handheld;

    public override LocalizedString DisplayName => "Rayman Raving Rabbids (Game Boy Advance)";
    public override string[] SearchKeywords => new[] { "rrr", "gba" };
    public override DateTime ReleaseDate => new(2006, 11, 14);

    public override GameIconAsset Icon => GameIconAsset.RaymanRavingRabbids;
    public override GameBannerAsset Banner => GameBannerAsset.RaymanRavingRabbids_Gba;

    #endregion

    #region Protected Methods

    protected override void RegisterComponents(IGameComponentBuilder builder)
    {
        base.RegisterComponents(builder);

        builder.Register(new RayMapComponent(RayMapComponent.RayMapViewer.Ray1Map, "RaymanRavingRabbidsGBAEU", "gba_rrr/rrr_eu"));
    }

    protected override ProgramInstallationStructure GetStructure() => new GbaRomProgramInstallationStructure(new[]
    {
        new GbaRomLayout("EU", "RAYMAN4", "BQ3P", "41"),
        new GbaRomLayout("US", "RAYMAN4", "BQ3E", "41"),
    });

    #endregion

    #region Public Methods

    public override IEnumerable<GameAddAction> GetAddActions() => new GameAddAction[]
    {
        new LocateFileGameAddAction(this),
    };

    #endregion
}