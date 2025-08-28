using RayCarrot.RCP.Metro.Games.Components;
using RayCarrot.RCP.Metro.Games.Structure;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman Lum'o'Grams Demo (Win32) game descriptor
/// </summary>
public sealed class GameDescriptor_RaymanLumoGramsDemo_Win32 : Win32GameDescriptor
{
    #region Constant Fields

    private const string GameJoltUrl = "https://gamejolt.com/games/Rayman-Lum-O-Grams/770177";

    #endregion

    #region Public Properties

    public override string GameId => "RaymanLumoGramsDemo_Win32";
    public override Game Game => Game.RaymanLumoGrams;
    public override GameCategory Category => GameCategory.Fan;

    public override LocalizedString DisplayName => "Rayman Lum'o'Grams Demo"; // TODO-LOC
    public override DateTime ReleaseDate => new(2025, 09, 01);

    public override GameIconAsset Icon => GameIconAsset.RaymanLumoGrams;
    public override GameBannerAsset Banner => GameBannerAsset.RaymanLumoGrams;

    #endregion

    #region Protected Methods

    protected override void RegisterComponents(IGameComponentBuilder builder)
    {
        base.RegisterComponents(builder);

        builder.Register<ExternalGameLinksComponent>(new GameJoltExternalGameLinksComponent(GameJoltUrl));
    }
    
    protected override ProgramInstallationStructure CreateStructure() => new DirectoryProgramInstallationStructure(new ProgramFileSystem(new ProgramPath[]
    {
        // Files
        new ProgramFilePath("Rayman Lum-o-Grams Demo.exe", ProgramPathType.PrimaryExe, required: true),
    }));

    #endregion

    #region Public Methods

    public override IEnumerable<GameAddAction> GetAddActions() => new GameAddAction[]
    {
        new LocateDirectoryGameAddAction(this),
    };

    public override IEnumerable<GamePurchaseLink> GetPurchaseLinks() => new GamePurchaseLink[]
    {
        new(new ResourceLocString(nameof(Resources.GameDisplay_GameJolt)), GameJoltUrl, GenericIconKind.GameAction_Web),
    };

    #endregion
}