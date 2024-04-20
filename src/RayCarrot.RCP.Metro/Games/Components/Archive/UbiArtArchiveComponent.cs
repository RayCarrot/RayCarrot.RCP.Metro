using BinarySerializer.UbiArt;
using RayCarrot.RCP.Metro.Archive;
using RayCarrot.RCP.Metro.Archive.UbiArt;

namespace RayCarrot.RCP.Metro.Games.Components;

[RequiredGameComponents(typeof(BinaryGameModeComponent), typeof(UbiArtPathsComponent))]
public class UbiArtArchiveComponent : ArchiveComponent
{
    public UbiArtArchiveComponent() : base(GetArchiveManager, GetArchivePaths, Id) { }

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

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

    private async Task RecreatedFileTableAsync(GameInstallation gameInstallation)
    {
        UbiArtPathsComponent paths = gameInstallation.GetRequiredComponent<UbiArtPathsComponent>();
        string globalFatFileName = paths.GlobalFatFile ?? throw new InvalidOperationException("A global fat file has to be specified");

        UbiArtGlobalFatManager globalFatManager = new(gameInstallation, paths.GameDataDirectory, globalFatFileName);

        using (LoadState state = await Services.App.LoaderViewModel.RunAsync(Resources.Archive_RecreatedUbiArtFileTableStatus, canCancel: true))
        {
            try
            {
                string[] bundleNames = paths.GetBundleNames(includePatch: false).ToArray();
                await Task.Run(() => globalFatManager.CreateFileAllocationTable(bundleNames, state.CancellationToken, state.SetProgress));

                await Services.MessageUI.DisplaySuccessfulActionMessageAsync(String.Format(
                    Resources.Archive_RecreatedUbiArtFileTableSuccess,
                    bundleNames.JoinItems(Environment.NewLine)));
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Recreating file table");

                await Services.MessageUI.DisplayExceptionMessageAsync(ex, Resources.Archive_RecreatedUbiArtFileTableError);
            }
        }
    }

    public override AdditionalArchiveAction? GetAdditionalAction()
    {
        if (GameInstallation.GetComponent<UbiArtPathsComponent>()?.GlobalFatFile == null)
            return null;

        return new AdditionalArchiveAction(
            new ResourceLocString(nameof(Resources.Archive_RecreatedUbiArtFileTable)),
            new ResourceLocString(nameof(Resources.Archive_RecreatedUbiArtFileTableInfo)),
            RecreatedFileTableAsync);
    }
}