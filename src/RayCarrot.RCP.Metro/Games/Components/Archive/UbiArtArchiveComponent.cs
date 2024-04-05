using System.IO;
using BinarySerializer.UbiArt;
using RayCarrot.RCP.Metro.Archive;
using RayCarrot.RCP.Metro.Archive.UbiArt;

namespace RayCarrot.RCP.Metro.Games.Components;

[RequiredGameComponents(typeof(BinaryGameModeComponent))]
public class UbiArtArchiveComponent : ArchiveComponent
{
    public UbiArtArchiveComponent(string gameDataDir) 
        : base(GetArchiveManager, x => GetArchivePaths(x, gameDataDir), Id)
    {
        GameDataDir = gameDataDir;
    }

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public new const string Id = "UBIART_IPK";

    public string GameDataDir { get; }

    private static UbiArtSettings GetSettings(GameInstallation gameInstallation)
    {
        BinaryGameModeComponent gameModeComponent = gameInstallation.GetRequiredComponent<BinaryGameModeComponent>();

        if (gameModeComponent.GameModeAttribute.GetSettingsObject() is not UbiArtSettings ubiArtSettings)
            throw new Exception($"The settings object provided by the corresponding game mode {gameModeComponent.GameMode} is not of the correct type");

        return ubiArtSettings;
    }

    private static IArchiveDataManager GetArchiveManager(GameInstallation gameInstallation)
    {
        UbiArtSettings settings = GetSettings(gameInstallation);

        return new UbiArtIPKArchiveDataManager(
            settings: settings,
            compressionMode: UbiArtIPKArchiveConfigViewModel.FileCompressionMode.WasCompressed);
    }

    private static IEnumerable<string> GetBundleNames(GameInstallation gameInstallation, string gameDataDir)
    {
        string platformString = gameInstallation.
            GetRequiredComponent<BinaryGameModeComponent>().
            GetRequiredSettings<UbiArtSettings>().
            PlatformString;

        foreach (FileSystemPath bundleFilePath in Directory.EnumerateFiles(gameInstallation.InstallLocation.Directory + gameDataDir,
                     $"*_{platformString}.ipk", SearchOption.TopDirectoryOnly))
        {
            string fileName = bundleFilePath.Name;
            yield return fileName.Substring(0, fileName.Length - (5 + platformString.Length));
        }
    }

    private static IEnumerable<string> GetArchivePaths(GameInstallation gameInstallation, string gameDataDir)
    {
        string platformString = gameInstallation.
            GetRequiredComponent<BinaryGameModeComponent>().
            GetRequiredSettings<UbiArtSettings>().
            PlatformString;

        foreach (string bundleName in GetBundleNames(gameInstallation, gameDataDir))
            yield return System.IO.Path.Combine(gameDataDir, $"{bundleName}_{platformString}.ipk");
    }

    private async Task RecreatedFileTableAsync(GameInstallation gameInstallation)
    {
        string globalFatFileName = gameInstallation.GetRequiredComponent<UbiArtGlobalFatComponent>().FileName;

        UbiArtGlobalFatManager globalFatManager = new(gameInstallation, GameDataDir, globalFatFileName);

        // TODO-LOC
        using (LoadState state = await Services.App.LoaderViewModel.RunAsync("Recreating file table", canCancel: true))
        {
            try
            {
                string[] bundleNames = GetBundleNames(gameInstallation, GameDataDir).ToArray();
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
        if (!GameInstallation.HasComponent<UbiArtGlobalFatComponent>())
            return null;

        return new AdditionalArchiveAction(
            "Recreate file table", // TODO-LOC
            "TODO: Info", // TODO-LOC
            RecreatedFileTableAsync);
    }
}