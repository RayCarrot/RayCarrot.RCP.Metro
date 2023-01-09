using RayCarrot.RCP.Metro.Games.Structure;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman Premiers Clics (Win32) game descriptor
/// </summary>
public sealed class GameDescriptor_RaymanPremiersClics_Win32 : Win32GameDescriptor
{
    #region Public Properties

    public override string GameId => "RaymanPremiersClics_Win32";
    public override Game Game => Game.RaymanPremiersClics;
    public override GameCategory Category => GameCategory.Other;
    public override LegacyGame? LegacyGame => Metro.LegacyGame.RaymanPremiersClics;

    public override LocalizedString DisplayName => "Rayman Premiers Clics";
    public override DateTime ReleaseDate => new(2001, 01, 01); // Not exact

    public override GameIconAsset Icon => GameIconAsset.RaymanPremiersClics;
    public override GameBannerAsset Banner => GameBannerAsset.RaymanPremiersClics;

    #endregion

    #region Protected Methods

    protected override GameInstallationStructure GetStructure() => new(new GameInstallationPath[]
    {
        // Files
        new GameInstallationFilePath("RAYMAN.exe", GameInstallationPathType.PrimaryExe, required: true),
    });

    #endregion
}