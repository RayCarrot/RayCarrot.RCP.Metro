using BinarySerializer.UbiArt;
using RayCarrot.RCP.Metro.Archive;
using RayCarrot.RCP.Metro.Archive.UbiArt;

namespace RayCarrot.RCP.Metro.Games.Components;

[RequiredGameComponents(typeof(BinaryGameModeComponent), typeof(UbiArtPathsComponent))]
public class UbiArtArchiveComponent : ArchiveComponent
{
    public UbiArtArchiveComponent() : base(GetArchiveManager, GetArchivePaths, Id) { }

    public new const string Id = "UBIART_IPK";

    private static IArchiveDataManager GetArchiveManager(GameInstallation gameInstallation)
    {
        UbiArtSettings settings = gameInstallation.
            GetRequiredComponent<BinaryGameModeComponent, UbiArtGameModeComponent>().
            GetSettings();

        return new UbiArtIPKArchiveDataManager(
            settings: settings,
            gameInstallation: gameInstallation,
            compressionMode: UbiArtIPKArchiveConfigViewModel.FileCompressionMode.WasCompressed);
    }

    private static IEnumerable<string> GetArchivePaths(GameInstallation gameInstallation)
    {
        UbiArtPathsComponent paths = gameInstallation.GetRequiredComponent<UbiArtPathsComponent>();

        string platformString = gameInstallation.
            GetRequiredComponent<BinaryGameModeComponent, UbiArtGameModeComponent>().
            GetSettings().
            PlatformString;

        foreach (string bundleName in paths.GetBundleNames(includePatch: true))
            yield return System.IO.Path.Combine(paths.GameDataDirectory, $"{bundleName}_{platformString}.ipk");
    }
}