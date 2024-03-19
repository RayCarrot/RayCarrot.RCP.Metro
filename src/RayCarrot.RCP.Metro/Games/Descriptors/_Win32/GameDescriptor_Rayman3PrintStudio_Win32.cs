using RayCarrot.RCP.Metro.Games.Components;
using RayCarrot.RCP.Metro.Games.Finder;
using RayCarrot.RCP.Metro.Games.Options;
using RayCarrot.RCP.Metro.Games.Structure;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman 3 Print Studio (Win32) game descriptor
/// </summary>
public sealed class GameDescriptor_Rayman3PrintStudio_Win32 : Win32GameDescriptor
{
    #region Public Properties

    public override string GameId => "Rayman3PrintStudio_Win32";
    public override string LegacyGameId => "PrintStudio";
    public override Game Game => Game.Rayman3PrintStudio;
    public override GameCategory Category => GameCategory.Other;

    public override LocalizedString DisplayName => new ResourceLocString(nameof(Resources.Rayman3PrintStudio_Win32_Title));
    public override DateTime ReleaseDate => new(2003, 01, 01); // Not exact

    public override GameIconAsset Icon => GameIconAsset.Rayman3PrintStudio;
    public override GameBannerAsset Banner => GameBannerAsset.Rayman3;

    #endregion

    #region Protected Methods

    protected override void RegisterComponents(IGameComponentBuilder builder)
    {
        base.RegisterComponents(builder);

        builder.Register(new GameOptionsComponent(x => new Rayman3PrintStudioGameOptionsViewModel(x)));
    }

    protected override ProgramInstallationStructure CreateStructure() => new DirectoryProgramInstallationStructure(new GameInstallationPath[]
    {
        // Files
        new GameInstallationFilePath("Autorun.exe", GameInstallationPathType.PrimaryExe, required: true),
    });

    #endregion

    #region Public Methods

    // Can only be downloaded
    public override IEnumerable<GameAddAction> GetAddActions() => new GameAddAction[]
    {
        new DownloadGameAddAction(this, new Uri[]
        {
            new(AppURLs.Games_PrintStudio1_Url),
            new(AppURLs.Games_PrintStudio2_Url),
        })
    };

    public override FinderQuery[] GetFinderQueries() => new FinderQuery[]
    {
        new PreviouslyDownloadedGameFinderQuery(GameId, LegacyGameId),
    };

    #endregion
}