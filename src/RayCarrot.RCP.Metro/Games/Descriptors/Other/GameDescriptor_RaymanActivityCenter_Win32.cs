using RayCarrot.RCP.Metro.Games.Structure;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman Activity Center (Win32) game descriptor
/// </summary>
public sealed class GameDescriptor_RaymanActivityCenter_Win32 : Win32GameDescriptor
{
    #region Public Properties

    public override string GameId => "RaymanActivityCenter_Win32";
    public override string LegacyGameId => "RaymanActivityCenter";
    public override Game Game => Game.RaymanActivityCenter;
    public override GameCategory Category => GameCategory.Other;

    public override LocalizedString DisplayName => "Rayman Activity Center";
    public override DateTime ReleaseDate => new(1999, 01, 01); // Not exact

    public override GameIconAsset Icon => GameIconAsset.RaymanActivityCenter;
    public override GameBannerAsset Banner => GameBannerAsset.RaymanActivityCenter;

    #endregion

    #region Protected Methods

    protected override GameInstallationStructure GetStructure() => new(new GameInstallationPath[]
    {
        // Files
        new GameInstallationFilePath("Rayman.exe", GameInstallationPathType.PrimaryExe, required: true),
    });

    #endregion
}