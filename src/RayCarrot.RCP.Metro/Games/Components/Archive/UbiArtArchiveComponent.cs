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

        foreach (string bundleName in paths.GetBundleNames())
            yield return System.IO.Path.Combine(paths.GameDataDirectory, $"{bundleName}_{platformString}.ipk");
    }

    private async Task RecreatedFileTableAsync(GameInstallation gameInstallation)
    {
        UbiArtPathsComponent paths = gameInstallation.GetRequiredComponent<UbiArtPathsComponent>();
        string globalFatFileName = paths.GlobalFatFile ?? throw new InvalidOperationException("A global fat file has to be specified");

        UbiArtGlobalFatManager globalFatManager = new(gameInstallation, paths.GameDataDirectory, globalFatFileName);

        // TODO-LOC
        using (LoadState state = await Services.App.LoaderViewModel.RunAsync("Recreating file table", canCancel: true))
        {
            try
            {
                string[] bundleNames = paths.GetBundleNames().ToArray();
                await Task.Run(() => globalFatManager.CreateFileAllocationTable(bundleNames, state.CancellationToken, state.SetProgress));

                // TODO-LOC
                await Services.MessageUI.DisplaySuccessfulActionMessageAsync($"The file table was successfully recreated from the following bundles:\n\n{bundleNames.JoinItems(Environment.NewLine)}");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Recreating file table");

                // TODO-LOC
                await Services.MessageUI.DisplayExceptionMessageAsync(ex, "An error occurred when recreating the file table");
            }
        }
    }

    public override AdditionalArchiveAction? GetAdditionalAction()
    {
        if (GameInstallation.GetComponent<UbiArtPathsComponent>()?.GlobalFatFile == null)
            return null;

        return new AdditionalArchiveAction(
            "Recreate file table", // TODO-LOC
            "TODO: Info", // TODO-LOC
            RecreatedFileTableAsync);
    }
}