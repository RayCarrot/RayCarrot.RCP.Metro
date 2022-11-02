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
    /// <param name="gameLogoFileName">The game logo file name</param>
    /// <param name="gifFileNames">The .gif file names</param>
    public GameInstallerInfo(string discFilesListFileName, string gameLogoFileName, string[] gifFileNames)
    {
        DiscFilesListFileName = discFilesListFileName;
        GameLogoFileName = gameLogoFileName;
        GifFileNames = gifFileNames;
    }

    /// <summary>
    /// The disc files list .txt file name
    /// </summary>
    public string DiscFilesListFileName { get; }

    /// <summary>
    /// The game logo file name
    /// </summary>
    public string GameLogoFileName { get; }

    /// <summary>
    /// The .gif file names
    /// </summary>
    public string[] GifFileNames { get; }
}