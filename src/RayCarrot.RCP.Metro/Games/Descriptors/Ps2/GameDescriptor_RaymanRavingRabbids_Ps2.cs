using RayCarrot.RCP.Metro.Games.Components;
using RayCarrot.RCP.Metro.Games.Structure;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman Raving Rabbids (PS2) game descriptor
/// </summary>
public sealed class GameDescriptor_RaymanRavingRabbids_Ps2 : Ps2GameDescriptor
{
    #region Public Properties

    public override string GameId => "RaymanRavingRabbids_Ps2";
    public override Game Game => Game.RaymanRavingRabbids;
    public override GameCategory Category => GameCategory.Rabbids;

    public override LocalizedString DisplayName => "Rayman Raving Rabbids"; // TODO-LOC
    public override string[] SearchKeywords => new[] { "rrr" };
    public override DateTime ReleaseDate => new(2006, 12, 05);

    public override GameIconAsset Icon => GameIconAsset.RaymanRavingRabbids;
    public override GameBannerAsset Banner => GameBannerAsset.RaymanRavingRabbids;

    #endregion

    #region Protected Methods

    protected override void RegisterComponents(IGameComponentBuilder builder)
    {
        base.RegisterComponents(builder);

        // TODO-UPDATE: Add progression support
        builder.Register<BinaryGameModeComponent>(new JadeGameModeComponent(JadeGameMode.RaymanRavingRabbids_PS2));
    }

    protected override ProgramInstallationStructure CreateStructure() => new Ps2DiscProgramInstallationStructure(new[]
    {
        new Ps2DiscProgramLayout("EU", new ProgramFileSystem(new ProgramPath[]
        {
            new ProgramFilePath("SLES_543.07;1", ProgramPathType.PrimaryExe, required: true),
            new ProgramDirectoryPath("DATA", ProgramPathType.Data)
            {
                new ProgramFilePath("DATA.BF;1", ProgramPathType.Data, required: true),
            }
        })),
        new Ps2DiscProgramLayout("US", new ProgramFileSystem(new ProgramPath[]
        {
            new ProgramFilePath("SLUS_215.76;1", ProgramPathType.PrimaryExe, required: true),
            new ProgramDirectoryPath("DATA", ProgramPathType.Data)
            {
                new ProgramFilePath("DATA.BF;1", ProgramPathType.Data, required: true),
            }
        })),
    });

    #endregion

    #region Public Methods

    public override IEnumerable<GameAddAction> GetAddActions() => new GameAddAction[]
    {
        new LocateFileGameAddAction(this),
    };

    #endregion
}