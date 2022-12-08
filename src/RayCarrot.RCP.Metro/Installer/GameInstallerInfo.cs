namespace RayCarrot.RCP.Metro;

/// <summary>
/// Info for the game installer
/// </summary>
public class GameInstallerInfo
{
    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="discFilesListFileName">The disc files list .txt file name</param>
    /// <param name="gameLogo">The game logo file name</param>
    /// <param name="gifFileNames">The .gif file names</param>
    public GameInstallerInfo(string discFilesListFileName, GameLogoAsset gameLogo, string[] gifFileNames)
    {
        DiscFilesListFileName = discFilesListFileName;
        GameLogo = gameLogo;
        GifFileNames = gifFileNames;
    }

    /// <summary>
    /// The disc files list .txt file name
    /// </summary>
    public string DiscFilesListFileName { get; }

    /// <summary>
    /// The game logo file name
    /// </summary>
    public GameLogoAsset GameLogo { get; }

    /// <summary>
    /// The .gif file names
    /// </summary>
    public string[] GifFileNames { get; }
}