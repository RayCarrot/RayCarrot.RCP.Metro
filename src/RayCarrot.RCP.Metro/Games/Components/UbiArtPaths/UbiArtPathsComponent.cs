using System.IO;

namespace RayCarrot.RCP.Metro.Games.Components;

[BaseGameComponent]
[SingleInstanceGameComponent]
public class UbiArtPathsComponent : GameComponent
{
    public UbiArtPathsComponent(string gameDataDirectory, string? globalFatFile)
    {
        GameDataDirectory = gameDataDirectory;
        GlobalFatFile = globalFatFile;
    }

    public string GameDataDirectory { get; }
    public string? GlobalFatFile { get; }

    public IEnumerable<string> GetBundleNames()
    {
        string platformString = GameInstallation.
            GetRequiredComponent<BinaryGameModeComponent, UbiArtGameModeComponent>().
            GetSettings().
            PlatformString;

        foreach (FileSystemPath bundleFilePath in Directory.EnumerateFiles(GameInstallation.InstallLocation.Directory + GameDataDirectory,
                     $"*_{platformString}.ipk", SearchOption.TopDirectoryOnly))
        {
            string fileName = bundleFilePath.Name;
            yield return fileName.Substring(0, fileName.Length - (5 + platformString.Length));
        }
    }
}