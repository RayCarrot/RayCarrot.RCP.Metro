using RayCarrot.RCP.Metro.Games.Components;
using RayCarrot.RCP.Metro.Games.Structure;
using RayCarrot.RCP.Metro.Games.Tools.RuntimeModifications;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman 1 (GBA) game descriptor
/// </summary>
public sealed class GameDescriptor_Rayman1_Gba : GbaGameDescriptor
{
    #region Public Properties

    public override string GameId => "Rayman1_Gba";
    public override Game Game => Game.Rayman1;
    public override GameCategory Category => GameCategory.Rayman;

    public override LocalizedString DisplayName => new ResourceLocString(nameof(Resources.Rayman1_Gba_Title));
    public override string[] SearchKeywords => new[] { "r1", "ray1", "ra", "gba" };
    public override DateTime ReleaseDate => new(2001, 06, 11);

    public override GameIconAsset Icon => GameIconAsset.RaymanAdvance;
    public override GameBannerAsset Banner => GameBannerAsset.Rayman1;

    #endregion

    #region Protected Methods

    protected override void RegisterComponents(IGameComponentBuilder builder)
    {
        base.RegisterComponents(builder);

        builder.Register(new ProgressionManagersComponent(x => new GameProgressionManager_RaymanAdvance_Gba(x, "Rayman Advance")));
        builder.Register(new RayMapComponent(RayMapComponent.RayMapViewer.Ray1Map, "RaymanAdvanceGBAEU", "r1/gba"));
        builder.Register<BinaryGameModeComponent>(new Ray1GameModeComponent(Ray1GameMode.Rayman1_GBA));

        builder.Register(new RuntimeModificationsGameManagersComponent(EmulatedPlatform.Gba, _ =>
            new[]
            {
                new Ray1GameManager(
                    displayName: new ResourceLocString(nameof(Resources.Mod_Mem_Game_R1_GBA_EU)),
                    getOffsetsFunc: () => Ray1MemoryData.Offsets_GBA_EU)
            }));
    }

    protected override ProgramInstallationStructure CreateStructure() => new GbaRomProgramInstallationStructure(new[]
    {
        new GbaRomLayout("EU", "RAYMAN", "ARYP", "41"),
        new GbaRomLayout("US", "RAYMAN", "ARYE", "41"),
    });

    #endregion

    #region Public Methods

    public override IEnumerable<GameAddAction> GetAddActions() => new GameAddAction[]
    {
        new LocateFileGameAddAction(this),
    };

    #endregion
}