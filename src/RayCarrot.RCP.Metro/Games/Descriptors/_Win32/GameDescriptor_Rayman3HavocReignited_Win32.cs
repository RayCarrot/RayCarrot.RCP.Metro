using RayCarrot.RCP.Metro.Games.Components;
using RayCarrot.RCP.Metro.Games.Structure;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman 3 Havoc Reignited (Win32) game descriptor
/// </summary>
public sealed class GameDescriptor_Rayman3HavocReignited_Win32 : Win32GameDescriptor
{
    #region Constant Fields

    private const string GameUrl = "https://r3hr.carrd.co";

    #endregion

    #region Public Properties

    public override string GameId => "Rayman3HavocReignited_Win32";
    public override Game Game => Game.Rayman3HavocReignited;
    public override GameCategory Category => GameCategory.Fan;

    public override LocalizedString DisplayName => "Rayman 3 Havoc Reignited"; // TODO-LOC
    public override DateTime ReleaseDate => new(2025, 09, 01);

    public override GameIconAsset Icon => GameIconAsset.Rayman3HavocReignited;
    public override GameBannerAsset Banner => GameBannerAsset.Rayman3HavocReignited;

    #endregion

    #region Protected Methods

    // ReSharper disable once RedundantOverriddenMember
    protected override void RegisterComponents(IGameComponentBuilder builder)
    {
        base.RegisterComponents(builder);
    }
    
    protected override ProgramInstallationStructure CreateStructure() => new DirectoryProgramInstallationStructure(new ProgramFileSystem(new ProgramPath[]
    {
        // Files
        new ProgramFilePath("Rayman3Remake.exe", ProgramPathType.PrimaryExe, required: true),
    }));

    #endregion

    #region Public Methods

    public override IEnumerable<GameAddAction> GetAddActions() => new GameAddAction[]
    {
        new LocateDirectoryGameAddAction(this),
    };

    public override IEnumerable<GamePurchaseLink> GetPurchaseLinks() => new GamePurchaseLink[]
    {
        // TODO-LOC
        new("Download from website", GameUrl, GenericIconKind.GameAction_Web),
    };

    #endregion
}