using RayCarrot.RCP.Metro.Games.Finder;
using RayCarrot.RCP.Metro.Games.Structure;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rabbids Coding (Win32) game descriptor
/// </summary>
public sealed class GameDescriptor_RabbidsCoding_Win32 : Win32GameDescriptor
{
    #region Public Properties

    public override string GameId => "RabbidsCoding_Win32";
    public override Game Game => Game.RabbidsCoding;
    public override GameCategory Category => GameCategory.Rabbids;
    public override LegacyGame? LegacyGame => Metro.LegacyGame.RabbidsCoding;

    public override LocalizedString DisplayName => "Rabbids Coding";
    public override DateTime ReleaseDate => new(2019, 10, 08);

    public override GameIconAsset Icon => GameIconAsset.RabbidsCoding;

    #endregion

    #region Protected Methods

    protected override GameInstallationStructure GetStructure() => new(new GameInstallationPath[]
    {
        // Files
        new GameInstallationFilePath("Rabbids Coding.exe", GameInstallationPathType.PrimaryExe, required: true),
    });

    #endregion

    #region Public Methods

    public override IEnumerable<GamePurchaseLink> GetPurchaseLinks() => new GamePurchaseLink[]
    {
        new(new ResourceLocString(nameof(Resources.GameDisplay_DownloadUplay)), "https://register.ubisoft.com/rabbids-coding/")
    };

    public override FinderQuery[] GetFinderQueries() => new FinderQuery[]
    {
        new UninstallProgramFinderQuery("RabbidsCoding"),
        new UninstallProgramFinderQuery("Rabbids Coding"),

        new Win32ShortcutFinderQuery("Rabbids Coding"),
    };

    #endregion
}